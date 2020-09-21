/*
Copyright 2019-2020 NetFoundry, Inc.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

https://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.IO;
using System.Text;

namespace OpenZiti.Samples
{
    public class Weather
    {
        static MemoryStream ms = new MemoryStream(2 << 16); //a big bucket to hold bytes to display contiguously at the end of the program
        public static void Run(string[] args)
        {
            CheckUsage(args);
            ZitiOptions opts = new ZitiOptions();
            opts.InitComplete = ZitiUtil.CheckStatus;
            opts.ServiceChange = onServiceChange;
            opts.ConfigFile = args[0];
            opts.ServiceRefreshInterval = 15;
            opts.Context = args[2];
            ZitiIdentity id = new ZitiIdentity(opts);
            API.Run();
        }

        private static void onServiceChange(ZitiContext zitiContext, ZitiService service, ZitiStatus status, int flags, object serviceContext)
        {
            if (service.Name == serviceContext.ToString())
            {
                service.Dial(onConnected, onData);
            }
        }
        private static void onConnected(ZitiConnection connection, ZitiStatus status)
        {
            ZitiUtil.CheckStatus(status);

            Console.WriteLine("sending HTTP request: " + connection.ConnectionContext);

            byte[] wttrRequestAsBytes = Encoding.UTF8.GetBytes("GET / HTTP/1.0\r\n"
                                                               + "Accept: *-/*\r\n"
                                                               + "Connection: close\r\n"
                                                               + "User-Agent: curl/7.59.0\r\n"
                                                               + "Host: wttr.in\r\n"
                                                               + "\r\n");

            connection.Write(wttrRequestAsBytes, afterDataWritten, "write context");
        }

        private static void afterDataWritten(ZitiConnection connection, ZitiStatus status, object context)
        {
            ZitiUtil.CheckStatus(status);
        }

        private static void onData(ZitiConnection connection, ZitiStatus status, byte[] data)
        {
            if (status == ZitiStatus.OK)
            {
                ms.Write(data); //collect all the bytes to display contiguously at the end of the program
            }
            else
            {
                if (status == ZitiStatus.EOF)
                {
                    Console.WriteLine("request completed: " + status.GetDescription());
                }
                else
                {
                    Console.WriteLine("unexpected error: " + status.GetDescription());
                }
                ConsoleHelper.OutputResponseToConsole(ms.ToArray());
                connection.Close();
                Environment.Exit(0);
            }
        }

        public static void CheckUsage(string[] args)
        {
            if (args.Length < 3)
            {
                string appname = System.AppDomain.CurrentDomain.FriendlyName;
                Console.WriteLine("Usage:");
                Console.WriteLine($"\t{appname} {args[0]} {args[1]} <service-name>");
                throw new ArgumentException("too few arguments");
            }
        }
    }
}
