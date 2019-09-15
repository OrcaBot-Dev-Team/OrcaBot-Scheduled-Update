using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace OrcaBotScheduledUpdate
{
    class Logger
    {
        public enum MessageType
        {
            Verbose,
            Info,
            Warning,
            Error,
            Critical
        }


        private bool printVerbose;
        public String fileName { get; }
        private Uri fileLocation;

        public Logger(Uri pathToFolder, bool _printVerbose) {
            printVerbose = _printVerbose;
            if (!Directory.Exists(pathToFolder.AbsolutePath)) {
                Directory.CreateDirectory(pathToFolder.AbsolutePath);
            }
            uint epoch = (uint)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            fileName = epoch.ToString() ;
            fileLocation = new Uri(Path.Combine(pathToFolder.AbsolutePath, fileName+".log"));
       


        }

        public void Write(string message,MessageType mt) {
            WriteToFile(message, mt);
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
