using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.IO;
namespace Data
{
    public class PList : Dictionary<string, object>
    {
        public PList()
        {
        }

        public PList(string file)
        {
            Load(file);
        }

        public void Load(string file)
        {
            Clear();

            XDocument doc = XDocument.Load(file);
            XElement plist = doc.Element("plist");
            XElement dict = plist.Element("dict");

            var dictElements = dict.Elements();
            Parse(this, dictElements);
        }

        public void LoadText(string text)
        {
            Clear();
            XDocument doc = XDocument.Parse(text);
            XElement plist = doc.Element("plist");
            XElement dict = plist.Element("dict");

            var dictElements = dict.Elements();
            Parse(this, dictElements);
        }

        private void Parse(PList dict, IEnumerable<XElement> elements)
        {
            for (int i = 0; i < elements.Count(); i += 2)
            {
                XElement key = elements.ElementAt(i);
                XElement val = elements.ElementAt(i + 1);

                dict[key.Value] = ParseValue(val);
            }
        }

        private List<object> ParseArray(IEnumerable<XElement> elements)
        {
            List<dynamic> list = new List<dynamic>();
            foreach (XElement e in elements)
            {
                object one = ParseValue(e);
                list.Add(one);
            }

            return list;
        }

        private object ParseValue(XElement val)
        {
            switch (val.Name.ToString())
            {
                case "string":
                    return val.Value;
                case "integer":
                    return int.Parse(val.Value);
                case "real":
                    return float.Parse(val.Value);
                case "true":
                    return true;
                case "false":
                    return false;
                case "dict":
                    PList plist = new PList();
                    Parse(plist, val.Elements());
                    return plist;
                case "array":
                    List<object> list = ParseArray(val.Elements());
                    return list;
                default:
                    throw new ArgumentException("Unsupported");
            }
        }
    }
}