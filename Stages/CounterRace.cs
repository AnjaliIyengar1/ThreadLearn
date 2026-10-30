using System.Diagnostics;

public static class CounterRace
{
    const int PerThread = 1_000_000;

    // Shared counters: class-level, so every thread and method can see them
    private static int unsafeCount;
    private static int lockedCount;
    private static int atomicCount;
    private static readonly object gate = new();

    private static void AddOneUnsafe() => unsafeCount++;

    private static void AddOneLocked()
    {
        lock (gate)
        {
            lockedCount++;
        }
    }

    // read + add + write as one indivisible step. Other threads can’t get in between, so no update is ever lost.
    private static void AddOneAtomic() => Interlocked.Increment(ref atomicCount);

    public static void Run(int threadCount)
    {
        unsafeCount = lockedCount = atomicCount = 0; // reset, in case Run is called twice
        Console.WriteLine($"\nExpected total: {threadCount * PerThread:N0}");

        Time("No lock", threadCount, AddOneUnsafe, () => unsafeCount);
        Time("lock", threadCount, AddOneLocked, () => lockedCount);
        Time("Interlocked", threadCount, AddOneAtomic, () => atomicCount);
    }

    // Starts N threads; each runs addOne 1,000,000 times. Then prints the final count and time.
    static void Time(string label, int threadCount, Action addOne, Func<int> readTotal)
    {
        var sw = Stopwatch.StartNew();
        var threads = new Thread[threadCount];

        for (int t = 0; t < threadCount; t++)
        {
            threads[t] = new Thread(() =>
            {
                for (int i = 0; i < PerThread; i++)
                {
                    addOne();
                }
            });
            threads[t].Start();
        }

        foreach (Thread th in threads)
        {
            th.Join(); // wait for every thread to finish before reading the total
        }

        int total = readTotal();
        Console.WriteLine($"{label,-12}: {total:N0} in {sw.ElapsedMilliseconds} ms");
    }
}