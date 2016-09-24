using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VastDocProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            String dir = @"D:\PROJECT_COLLABORATIVE_DESIGN_SPACE\VAST Challenge\VAST_Data_preprocessed\news-processed\";
            DocumentList mdir = new DocumentList();
            mdir.Load(dir);
            string fileName = "vast2006.preload";
            // This text is added only once to the file.

            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(fileName))
            {
                sw.Write("");
            }
            // Open the file to read from.
            using (StreamWriter sw = File.AppendText(fileName))
            {
                foreach (ProcessedDocument pd in mdir.GetList())
                {
                    sw.WriteLine(pd.ToJson());
                }
            }
            Console.ReadLine();
        }
    }
}
