namespace ThreadLearn.Stages;

// Thread pool: .NET keeps a set of threads and reuses them; we just hand it work
public static class ThreadPoolTasks
{
    // One task per file; pool threads hash, main thread waits
    public static void RunTasks(string[] files)
    {
        var tasks = new Task[files.Length];

        for (int i = 0; i < files.Length; i++)
        {
            string path = files[i]; // own copy per task; i keeps changing
            tasks[i] = Task.Run(() => Hasher.HashAndPrint(path)); // give this file to the pool
        }

        Task.WaitAll(tasks); // wait until every file is done
    }

    // Pool threads AND the main thread hash the files
    public static void RunParallelForEach(string[] files)
    {
        Parallel.ForEach(files, path => Hasher.HashAndPrint(path));
    }
}