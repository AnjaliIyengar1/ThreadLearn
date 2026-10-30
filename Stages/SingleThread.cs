namespace ThreadLearn.Stages;

// Use this to see single-threaded behavior: every file is hashed by the main thread, one after another
public static class SingleThread
{
    public static void Run(string[] files)
    {
        foreach (string path in files)
            Hasher.HashAndPrint(path);
    }
}