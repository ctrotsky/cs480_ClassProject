﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteToLocalAttackDetection
{
    class LogFileParser {

        public LogFile Parse(String filename)
        {
            var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);         

            //start a thread that does and post message in gui saying it may take a while this cause it'll probably take a while
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                string line;
                LogFileEntry entry;
                LogFile log = new LogFile();
                int lineNum = 1;

                Console.WriteLine("Parsing log file..");

                while ((line = streamReader.ReadLine()) != null)
                {
                    try
                    {
                        entry = new LogFileEntry(line);
                        log.add(entry);
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
                return log;
            }
        }
    }

}
