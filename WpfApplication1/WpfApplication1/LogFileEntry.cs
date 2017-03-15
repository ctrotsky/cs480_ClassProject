using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
{
    [Serializable]
    public class LogFileEntry
    {
        private static Dictionary<string, int> Months = new Dictionary<string, int> {
            {"Jan", 1 },
            {"Feb", 2 },
            {"Mar", 3 },
            {"Apr", 4 },
            {"May", 5 },
            {"Jun", 6 },
            {"Jul", 7 },
            {"Aug", 8 },
            {"Sep", 9 },
            {"Oct", 10 },
            {"Nov", 11 },
            {"Dec", 12 }
        };
        public enum LogType { FailedPass, TooManyFailures, AuthFailure, MultiAuthFailure, Other };      //if no logtype found, make misc

        public DateTime dateTime;
        public string systemName; //home
        public string applicationName; //sshd
        public int processID; //[xxxx], maybe instance number? idk about this one. all entries with same proccess id coming from same Ip. but same ip can have multiple process id. 
        public string message; //actual message part
        public LogType logType; //Type of Log

        //Following values are only for failed password attempts. Null for other message types
        public bool invalidUser = false;
        public string targetUser = null;
        public string sourceIP = null;
        public int sourcePort = -1;
        public string sourceApplication = null;


        public int numFailures = -1;    //only used for mulitple authentication failures in one line. Ex: "PAM 5 more authentication failures;"

        public bool suspicious = false;


        //Line #21416 has sudo command. No processId.


        //https://ubuntuforums.org/showthread.php?t=1580771

        public LogFileEntry()
        {

        }

        public LogFileEntry(string line)
        {
            string[] splitLine = line.Split(' ');

            dateTime = ParseDateTime(splitLine[0], splitLine[1], splitLine[2]);
            systemName = splitLine[3];
            applicationName = splitLine[4].Split('[')[0];

            //Parse processID
            try
            {
                processID = Int32.Parse(splitLine[4].Split('[')[1].Substring(0, (splitLine[4].Split('[')[1].Length - 2))); //this is disgusting
            }
            catch (FormatException e)
            {
                //Do something
                processID = -1;
            }
            catch(IndexOutOfRangeException e){
                //Do something
                processID = -1;
            }

            //Parse message
            for (int i = 5; i < splitLine.Length; i++)
            {
                message = message + splitLine[i] + " ";
            }

            logType = ParseLogType(message);
        }

        private LogType ParseLogType(string message)
        {
            LogType result = LogType.Other;
            if (message.Contains("Failed password"))
            {
                result = LogType.FailedPass;
                ParseFailedPassInfo(message);
            }
            else if (message.Contains("Too many authentication failures"))
            {
                result = LogType.TooManyFailures;
                ParseTooManyFailuresInfo(message);
            }
            else if (message.Contains("authentication failure;"))
            {
                //NOTE: NOT EVERY AUTHENTICATION FAILURE ENTRY WILL HAVE A TARGET USER
                result = LogType.AuthFailure;
                ParseAuthenticationFailureInfo(message);
            }
            else if (message.Contains("more authentication failures;"))
            {
                result = LogType.MultiAuthFailure;
                ParseAuthenticationFailureInfo(message);
                ParseNumFailures(message);
            }
            else
            {
                result = LogType.Other;
            }
            return result;
        }

        private DateTime ParseDateTime(string monthString, string dayString, string timeString)
        {
            int year = DateTime.Today.Year; //Set year equal to the current year since year information is not stored in given log files.
            int month = 0;
            int day = 0;
            int hour = 0;
            int min = 0;
            int sec = 0;

            try
            {
                month = Months[monthString];

                day = Int32.Parse(dayString);

                string[] timeSplit = timeString.Split(':');
                hour = Int32.Parse(timeSplit[0]);
                min = Int32.Parse(timeSplit[1]);
                sec = Int32.Parse(timeSplit[2]);

                
            } catch (FormatException e)
            {
                //Parse failed. not formatted correctly. do something.
            }
            return new DateTime(year, month, day, hour, min, sec);
        }

        private void ParseFailedPassInfo(string message)
        {
            string[] splitMessage = message.Split(' ');
            if (!splitMessage[3].Equals("invalid"))
            {
                //if attempting to log into valid user, targetUser is in segment 3 of message
                targetUser = splitMessage[3];
                sourceIP = splitMessage[5];
                sourcePort = Int32.Parse(splitMessage[7]);
                sourceApplication = splitMessage[8];             
            }
            else
            {
                //if attempting to log into invalid user, targetUser is in segment 5 of message
                invalidUser = true;
                targetUser = splitMessage[5];
                sourceIP = splitMessage[7];
                sourcePort = Int32.Parse(splitMessage[9]);
                sourceApplication = splitMessage[10];
            }
        }

        private void ParseTooManyFailuresInfo(string message)
        {
            string[] splitMessage = message.Split(' ');
            if (!splitMessage[6].Equals("invalid"))
            {
                //if attempting to log into valid user, targetUser is in segment 6 of message
                targetUser = splitMessage[6];
                sourceIP = splitMessage[8];
                sourcePort = Int32.Parse(splitMessage[10]);
                sourceApplication = splitMessage[11];
            }
            else
            {
                //if attempting to log into invalid user, targetUser is in segment 8 of message
                targetUser = splitMessage[8];
                sourceIP = splitMessage[10];
                sourcePort = Int32.Parse(splitMessage[12]);
                sourceApplication = splitMessage[13];
            }
        }

        private void ParseAuthenticationFailureInfo(string message)
        {
            //NOTE: NOT EVERY AUTHENTICATION FAILURE ENTRY WILL HAVE A TARGET USER
            string[] splitMessage = message.Split(' ');
            foreach (string segment in splitMessage)
            {
                if (segment.Contains("rhost="))
                {
                    sourceIP = segment.Split('=')[1];
                }
                if (segment.Length > 5 && segment.Substring(0,5).Equals("user="))
                {
                    targetUser = segment.Split('=')[1];
                }
            }
        }

        private void ParseNumFailures(string message)
        {
            numFailures = Int32.Parse(message.Split(' ')[1]);
        }

        public override string ToString()
        {
            return (dateTime.ToString() + " " + systemName + " " + applicationName + " " + processID + " " + message);
        }

    }


    //session openened = logged in
    //session closed = logged out OR wrong password entered too many times and it closed the session
    //ssh means remote access. not sure about bracket number yet

    //successful login goes over 3 lines. maybe keep that as one object
    //other things take multiple lines as well. look at timestamp for same milisecond
}
