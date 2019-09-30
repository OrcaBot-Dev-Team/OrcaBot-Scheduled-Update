using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using syshelp = Orcabot.Helpers.SystemHelper;
namespace OrcaBotScheduledUpdate
{
    class Program
    {
        static Options options;

        static void Main(string[] args) {
            //Set the Options for the Task
            
            options = ParseOptions(args);
            Logger.Instance.Init(new Uri(new Uri(Environment.CurrentDirectory), "logs"), options.Verbose, options.Log);
            try {
                OptionsValidator ov = new OptionsValidator(options);
            }
            catch(Exception e) {
                HandleException(e, true);
            }
            if (options.Backup) {
                CreateBackUp(); 
            }
                
            //Now, download both required files, store them in the temp location
            try {
                string stationsFile;
                string populatedFile;
                try {
                    stationsFile = new FileDownloader(options.StationURL).FilePath; 
                    populatedFile = new FileDownloader(options.PopulatedSystemsURL).FilePath;
                }
                catch {
                    Logger.Instance.Write("Failed to download files", Logger.MessageType.Error);
                    throw;
                }
                Logger.Instance.Write("Finished download...", Logger.MessageType.Info);
                var dict = JSONParser.Parse(stationsFile, populatedFile);
                Export(dict,"populatedSystemsWithStations");
                Export(FilterMaterialTraders(dict), "systemsWithMaterialTraders");
                Logger.Instance.Write("The program has successfully reached its end. Press any key to exit...", Logger.MessageType.Info);
                Environment.Exit(0);

            }
            catch(OutOfMemoryException e) {
                HandleException(e, true);
            }

 
        }
        private static Options ParseOptions(string[] args) {
            var parserResult = Parser.Default.ParseArguments<Options>(args);
            if (parserResult.Tag == ParserResultType.NotParsed) {
                Console.WriteLine("Failed to parse one or more arguments:");
                var errors = ((NotParsed<Options>)parserResult).Errors;
                foreach (var error in errors) {
                    Console.WriteLine(error);
                 
                }
                Console.ReadKey();
                Environment.Exit(-1);
                return null;
            }
            else {
               return ((Parsed<Options>)parserResult).Value;
            }
        }


        private static void CreateBackUp() {
            var files = Directory.GetFiles(options.Path);
            if (files.Length > 0) {
                DirectoryInfo source = new DirectoryInfo(options.Path);
                string destPath = new Uri(new Uri(Environment.CurrentDirectory), Path.Combine("backups", Logger.Instance.fileName)).AbsolutePath;
                if (!Directory.Exists(destPath)) {
                    Directory.CreateDirectory(destPath);
                }
                DirectoryInfo destination = new DirectoryInfo(destPath);
                //Copy each file (no recursion)
                foreach (FileInfo fi in source.GetFiles()) {
                    fi.CopyTo(Path.Combine(destination.FullName, fi.Name), true);
                    Logger.Instance.Write(String.Format("Copied {0} to {1}", fi.FullName, Path.Combine(destination.FullName, fi.Name)), Logger.MessageType.Verbose);
                }

            }
            else {
                Logger.Instance.Write("BackUp flag set, but no files under path found, no backup created.", Logger.MessageType.Info);
            }
        }

     

        private static void Export(Dictionary<string,Orcabot.Types.System> dictionary,string filename) {
            //Generate a JSON out of the dict
            {
                string json = JSONParser.Stringify(dictionary);
                File.WriteAllText(Path.Combine(options.Path, $"{filename}.json"), json);
                Logger.Instance.Write("Finished creating output json. It can be found at " + Path.Combine(options.Path, $"{filename}.json"), Logger.MessageType.Info);
            }         
        }
        private static Dictionary<string,Orcabot.Types.System> FilterMaterialTraders(Dictionary<string,Orcabot.Types.System> dict) {
            var retDict = new Dictionary<string, Orcabot.Types.System>();
            foreach (var entry in dict) {
                 if (syshelp.HasMatTrader(entry.Value)){
                    retDict.Add(entry.Key,entry.Value);
                 }
            }
            return retDict;
        }

        static void HandleException(Exception e,bool killApp = false) {
            Logger.Instance.Write("An exception has been thrown. Please check the logs under " + (new Uri(new Uri(Environment.CurrentDirectory), "logs\t\t" + e.Message)), Logger.MessageType.Critical);
            if (killApp) {
                Console.ReadKey();
                Environment.Exit(-1);
            }
        }


    }
}
