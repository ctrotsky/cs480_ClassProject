using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
{
    class LogFileParser {

        public void Parse(String filename, List<LogFileEntry> log)
        {
            var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);         

            //start a thread that does and post message in gui saying it may take a while this cause it'll probably take a while
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                string line;
                LogFileEntry entry;
                int lineNum = 1;

                Console.WriteLine("Parsing log file..");

                while ((line = streamReader.ReadLine()) != null)
                //for (int i = 0; i < 100; i++)
                {
                    try
                    {
                        entry = new LogFileEntry(line);
                        log.Add(entry);
                    }
                    catch (KeyNotFoundException e)
                    {
                        //Parse error in date. Month not found.
                        Console.WriteLine("KeyNotFoundException on line " + lineNum);
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        //Parse error. Probably end of file.
                        Console.WriteLine("IndexOutOfRangeException on line " + lineNum);
                        break;
                    }
                    lineNum++;
                }

                Console.WriteLine("Finished parsing.");
                return;
            }
        }
    }

}
