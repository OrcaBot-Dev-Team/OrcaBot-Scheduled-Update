using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CommandLine;


namespace OrcaBotScheduledUpdate
{
    class Options
    {
        [Option("stationUrl",Default = "https://www.edsm.net/dump/stations.json",HelpText ="The URL pointing to the json object containing station data.")]
        public string StationURL { get; set; }

        [Option("systemsUrl", Default = "https://eddb.io/archive/v6/systems_populated.json", HelpText = "The URL pointing to the json object containing all populated systems")]
        public string PopulatedSystemsURL { get; set; }

        [Option('v',"verbose",Default =false,HelpText = "Should there be verbose output?")]
        public bool Verbose { get; set; }

        [Option('b',"backup",Default =false,HelpText = "Should backups be made of the old files?")]
        public bool Backup { get; set; }

        [Option('l',"log",Default = false, HelpText = "Should a log be written?")]
        public bool Log { get; set; }

        [Option('p',"path",Required =true,HelpText = "The path where the processed files should be output to.")]
        public string Path { get; set; }

    }
    class OptionsValidator
    {
        public OptionsValidator(Options options,Logger logger) {
            logger.Write("Starting Parameter Validation", Logger.MessageType.Verbose);

            if (IsURLValid(options.StationURL)) {
                logger.Write("Station URL is valid: " + options.StationURL, Logger.MessageType.Verbose);
            }
            else {
                logger.Write("Station URL is not valid: " + options.StationURL, Logger.MessageType.Critical);
                throw new Exception("Invalid Station URL");
            }
            if (IsURLValid(options.PopulatedSystemsURL)) {
                logger.Write("Populated Systems URL is valid: " + options.PopulatedSystemsURL, Logger.MessageType.Verbose);
            }
            else {
                logger.Write("Populated Systems URL is not valid: " + options.PopulatedSystemsURL, Logger.MessageType.Critical);
                throw new Exception("Invalid Station URL");
            }
            if (IsValidDirectory(options.Path)) {
                logger.Write("Given path is valid: " + options.Path, Logger.MessageType.Verbose);
                if (!Directory.Exists(options.Path)) {
                    logger.Write("Directory does not exists. Creating directory " + options.Path, Logger.MessageType.Warning);
                    Directory.CreateDirectory(options.Path);
                }
                else {
                    logger.Write("Directory exists: " + options.Path, Logger.MessageType.Verbose);
                }
            }
            else {
                logger.Write("Given path is invalid, cannot proceed. " + options.Path, Logger.MessageType.Critical);
                throw new Exception("Given path invalid");
            }
        }

        public bool IsURLValid(string url) {
            Uri u;
            return Uri.TryCreate(url, UriKind.Absolute, out u) && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps);
        }
        public bool IsValidDirectory(string path) {
            try {
                Path.GetFullPath(path);
                return true;
            }
            catch {
                return false;
            }
            
           
        }
    }
    
}
