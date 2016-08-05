using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var aa = new CompareInfoConfig();
            aa.Names.Add("123");
            XMLSaveData<CompareInfoConfig>(aa, Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"1.xml"));
        }

        public static void XMLSaveData<T>(T data, string filePath) where T : class
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            if (data != null)
            {
                XMLSave(data, filePath);
            }
        }

        public static void XMLSave<T>(T data, string filePath) where T : class
        {
            var ser = new XmlSerializer(typeof(T));
            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.Unicode))
            {
                ser.Serialize(sw, data);
            }
        }
    }
    public class CompareInfoConfig
    {
        public CompareInfoConfig()
        {
            Names = new List<string>();
        }
        public List<string> Names { get; set; }
    }
}
