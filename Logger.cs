using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace OrcaBotScheduledUpdate
{
    public sealed class Logger
    {
        private static Logger instance = null;
        private static readonly object padlock = new object();

        public enum MessageType
        {
            Verbose,
            Info,
            Warning,
            Error,
            Critical
        }
        public static Logger Instance {
            get {
                lock (padlock) {
                    if(instance == null) {
                        instance = new Logger();
                    }
                    return instance;
                }
            }
        }
        private bool createLog;
        private bool printVerbose;
        public String fileName;
        private Uri fileLocation;

        Logger() {

        }

        public void Init(Uri pathToFolder, bool _printVerbose, bool _createLog) {
            createLog = _createLog;
            printVerbose = _printVerbose;
            uint epoch = (uint)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            fileName = epoch.ToString();
            fileLocation = new Uri(Path.Combine(pathToFolder.AbsolutePath, fileName + ".log"));
            if (!createLog) {
                return;
            }
            if (!Directory.Exists(pathToFolder.AbsolutePath)) {
                Directory.CreateDirectory(pathToFolder.AbsolutePath);
            }
           
       


        }

        public void Write(string message,MessageType mt) {
            if (createLog) {
                WriteToFile(message, mt);
            }
            
            if(!printVerbose && mt == MessageType.Verbose) {
                return;
            }

            Console.WriteLine("[{0}] {1}", mt, message);
        }

        private void WriteToFile(string message,MessageType mt) {
            string messageToFile = String.Format("[{0}][{1}] {2}\n",DateTime.UtcNow.ToLongTimeString(),mt,message);
            using (StreamWriter tw = File.AppendText(fileLocation.AbsolutePath)) {
                tw.Write(messageToFile);
                tw.Close();
            }
        }



    }
}
