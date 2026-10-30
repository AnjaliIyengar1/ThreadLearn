using System.Diagnostics;

namespace ThreadLearn;

public static class Measure
{
    // Runs the work and reports wall time, CPU time and average CPU %
    public static void Run(string label, Action work)
    {
        Console.WriteLine($"\n=== {label} ===");

        // CPU time this process has used so far (summed across all cores)
        TimeSpan cpuBefore = Process.GetCurrentProcess().TotalProcessorTime;

        // Start a timer so we can measure how long the work takes
        var sw = Stopwatch.StartNew();

        work();

        sw.Stop();
        TimeSpan cpuUsed = Process.GetCurrentProcess().TotalProcessorTime - cpuBefore;

        // 100% = one core busy for the whole run; max = cores x 100%
        double cpuPercent = cpuUsed.TotalMilliseconds / sw.Elapsed.TotalMilliseconds * 100;

        // Print the summary: wall time, CPU time, average CPU %
        Console.WriteLine($"{label}: {sw.ElapsedMilliseconds} ms, CPU time: {cpuUsed.TotalMilliseconds:F0} ms, " +
                          $"Avg CPU: {cpuPercent:F0}% (max possible: {Environment.ProcessorCount * 100}%)");
    }
}