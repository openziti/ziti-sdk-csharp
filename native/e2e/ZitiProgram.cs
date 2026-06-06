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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace E2ETest;

// Finds and runs native/e2e-app (the ziti client+server) as child processes: `e2e-app host ...` and
// `e2e-app dial ...`. It P/Invokes the fresh ziti4dotnet lib, so the test proves the C SDK works in .NET as
// two real processes on whatever OS runs it. The dll path comes from E2E_APP_DLL (set by run-e2e-test.ps1);
// when unset (local `dotnet test`) it is found under native/e2e-app/bin relative to the repo.
internal static class ZitiProgram
{
    public static string ResolveApp() => Resolve("E2E_APP_DLL", "e2e-app", "e2e-app");

    private static string Resolve(string envVar, string projectDir, string assemblyName)
    {
        var fromEnv = Environment.GetEnvironmentVariable(envVar);
        if (!string.IsNullOrEmpty(fromEnv))
        {
            if (!File.Exists(fromEnv)) throw new FileNotFoundException($"{envVar} points at a missing file", fromEnv);
            return fromEnv;
        }

        // Fallback for local runs: walk up to the repo 'native' folder, then take the newest built program dll.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "native"))) dir = dir.Parent;
        if (dir == null) throw new InvalidOperationException($"could not find repo 'native' dir to locate {assemblyName}.dll; set {envVar}.");

        var binDir = Path.Combine(dir.FullName, "native", projectDir, "bin");
        if (!Directory.Exists(binDir))
            throw new DirectoryNotFoundException($"{projectDir} is not built ({binDir} missing). Build it or set {envVar}.");

        var dll = Directory.GetFiles(binDir, assemblyName + ".dll", SearchOption.AllDirectories)
            .OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
        if (dll == null) throw new FileNotFoundException($"could not find {assemblyName}.dll under {binDir}; set {envVar}.");
        return dll;
    }

    // A long-running child (the server). Stdout/stderr are captured; once ReadyMarker is seen on stdout,
    // WaitForReadyAsync completes.
    public sealed class Handle : IDisposable
    {
        private readonly Process _proc;
        private readonly StringBuilder _out = new();
        private readonly TaskCompletionSource<bool> _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Handle(string dll, string readyMarker, string[] args)
        {
            _proc = NewDotnet(dll, args);
            _proc.OutputDataReceived += (_, e) =>
            {
                if (e.Data == null) return;
                lock (_out) _out.AppendLine(e.Data);
                if (e.Data.Contains(readyMarker)) _ready.TrySetResult(true);
            };
            _proc.ErrorDataReceived += (_, e) => { if (e.Data != null) lock (_out) _out.AppendLine(e.Data); };
            _proc.Start();
            _proc.BeginOutputReadLine();
            _proc.BeginErrorReadLine();
        }

        public async Task<bool> WaitForReadyAsync(TimeSpan timeout)
            => await Task.WhenAny(_ready.Task, Task.Delay(timeout)) == _ready.Task;

        public string Output { get { lock (_out) return _out.ToString(); } }

        public void Dispose()
        {
            try { if (!_proc.HasExited) _proc.Kill(entireProcessTree: true); } catch { /* already gone */ }
            _proc.Dispose();
        }
    }

    public static Handle Start(string dll, string readyMarker, params string[] args) => new(dll, readyMarker, args);

    // Run a program to completion (the client) and return its exit code + combined stdout/stderr.
    public static async Task<(int exit, string output)> RunAsync(string dll, TimeSpan timeout, params string[] args)
    {
        using var proc = NewDotnet(dll, args);
        var sb = new StringBuilder();
        proc.OutputDataReceived += (_, e) => { if (e.Data != null) lock (sb) sb.AppendLine(e.Data); };
        proc.ErrorDataReceived += (_, e) => { if (e.Data != null) lock (sb) sb.AppendLine(e.Data); };
        proc.Start();
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();

        using var cts = new CancellationTokenSource(timeout);
        try { await proc.WaitForExitAsync(cts.Token); }
        catch (OperationCanceledException)
        {
            try { proc.Kill(entireProcessTree: true); } catch { }
            throw new TimeoutException($"program {Path.GetFileName(dll)} did not exit within {timeout}");
        }
        lock (sb) return (proc.ExitCode, sb.ToString());
    }

    private static Process NewDotnet(string dll, string[] args)
    {
        var psi = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        psi.ArgumentList.Add(dll);
        foreach (var a in args) psi.ArgumentList.Add(a);
        psi.Environment["ZITI_LOG"] = Environment.GetEnvironmentVariable("ZITI_LOG") ?? "2"; // WARN: quiet children
        return new Process { StartInfo = psi };
    }
}
