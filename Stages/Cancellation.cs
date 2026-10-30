namespace ThreadLearn.Stages;

// Cancellation: you can't force-stop a thread; you ask it to stop, and it checks the request
public static class Cancellation
{
    // Single thread: checks for a stop request before each file
    public static void RunLoop(string[] files, int cancelAfterMs)
    {
        using var cts = new CancellationTokenSource(); // the stop button
        cts.CancelAfter(cancelAfterMs);                // press it automatically after N ms
        CancellationToken token = cts.Token;           // what the worker watches

        int done = 0;
        foreach (string path in files)
        {
            if (token.IsCancellationRequested) // was stop pressed?
            {
                Console.WriteLine($"  Stopped after {done} of {files.Length} files");
                return;
            }

            Hasher.HashFile(path); // a file in progress always finishes
            done++;
        }

        Console.WriteLine($"  Finished all {done} files before cancel");
    }

    // Parallel.ForEach: give it the token; it stops handing out files and throws
    public static void RunParallel(string[] files, int cancelAfterMs)
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(cancelAfterMs);
        var options = new ParallelOptions { CancellationToken = cts.Token };

        int done = 0;
        try
        {
            Parallel.ForEach(files, options, path =>
            {
                Hasher.HashFile(path);
                Interlocked.Increment(ref done); // many threads update 'done'
            });

            Console.WriteLine($"  Finished all {done} files before cancel");
        }
        catch (OperationCanceledException) // this is how Parallel.ForEach reports "cancelled"
        {
            Console.WriteLine($"  Cancelled after {done} of {files.Length} files");
        }
    }
}