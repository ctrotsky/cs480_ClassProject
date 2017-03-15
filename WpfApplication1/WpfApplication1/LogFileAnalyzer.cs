using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
{
    class LogFileAnalyzer
    {
        public void Analyze(List<LogFileEntry> log, int maxFailures, TimeSpan timeWindow)
        {
            //timeWindow given in seconds.
            //Each time an authentication failure is encountered, the entry is added to the considered entries for that IP address.

            Dictionary<string, List<LogFileEntry>> consideredEntries = new Dictionary<string, List<LogFileEntry>>();  //Mapping of IP address to all of their LogFileEntries that are authentication failures.


            //Area around line 21420 looks sus. alejandro opens session for root

            Console.WriteLine("Analyzing log file..");

            for (int i = 0; i < log.Count; i++)
            {
                //Console.WriteLine("Analyzing line " + i);
                LogFileEntry entry = log[i];
                if (CheckRuleOne(entry, i) || CheckRuleThree(entry, i) || CheckRuleFour(entry, i)) // not currently counting Rule 2 because it is a result of many rule 1s
                {
                    List<LogFileEntry> consideredEntriesForIP;
                    if (!consideredEntries.TryGetValue(entry.sourceIP, out consideredEntriesForIP))
                    {
                        //No entries currently exist for this IP. Make a list of entries for this IP.
                        consideredEntriesForIP = new List<LogFileEntry>();
                        consideredEntries.Add(entry.sourceIP, consideredEntriesForIP);
                    }
                    //Add the LogFileEntry to this IP's list of considered entries.
                    consideredEntriesForIP.Add(entry);

                    //If its a multi auth failure, add it multiple times so it is weighted more.
                    if (entry.logType == LogFileEntry.LogType.MultiAuthFailure)
                    {                     
                        //Start at 1 because already added one
                        for (int j = 1; j < entry.numFailures; j++)
                        {
                            consideredEntriesForIP.Add(entry);
                        }
                    }
                }

                UpdateConsideredEntries(consideredEntries, maxFailures, timeWindow, entry.dateTime);
            }

            Console.WriteLine("Finished analyzing.");
        }

        //Rule 1 triggers if the entry is a failed password
        private bool CheckRuleOne(LogFileEntry entry, int index)
        {
            if (entry.logType == LogFileEntry.LogType.FailedPass)
            {
                return true;
            }

            return false;
        }

        //Rule 2 triggers if the user was disconnected for too many authentication failures.
        //Note: Might not want to count these entries since they are a result of other entries. Just count those other entries.
        private bool CheckRuleTwo(LogFileEntry entry, int index)
        {
            if (entry.logType == LogFileEntry.LogType.TooManyFailures)
            {
                return true;
            }

            return false;
        }

        //Rule 3 triggers if the entry is from a pam:unix authentication failure.
        private bool CheckRuleThree(LogFileEntry entry, int index)
        {
            if (entry.logType == LogFileEntry.LogType.AuthFailure)
            {
                return true;
            }

            return false;
        }

        //Rule 4 triggers if the entry is from a PAM log entry for multiple authentication failures.
        //Note: Current implementation weighs these failures by adding the same entry multiple times to considered entries. In the future should implement failure "weight" more properly.
        private bool CheckRuleFour(LogFileEntry entry, int index)
        {
            if (entry.logType == LogFileEntry.LogType.MultiAuthFailure)
            {
                return true;
            }

            return false;
        }

        private void UpdateConsideredEntries(Dictionary<string, List<LogFileEntry>> consideredEntries, int maxFailures, TimeSpan timeWindow, DateTime currentTime)
        {
            //After each LogFileEntry is analyzed, the considered entries are updated.
            //If an entry is in consideredEntries longer than the timeWindow, it is removed from consideredEntries
            //If a single IP address has more than maxFailures failures within the time window given by timeWindow, then all of its entries are marked as suspicious.

            foreach (KeyValuePair<string, List<LogFileEntry>> consideredIP in consideredEntries)
            {
                List<LogFileEntry> consideredEntriesForIP = consideredIP.Value;
                List<LogFileEntry> expiredEntriesForIP = new List<LogFileEntry>(); //the entries which are older than the timeWindow. Will be removed from the list.
                foreach (LogFileEntry entry in consideredEntriesForIP)
                {
                    //iterate through the list of considered entries for each considered IP address
                    TimeSpan difference = currentTime - entry.dateTime;

                    //if the entry is expired. (older than the timeWindow allows)
                    if (difference >= timeWindow)
                    {
                        //Add the entry to the list of expired entries to be removed.
                        expiredEntriesForIP.Add(entry);
                    }
                }

                //remove all expired entries
                foreach (LogFileEntry expiredEntry in expiredEntriesForIP)
                {
                    consideredEntriesForIP.Remove(expiredEntry);
                }

                //if an IP has more recent failed authentications than allowed by maxFailures
                if (consideredEntriesForIP.Count >= maxFailures)
                {
                    //Console.WriteLine("Too many failures for " + consideredIP.Key + " (" + consideredEntriesForIP.Count + ")");
                    foreach (LogFileEntry entry in consideredEntriesForIP)
                    {
                        entry.suspicious = true;
                    }
                }
            }
        }
    }
}
