using System;
using System.IO;
using System.Linq;

namespace ChuckDvhBatch
{
    public class Input
    {
        private const int NumberOfFields = 5;
        private const char InputFieldSeparator = '\t';

        public Input(string path)
        {
            Validate(path);
            Initialize(path);
        }

        private Input() { }

        public static Input FromText(string data)
        {
            var input = new Input();
            input.InitializeFromText(data);
            return input;
        }

        public InputData[] Data { get; private set; }

        private void Validate(string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException("Input path does not exist");
        }

        private void Initialize(string path)
        {
            Data = File.ReadAllLines(path).Select(CreateInputData).ToArray();
        }

        private void InitializeFromText(string data)
        {
            Data = data.Split('\n').Where(IsNotEmpty).Select(CreateInputData).Where(t => !string.IsNullOrEmpty(t.PlanSetupId)).ToArray();

            //Console.Error.WriteLine($"{Data.Count()}  --  planningItems for this patient from input");
        }

        private bool IsNotEmpty(string s) => !string.IsNullOrEmpty(s);

        private InputData CreateInputData(string line)
        {
            var tokens = line.Split(InputFieldSeparator);
            Validate(tokens);

            return new InputData
            {
                PatientId = tokens[0],
                CourseId = tokens[1],
                PlanSetupId = tokens[2],
                PlanSetupUid = tokens[3],
                FractionsDelivered = Convert.ToInt32(tokens[4])
            };
        }

        private void Validate(string[] tokens)
        {
            if (tokens.Length != NumberOfFields)
                throw new InvalidOperationException($"Input line does not have {NumberOfFields} fields. It has [{tokens.Length}]. First field is [{tokens[0]}]");
        }
    }
}
