using System;
using System.IO;

public static class SafeFileWriter
{
    // Global lock object for thread safety
    private static readonly object _lock = new object();

    // Path of the shared output file
    private static readonly string _filePath = "output.txt";

    public static void WriteLineSafe(string text)
    {
        lock (_lock)  // ensures only one thread writes at a time
        {
            File.AppendAllText(_filePath, text + Environment.NewLine);
        }
    }
}