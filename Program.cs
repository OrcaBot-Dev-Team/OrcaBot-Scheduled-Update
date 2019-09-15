using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;


namespace OrcaBotScheduledUpdate
{
    class Program
    {
        static Logger logger;
        static Options options;
        static Dictionary<string, int> SystemsDict;
        static void Main(string[] args) {
            //Set the Options for the Task
            
            var parserResult = Parser.Default.ParseArguments<Options>(args);
            if(parserResult.Tag == ParserResultType.NotParsed) {
                Console.WriteLine("Failed to parse one or more arguments:");
                var errors = ((NotParsed<Options>)parserResult).Errors;
                foreach(var error in errors) {
                    Console.WriteLine(error);
                    Console.ReadKey();
                    Environment.Exit(-1);
                }
            }
            else {
                options = ((Parsed<Options>)parserResult).Value;
                logger = new Logger(new Uri(new Uri(Environment.CurrentDirectory),"logs"), options.Verbose);
                try {
                    OptionsValidator ov = new OptionsValidator(options, logger);
                }
                catch(Exception e) {
                    HandleException(e, true);
                }
                if (options.Backup) {
                    var files = Directory.GetFiles(options.Path);
                    if (files.Length > 0) {
                        DirectoryInfo source = new DirectoryInfo(options.Path);
                        string destPath = new Uri(new Uri(Environment.CurrentDirectory), Path.Combine("backups" , logger.fileName)).AbsolutePath;
                        if (!Directory.Exists(destPath)) {
                            Directory.CreateDirectory(destPath);
                        }
                        DirectoryInfo destination = new DirectoryInfo(destPath);
                        //Copy each file (no recursion)
                        foreach (FileInfo fi in source.GetFiles()) {
                            fi.CopyTo(Path.Combine(destination.FullName, fi.Name), true);
                            logger.Write(String.Format("Copied {0} to {1}", fi.FullName, Path.Combine(destination.FullName, fi.Name)), Logger.MessageType.Verbose);
                        }

                    }
                    else {
                        logger.Write("BackUp flag set, but no files under path found, no backup created.", Logger.MessageType.Info);
                    }
                }
                
                //Now, download both required files, store them in the temp location
                try {
                    string stationsResponse;
                    string populatedSystemsResponse;
                    try {
                        logger.Write("Trying to download necessary files", Logger.MessageType.Verbose);
                        using (var wc = new System.Net.WebClient()) {

                            stationsResponse = wc.DownloadString(options.StationURL);
                            logger.Write("1/2 files done.", Logger.MessageType.Verbose);
                            populatedSystemsResponse = wc.DownloadString(options.PopulatedSystemsURL);
                            logger.Write("2/2 files done.", Logger.MessageType.Verbose);


                        }
                    }
                    catch {
                        logger.Write("Failed to download files", Logger.MessageType.Error);
                        throw;
                    }
                    logger.Write("Finished download...", Logger.MessageType.Info);
                    JSONParser.Parse(stationsResponse, populatedSystemsResponse);

                    
                }
                catch(Exception e) {
                    HandleException(e, true);
                }


            }

            

            
        }
        static void HandleException(Exception e,bool killApp = false) {
            logger.Write("An exception has been thrown. Please check the logs under " + (new Uri(new Uri(Environment.CurrentDirectory), "logs\t\t" + e.Message)), Logger.MessageType.Critical);
            if (killApp) {
                Console.ReadKey();
                Environment.Exit(-1);
            }
        }


    }
}
