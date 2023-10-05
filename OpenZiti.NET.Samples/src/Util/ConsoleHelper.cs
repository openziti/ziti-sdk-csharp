/*
Copyright NetFoundry Inc.

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
using System.Runtime.InteropServices;
using System.Text;

namespace OpenZiti.NET.Samples {
    public class ConsoleHelper {
        // credit for most of this code to https://gist.github.com/tomzorz/6142d69852f831fb5393654c90a1f22e
        private const int STD_OUTPUT_HANDLE = -11;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        private const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        public static void AllowAsciEscapeCodes() {
            var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
            if (!GetConsoleMode(iStdOut, out var outConsoleMode)) {
                Console.WriteLine("failed to get output console mode");
                Console.ReadKey();
                return;
            }

            outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;
            if (!SetConsoleMode(iStdOut, outConsoleMode)) {
                Console.WriteLine($"failed to set output console mode, error code: {GetLastError()}");
                Console.ReadKey();
                return;
            }
        }

        /// <summary>
        /// For whatever reason - writing perfectly fine bytes out to the console using c# is overly difficult.
        /// This method will convert the bytes to output, read lines from that string and write the strings out
        /// line by line. Why _this_ seems to be necessary is still unknown but this does seem to work.
        /// </summary>
        /// <param name="utf8bytes"></param>
        public static void OutputResponseToConsole(byte[] utf8bytes) {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && !RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                AllowAsciEscapeCodes();
            }

            var sr = new StringReader(Encoding.UTF8.GetString(utf8bytes));

            Console.WriteLine("");
            Console.WriteLine("Response is output between the asterisks: ");
            Console.WriteLine(new string('*', 125));
            var l = sr.ReadLine();
            while (l != null) {
                Console.WriteLine(l);
                l = sr.ReadLine();
            }
            Console.WriteLine(new string('*', 125));
        }
    }
}
