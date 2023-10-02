using OpenZiti.NET.Tests;

namespace OpenZiti.TestProject {
    internal class Program {
        public static async Task Main(string[] args) {
            DataTests t = new DataTests();
            await t.TestWeatherAsync();
        }
    }
}
