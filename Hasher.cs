using System.Security.Cryptography;

namespace ThreadLearn;

public static class Hasher
{
    // Reads the whole file and returns its SHA-256 fingerprint as 64 hex characters
    public static string HashFile(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    // Hashes a file and prints which thread did it
    public static void HashAndPrint(string path)
    {
        string hash = HashFile(path);
        Console.WriteLine($"[Thread {Environment.CurrentManagedThreadId}] {Path.GetFileName(path)}  {hash[..16]}...");
    }
}