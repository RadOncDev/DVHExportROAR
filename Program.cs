using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using ChuckDvhBatch;
using System.Diagnostics;
using System.IO;
using System.Globalization;

// TODO: Replace the following version attributes by creating AssemblyInfo.cs. You can do this in the properties of the Visual Studio project.
[assembly: AssemblyVersion("1.0.0.1")]
[assembly: AssemblyFileVersion("1.0.0.1")]
[assembly: AssemblyInformationalVersion("1.0")]

// TODO: Uncomment the following line if the script requires write access.
// [assembly: ESAPIScript(IsWriteable = true)]

namespace DVHExportROAR
{
    class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                // Using Standard Error output as a way to log/display information. Since Console.Writeline has been using to output DVH results.

                Console.Error.WriteLine("\nEntered Main() function.\n");

                var esapiApp = new EfficientEsapiApp(new EsapiApp());
                esapiApp.LogIn("SysAdmin", "SysAdmin");

                Console.Error.WriteLine("\nOpened EfficientEsapiApp.\n");

                var perPatientInput = new List<string>();

                CultureInfo cultureInfo = new CultureInfo("en-US");

                try
                {
                    // Check if an argument is provided
                    if (args.Length == 0)
                    {
                        throw new Exception("No input file provided.");
                    }

                    string filePath = args[0];

                    // Check if the file exists
                    if (!File.Exists(filePath))
                    {
                        throw new Exception("File does not exist.");
                    }

                    try
                    {
                        string[] lines = File.ReadAllLines(filePath);

                        string prevPatID = "";
                        string curCatStr = "";

                        foreach (string line in lines)
                        {
                            var patID = line.Split('\t')[0];

                            if(patID != prevPatID)
                            {
                                if(curCatStr != "")
                                {
                                    perPatientInput.Add(curCatStr);

                                    curCatStr = "";
                                }

                                prevPatID = patID;

                            }

                            curCatStr += line + "\n";
                        }

                        perPatientInput.Add(curCatStr);

                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"An error occurred when read input file: {ex.Message}");
                        throw;
                    }


                    //string inputText = "$ZAutoPlan_Prostate_02\t$AP\t$auto\tNA\t20\n101414412\t$AP2\t$auto4\tNA\t1";
                    //string inputText = "CAP-0003\tC2\t1_CTSim_defa3\tNA\t1";

                    for(int i =0; i < perPatientInput.Count; i++)
                    {
                        string inputText = perPatientInput[i];

                        Console.Error.WriteLine($"\n\n-----> start process patient ({i+1}/{perPatientInput.Count})\n{inputText}");

                        Input input;

                        try { 
                            input = Input.FromText(inputText);
                        }
                        catch(Exception ex)
                        {
                            Console.Error.WriteLine($"X --- An error occurred when process input for this patient: {ex.Message}");

                            continue;
                        }

                        var data = new AnalysisData(input);

                        var analysis = new DvhAnalysis(esapiApp, data);
                        analysis.Analyze();

                        Console.Error.WriteLine("Done processing patient");

                        using (var process = Process.GetCurrentProcess())
                            Console.Error.WriteLine($"Memory used by process: " +
                                $"{process.WorkingSet64.ToString("N0", cultureInfo)} (working set), " +
                                $"{process.PrivateMemorySize64.ToString("N0", cultureInfo)} (private)");
                    }

                    Console.ForegroundColor = ConsoleColor.Green;

                    Console.Error.WriteLine("\n\nScript finished successfully, type any key and press Enter to exit program.\n\n");

                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.Error.WriteLine(e.ToString());
                    Console.Error.WriteLine(e.Message);

                    Console.Error.WriteLine("\n\nScript stopped with the above exception.\n\n");


                }
                finally
                {
                    Console.Error.WriteLine("Logging out of ESAPI");
                    esapiApp.LogOut();
                }


            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                Console.Error.WriteLine(e.Message);

                Console.Error.WriteLine("\n\nScript stopped with the above exception.\n\n");
            }

            //Console.ReadLine();

        }
    }
}
