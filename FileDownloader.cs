using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;

namespace OrcaBotScheduledUpdate
{
    /// <summary>
    /// Downloads a file from the internet using gzip (if possible), places it in temp and returns the file path
    /// </summary>
    class FileDownloader
    {
        
        public string FilePath { get; }
        public FileDownloader(string path) {
            Logger.Instance.Write("Trying to download " + path, Logger.MessageType.Info);
            if( !Uri.TryCreate(path, UriKind.Absolute, out Uri u) && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps)) {
                throw new Exception("Given path is not a valid HTTP/HTTPS URL");
            }
            var request = WebRequest.CreateHttp(path);
            FilePath = Path.GetTempFileName();
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            using(var response = request.GetResponse()) {
                using(var stream = response.GetResponseStream())
                using(var fileStream = new FileStream(FilePath, FileMode.OpenOrCreate)) {
                    stream.CopyTo(fileStream);
                }
            }

        }
    }
}
