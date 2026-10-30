namespace ThreadLearn;

// Creates and loads the fixed set of test files used by every stage
public static class TestFilesManagement
{
    public static string[] EnsureFiles(string dir, int fileCount = 50, int minMb = 1, int maxMb = 20)
    {
        if (!Directory.Exists(dir))
        {
            Console.WriteLine("Generating test files (one-time)...");
            Generate(dir, fileCount, minMb, maxMb);
        }

        string[] files = Directory.GetFiles(dir);
        Array.Sort(files, StringComparer.Ordinal); // OS file order isn't guaranteed; sort for repeatable runs
        return files;
    }

    public static double TotalMb(string[] files) =>
        files.Sum(f => new FileInfo(f).Length) / (1024.0 * 1024.0);

    // Create sample files for the hashing demo on the first run.
    private static void Generate(string dir, int fileCount, int minMb, int maxMb)
    {
        Directory.CreateDirectory(dir);
        var rng = new Random(42); // fixed seed: identical files on every run
        byte[] buffer = new byte[1024 * 1024];

        for (int i = 0; i < fileCount; i++)
        {
            int sizeMb = rng.Next(minMb, maxMb + 1);
            using FileStream fs = File.Create(Path.Combine(dir, $"file_{i:D3}.bin"));
            for (int mb = 0; mb < sizeMb; mb++)
            {
                // fills randomDataBuffer with random byte values. Since the buffer is 1 MB, it creates 1 MB of random data to write to the file.
                rng.NextBytes(buffer);
                fs.Write(buffer);
            }
        }
    }
}