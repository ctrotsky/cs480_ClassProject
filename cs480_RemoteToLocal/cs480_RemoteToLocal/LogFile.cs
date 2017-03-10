using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteToLocalAttackDetection
{
    [Serializable]
    class LogFile
    {
        DateTime date;
        List<LogFileEntry> logs = new List<LogFileEntry>();
        public int Count { get; private set; } = 0;

        //Store start date and end date?

            //need to fix ip address in authfailure for ones that are separated with - instead of .


        public LogFileEntry this[int i]
        {
            get
            {
                return logs[i];
            }
            set
            {
                logs[i] = value;
            }
        }

        public void add(LogFileEntry newEntry)
        {
            logs.Add(newEntry);
            Count++;
        }
    }

    


}
