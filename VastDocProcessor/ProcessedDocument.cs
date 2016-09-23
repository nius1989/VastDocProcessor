using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace VastDocProcessor
{
    class ProcessedDocument
    {
        Token[] list;
        Regex regex = new Regex("([\\s{}():;., \"“”])");
        string docID = "";
        string title = "";
        string time = "";
        string serializedProcessedDocument = "";
        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                title = value;
            }
        }

        public string Time
        {
            get
            {
                return time;
            }

            set
            {
                time = value;
            }
        }

        internal Token[] List
        {
            get
            {
                return list;
            }

            set
            {
                list = value;
            }
        }

        public string DocID
        {
            get
            {
                return docID;
            }

            set
            {
                docID = value;
            }
        }

        public string SerializedProcessedDocument
        {
            get
            {
                return serializedProcessedDocument;
            }

            set
            {
                serializedProcessedDocument = value;
            }
        }

        internal string ToJson()
        {
            string[] result = list.Select(t => t.ToJson()).ToArray();
            serializedProcessedDocument = String.Join("",result);          
            return JsonConvert.SerializeObject(this);
        }
        public ProcessedDocument(string docID, string filePath)
        {
            string plainText = System.IO.File.ReadAllText(filePath);
            this.docID = docID;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(plainText);
            List<Token> tokenList = new List<Token>();
            foreach (HtmlNode pnode in doc.DocumentNode.SelectNodes("p"))
            {
                var dnode = pnode.Descendants();
                HtmlNode previousNode=null;
                if (pnode.Attributes["id"].Value.Equals("1"))
                {
                    title = pnode.InnerText.Trim();
                }
                if (pnode.InnerText.IndexOf("Date Published to Web: ")>=0){
                    time = pnode.InnerText.Remove(0, "Date Published to Web: ".Length).Trim();
                }
                foreach (HtmlNode cnode in dnode)
                {                   
                    if (previousNode != null)
                    {
                        if (previousNode.InnerText.Equals(cnode.InnerText))
                        {
                            continue;
                        }
                    }
                    if (cnode.InnerText.Length > 0)
                    {
                        if (cnode.Name.Equals("enamex"))
                        {
                            ProcessName(cnode, tokenList);
                        }
                        else if (cnode.Name.Equals("timex"))
                        {
                            ProcessTime(cnode, tokenList);
                        }
                        else if (cnode.Name.Equals("numex"))
                        {
                            ProcessNumber(cnode, tokenList);
                        }
                        else
                        {
                            ProcessRegular(cnode, tokenList);
                        }
                    }
                    previousNode = cnode;
                }
                Token tk = new Token();
                tk.WordType = WordType.LINEBREAK;
                tk.OriginalWord = "\\n"; 
            }
            this.list = tokenList.ToArray();
        }

        private void ProcessRegular(HtmlNode cnode, List<Token> tokenList)
        {            
            string content = Regex.Replace(cnode.InnerText, @"[^\u0000-\u007F]+"," ");
            string stack = "";
            List<Token> tempList = new List<Token>();
            //Partition the content with the regular expression.
            for (int i = 0; i < content.Length; i++)
            {
                if (regex.IsMatch("" + content[i]))//Add a spliter
                {
                    Token tk = new Token();
                    tk.OriginalWord = "" + content[i];
                    tempList.Add(tk);
                    if (stack.Length > 0)
                    {
                        Token token = new Token();
                        token.OriginalWord = stack;
                        tempList.Add(token);
                        stack = "";
                    }
                }
                else//Add the letter to the word stack
                {
                    stack += content[i];
                }
            }
            if (stack.Length > 0)
            {
                Token token = new Token();
                token.OriginalWord = stack;
                tempList.Add(token);
            }
            foreach (Token tk in tempList)
            {
                tk.Process();
                //Console.WriteLine(tk.OriginalWord);
                tokenList.Add(tk);
            }
        }

        private void ProcessTime(HtmlNode cnode, List<Token> tokenList)
        {
            Token token = new Token();
            token.OriginalWord = Regex.Replace(cnode.InnerText, @"[^\u0000-\u007F]+", " ");
            token.StemmedWord = token.OriginalWord.ToLower();
            if (cnode.Attributes["type"].Value.Equals("DATE"))
            {
                token.WordType = WordType.DATE;
            }
            else if (cnode.Attributes["type"].Value.Equals("TIME"))
            {
                token.WordType = WordType.TIME;
            }
            else
            {
                token.WordType = WordType.IRREGULAR;
            }
            tokenList.Add(token);
        }
        private void ProcessNumber(HtmlNode cnode, List<Token> tokenList)
        {
            Token token = new Token();
            token.OriginalWord = Regex.Replace(cnode.InnerText, @"[^\u0000-\u007F]+", " ");
            token.StemmedWord = token.OriginalWord.ToLower();
            if (cnode.Attributes["type"].Value.Equals("MONEY"))
            {
                token.WordType = WordType.MONEY;
            }
            else
            {
                token.WordType = WordType.IRREGULAR;
            }
            tokenList.Add(token);
        }

        private void ProcessName(HtmlNode cnode, List<Token> tokenList)
        {
            Token token = new Token();
            token.OriginalWord = Regex.Replace(cnode.InnerText, @"[^\u0000-\u007F]+", " ");
            token.StemmedWord = token.OriginalWord.ToLower();
            if (cnode.Attributes["type"].Value.Equals("LOCATION"))
            {
                token.WordType = WordType.LOCACTION;
            }
            else if (cnode.Attributes["type"].Value.Equals("PERSON"))
            {
                token.WordType = WordType.PERSON;
            }
            else if (cnode.Attributes["type"].Value.Equals("ORGANIZATION"))
            {
                token.WordType = WordType.ORGANIZATION;
            }
            else
            {
                token.WordType = WordType.IRREGULAR;
            }
            tokenList.Add(token);
        }
    }
}
