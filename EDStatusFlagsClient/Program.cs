using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EDStatusFlagsClient
{
    class Program
    {
        static async Task Main()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var port = int.Parse(config["Port"]);
            var sleep = int.Parse(config["Sleep"]);

            var statusFilePath = GetPathToStatusFile(config);
            uint lastFlags = 0;

            await foreach (var value in GetFileUpdateAsync(statusFilePath,sleep))
            {
                var flags = GetStatusFlags(value);
                
                if (flags > 0 && flags != lastFlags)
                {
                    var bytes = ConvertFlagsToBytes(flags);

                    ConsoleLog($"Sending - {string.Join(",", bytes)}");

                    await SendDataAsync(IPAddress.Loopback, port, bytes);

                    lastFlags = flags;
                }
            }
        }
        
        private static async Task<bool> SendDataAsync(IPAddress ipAddress, int port, byte[] data)
        {
            using var client = new TcpClient();

            try
            {
                await client.ConnectAsync(ipAddress, port);

                using var networkStream = client.GetStream();

                await networkStream.WriteAsync(data, 0, data.Length);
                await networkStream.FlushAsync();

                networkStream.Close();

                return true;
            }
            catch(Exception e)
            {
                if (e.Message.Contains("No connection could be made"))
                {
                    ConsoleLog($"Is the Target script running?  Listening on port {port}?");
                }
                else
                {
                    ConsoleLog($"Error - {e.Message}");
                }
            }
            finally
            {
                client.Close();
            }
            return false;
        }
        
        private static async IAsyncEnumerable<string> GetFileUpdateAsync(string filePath,int sleep)
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var memoryStream = new MemoryStream();

            var lastValue = string.Empty;

            do
            {
                await Task.Delay(sleep);
                
                memoryStream.SetLength(0);

                fileStream.Seek(0, SeekOrigin.Begin);
                
                await fileStream.CopyToAsync(memoryStream);

                memoryStream.Seek(0, SeekOrigin.Begin);

                var bytes = new byte[memoryStream.Length];

                await memoryStream.ReadAsync(bytes, 0, bytes.Length);

                var value = Encoding.UTF8.GetString(bytes);

                if (!string.IsNullOrWhiteSpace(value) && !value.Equals(lastValue))
                {
                    yield return value;
                    lastValue = value;
                }
            } while (true);
        }

        private static byte[] ConvertFlagsToBytes(uint flags)
        {
            var data = BitConverter.GetBytes(flags);

            /*
             * The target script runtime requires that the data be 
             * prepended with two bytes containing the size of the 
             * message.
             */
            var head = BitConverter.GetBytes((ushort)(data.Length + 2));

            return head.Concat(data).ToArray();
        }

        private static uint GetStatusFlags(string value)
        {
            /*
             * The status.json file contains one json object
             * i.e. 
             * { 
             *     "timestamp":"2017-12-07T10:31:37Z", 
             *     "event":"Status", 
             *     "Flags":16842765, 
             *     "Pips":[2,8,2], 
             *     "FireGroup":0, 
             *     "Fuel":{ 
             *         "FuelMain":15.146626, 
             *         "FuelReservoir":0.382796 }, 
             *     "GuiFocus":5 
             * }
             *
             * Currently this program is only interested in 
             * the flags attribute.
             */
            try
            {
                var json = JObject.Parse(value);
                return json["Flags"].Value<uint>();
            }
            catch (Exception e)
            {
                ConsoleLog("Error");
                ConsoleLog(value);
                ConsoleLog(e.Message);
            }
            return 0;
        }

        private static string GetPathToStatusFile(IConfigurationRoot config)
        {
            /*
             * First check if the status file exists in the default 
             * location
             */
            var homeDrive = Environment.GetEnvironmentVariable("HOMEDRIVE");
            var homePath = Environment.GetEnvironmentVariable("HOMEPATH");

            var defaultPath =  $"{homeDrive}{homePath}{config["StatusFileRelativePath"]}";

            if (File.Exists(defaultPath))
            {
                return defaultPath;
            }

            /*
             * If the default location is not being used, set the config value
             * StatusFilePath to the fullpath including file name (in appconfig.json) 
             */

            if (!string.IsNullOrWhiteSpace(config["StatusFilePath"]) && File.Exists(config["StatusFilePath"]))
            {
                return config["StatusFilePath"];
            }

            throw new Exception("Path to status file could not be determined.  Use the appsettings value StatusFilePath to set manually.");
        }

        private static void ConsoleLog(string message) => Console.WriteLine($"{DateTime.Now:HH:mm:ss} - {message}");
    }
}
