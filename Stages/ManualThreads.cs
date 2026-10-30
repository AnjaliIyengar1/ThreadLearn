using System.Diagnostics;

namespace ThreadLearn.Stages;

// Use this to see manual threads: files are split up front, thread t takes files t, t+N, t+2N, ...
public static class ManualThreads
{
    public static void Run(string[] files, int threadCount)
    {
        // array of threads
        var threads = new Thread[threadCount];

        // Create and start one thread
        for (int t = 0; t < threadCount; t++)
        {
            int start = t; // each thread needs its own copy; t keeps changing as the loop continues

            // Create and define the work for this thread
            threads[t] = new Thread(() =>
            {
                var sw = Stopwatch.StartNew();
                int count = 0;
                long bytes = 0;

                //with 4 threads: thread 0 gets 0,4,8...; thread 1 gets 1,5,9...
                for (int i = start; i < files.Length; i += threadCount)
                {
                    Hasher.HashAndPrint(files[i]);
                    count++;
                    bytes += new FileInfo(files[i]).Length;
                }

                // This thread has finished its share: report its file count, MB and time taken
                Console.WriteLine($"  -> Thread {Environment.CurrentManagedThreadId} done: {count} files, {bytes / (1024 * 1024)} MB, {sw.ElapsedMilliseconds} ms");
            });

            threads[t].Start(); // thread begins running now, in parallel with the main thread
        }

        foreach (Thread th in threads)
            th.Join(); // main thread waits here until each worker finishes
    }
}