namespace LabIoC.Core;

public class Logger : ILogger
{
    public void Log(string message) => Console.WriteLine($"[Logger]: {message}");
}