using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace RemoteToLocalAttackDetection
{
    class Program
    {
        static void Main(string[] args)
        {
            List<LogFileEntry> log = DeSerializeObject("log.log");
            LogFileParser logFileReader = new LogFileParser();
            LogFileAnalyzer logFileAnalyzer = new LogFileAnalyzer();
            

            //logFileReader.Parse(@"C:\auths.log",log);

            //Currently checks for 30 or more failures in one minute
            logFileAnalyzer.Analyze(log, 30, new TimeSpan(0, 1, 0));  

            //Log now has entries marked as suspicious.
            //for (int i = 0; i < log.Count; i++)
            //{
            //    Console.WriteLine(log[i].suspicious);
            //}

            SerializeObject(log, "log.log");

            Console.WriteLine("Done.");
            Console.ReadLine();
        }

        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializableObject"></param>
        /// <param name="fileName"></param>
        public static void SerializeObject(List<LogFileEntry> serializableObject, string fileName)
        {
            if (serializableObject == null) { return; }

            try
            {
                using (Stream stream = File.Open(fileName, FileMode.Create))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    Console.WriteLine("Serializing object");
                    bin.Serialize(stream, serializableObject);
                    stream.Close();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        /// <summary>
        /// Deserializes an xml file into an object list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static List<LogFileEntry> DeSerializeObject(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { return new List<LogFileEntry>(); }

            List<LogFileEntry> objectOut = new List<LogFileEntry>();

            try
            {
                using (Stream stream = File.Open(fileName, FileMode.Open))
                {
                    BinaryFormatter bin = new BinaryFormatter();

                    Console.WriteLine("Deserializing object");
                    objectOut = (List<LogFileEntry>)bin.Deserialize(stream);
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return objectOut;
        }
    }
}
