using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArticleParser
{
    internal class FileProcessor
    {
        public static void Save(string directory, string text)
        {
            using (StreamWriter sw = new StreamWriter(File.Create(directory)))
            {
                sw.Write(text);
            }

            Environment.ExpandEnvironmentVariables("");
        }
    }
}
