using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using ChuckDvhBatch;
using System.Diagnostics;

// TODO: Replace the following version attributes by creating AssemblyInfo.cs. You can do this in the properties of the Visual Studio project.
[assembly: AssemblyVersion("1.0.0.1")]
[assembly: AssemblyFileVersion("1.0.0.1")]
[assembly: AssemblyInformationalVersion("1.0")]

// TODO: Uncomment the following line if the script requires write access.
// [assembly: ESAPIScript(IsWriteable = true)]

namespace BasicStandAlone
{
    class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            Console.Error.WriteLine("\nEntered Main() function.\n");

            try
            {
                Execute();

                Console.ForegroundColor = ConsoleColor.Green;

                Console.Error.WriteLine("\n\nScript finished successfully, type any key and press Enter to exit program.\n\n");

                //Console.ReadLine();

            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.Error.WriteLine(e.ToString());

                //Console.ReadLine();

            }
        }




        static void Execute()
        {
            var esapiApp = new EfficientEsapiApp(new EsapiApp());
            esapiApp.LogIn("SysAdmin", "SysAdmin");


            //string inputText = "$ZAutoPlan_Prostate_02\t$AP\t$auto\tNA\t20\n101414412\t$AP2\t$auto4\tNA\t1";
            string inputText = "CAP-0003\tC2\t1_CTSim_defa3\tNA\t1";


            var input = Input.FromText(inputText);
            
            var data = new AnalysisData(input);

            var analysis = new DvhAnalysis(esapiApp, data);
            analysis.Analyze();

            Console.Error.WriteLine("Done processing input");

            using (var process = Process.GetCurrentProcess())
                Console.Error.WriteLine($"Memory used by process: " +
                    $"{process.WorkingSet64} (working set), " +
                    $"{process.PrivateMemorySize64} (private)");

        }
    }
}
