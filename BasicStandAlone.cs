using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
//using ChuckDvhBatch;

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
            Console.WriteLine("\nEntered Main() function.\n");

            try
            {
                using (Application app = Application.CreateApplication())
                {

                    if(app != null)
                    {
                        Console.WriteLine("\nApplication object created successfully.\n");
                    }
                    else
                    {
                        throw new Exception("\nApplication object creation failed.\n");
                    }

                    Execute(app);
                }

                Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine("\n\nScript finished successfully, type any key and press Enter to exit program.\n\n");

                Console.ReadLine();

            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.Error.WriteLine(e.ToString());

                Console.ReadLine();

            }
        }




        static void Execute(Application app)
        {
            Console.WriteLine("\nEntered Execute() here.\n");

            string patientID = "101414412";
            //string patientID = "CAP-0003";

            var patient1 = app.OpenPatientById(patientID);

            if(patient1 != null)
            {
                Console.WriteLine($"\nPatient {patientID} opened successfully.\n");
            }
            else
            {
                throw new Exception($"\nPatient {patientID} open failed.\n");
            }

            var n_courses = patient1.Courses.Count();

            Console.WriteLine($"\n{n_courses} courses found in opened patient.\n");
        }

        
    }
}
