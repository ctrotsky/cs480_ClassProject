using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            
            InitializeComponent();

            button.Click += button1Click;
        }

        private string openFile()
        {
            // Displays an OpenFileDialog so the user can select a Cursor.  
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Log Files|*.log";
            openFileDialog1.Title = "Select a Log File";

            string fileName = null;

            // Show the Dialog.  
            // If the user clicked OK in the dialog and  
            // a .log file was selected, open it.  
            if (openFileDialog1.ShowDialog() ?? false)
            {
                fileName = openFileDialog1.FileName;
            }

            return fileName;
        }

        private void displayInListView(List<LogFileEntry> log)
        {
            //Clear what's currently displayed
            this.Dispatcher.Invoke(() =>
            { 
                lvUsers.Items.Clear();
            });

            //Display each entry in the list view
            for (int i = 0; i < log.Count; i++)
            {
                LogFileEntry entry = log[i];
                string sus = "";
                if (entry.suspicious)
                {
                    sus = "X";
                }

                var row = new {EventNumber = i, Suspicious = sus, DateAndTime = entry.dateTime, SourceIP = entry.sourceIP, Message = entry.message };

                this.Dispatcher.Invoke(() =>
                {
                    lvUsers.Items.Add(row);
                    ListViewItem a = new ListViewItem();
                });
            }
        }

        private void button1Click(object sender, RoutedEventArgs e)
        {
            var row = new { Suspicious = "clicked" };
            lvUsers.Items.Add(row);
            string fileName = openFile();

            if (fileName!= null)
            {
                lvUsers.Items.Add(new { Suspicious = fileName });

                //Start this in a new thread so window doesn't freeze until done
                Task.Factory.StartNew(() => parseAndDisplayInListView(fileName));
            }
        }

        private void parseAndDisplayInListView(string fileName)
        {          
            List<LogFileEntry> log = parseAndAnalyze(fileName);
            displayInListView(log);

        }

        private List<LogFileEntry> parseAndAnalyze(string filename)
        {
            //List<LogFileEntry> log = DeSerializeObject("log.log");
            List<LogFileEntry> log = new List<LogFileEntry>();
            LogFileParser logFileReader = new LogFileParser();
            LogFileAnalyzer logFileAnalyzer = new LogFileAnalyzer();

            logFileReader.Parse(filename,log);

            //Currently checks for 30 or more failures in one minute
            int maxFailures = 30;
            int minutes = 1;
            this.Dispatcher.Invoke(() =>
            {
                bool failed;
                //if failed to parse, remains at default of 30 failures in 1 minute
                failed = Int32.TryParse(MaxFailuresTextBox.Text, out maxFailures);
                failed = Int32.TryParse(MinutesTextBox.Text, out minutes);
            });



            logFileAnalyzer.Analyze(log, maxFailures, new TimeSpan(0, minutes, 0));


            SerializeObject(log, "log.log");

            return log;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox2_Copy1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {

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

