using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningCards
{
    internal class Data
    {
        public static bool LoadData(string path, bool hasHeader, out List<Model> models, out string message)
        {
            models = new List<Model>();
            message = string.Empty;
            string line;
            bool isFirstLine = true;

            StreamReader file = new StreamReader(path);
            while ((line = file.ReadLine()) != null)
            {
                if (isFirstLine && hasHeader)
                {
                    isFirstLine = false;
                    continue;
                }

                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                string processedLine = processText(line);
                string[] values = processedLine.Split(new string[] { "\",\"" }, StringSplitOptions.None);

                if (values.Length != 5)
                {
                    message = $"the line {line} contains invalid count of data ({values.Length})";
                    return false;
                }

                models.Add(new Model()
                {
                    Content = values[0],
                    Location = values[1],
                    LTR = values[3].ToLower() == "ltr",
                    URL = new Uri(values[2]),
                    ImageLocation = values[4]
                });

                isFirstLine = false;
            }

            file.Close();

            return true;
        }

        private static string processText(string input)
        {
            if(input == string.Empty)
            {
                return input;
            }

            string output;

            if (input[0] == '"' && input[input.Length - 1] == '"')
            {
                output = input.Substring(1, input.Length - 2);
            }
            else
            {
                output = string.Copy(input);
            }

            output = output.Replace("\\r\\n", Environment.NewLine);
            output = output.Replace("\\n", Environment.NewLine);

            return output;
        }
    }
}
