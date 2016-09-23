using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VastDocProcessor
{
    class DocumentList
    {
        Dictionary<string, ProcessedDocument> xmlFileList = new Dictionary<string, ProcessedDocument>();
        Dictionary<string, string> entityList = new Dictionary<string, string>();
        internal void Load(String dir) {
            string[] fileEntries = Directory.GetFiles(dir);
            foreach (string fileName in fileEntries) {
                string ext = Path.GetExtension(fileName);
                if (ext.Equals(".NE")) {
                    ProcessedDocument doc = new ProcessedDocument(GetID(fileName),fileName);
                    Console.WriteLine(GetID(fileName));
                    xmlFileList.Add(GetID(fileName), doc);

                }
                if (ext.Equals(".NORMALIZED_ENTITY")) {
                    entityList.Add(GetID(fileName),fileName);
                }
            }
        }
        internal ProcessedDocument[] GetList() {
            return xmlFileList.Values.ToArray();
        }
        private string GetID(string dir) {
            string[] strs = Path.GetFileNameWithoutExtension(dir).Split('.');
            return strs[0];
;        }
    }
}
