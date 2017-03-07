using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteToLocalAttackDetection
{
    class Program
    {
        static void Main(string[] args)
        {
            LogFile log;
            LogFileParser logFileReader = new LogFileParser();
            LogFileAnalyzer logFileAnalyzer = new LogFileAnalyzer();
            

            log = logFileReader.Parse(@"C:\auths.log");

            //Currently checks for 30 or more failures in one minute
            logFileAnalyzer.Analyze(log, 30, new TimeSpan(0, 1, 0));  

            //Log now has entries marked as suspicious.
            for (int i = 0; i < log.Count; i++)
            {
                Console.WriteLine(log[i].suspicious);
            }

            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }
}
