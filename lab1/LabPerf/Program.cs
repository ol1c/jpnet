using System;
using System.Diagnostics;
using System.Globalization;

public class Program
{
    const string Sample = "STATUS=200;ID=49201;LINE=12;DELAY=00:04:15";

    static void HeavyMethod()
    {
        double result = 0;
        for (int i = 0; i < 50000; i++) result += Math.Sqrt(i) * Math.Sin(i);
        Console.WriteLine(result);
    }

    static (int Line, TimeSpan Delay) ParseClassic(string s)
    {
        int line = 0;
        TimeSpan delay = default;
        string[] parts = s.Split(';');
        foreach (string part in parts)
        {
            int eq = part.IndexOf('=');
            string key = part.Substring(0, eq);
            string val = part.Substring(eq + 1);
            if (key == "LINE") line = int.Parse(val, CultureInfo.InvariantCulture);
            else if (key == "DELAY") delay = TimeSpan.Parse(val, CultureInfo.InvariantCulture);
        }
        return (line, delay);
    }

    static (int Line, TimeSpan Delay) ParseSpan(ReadOnlySpan<char> s)
    {
        int line = 0;
        TimeSpan delay = default;
        while (!s.IsEmpty)
        {
            int sep = s.IndexOf(';');
            ReadOnlySpan<char> part = sep >= 0 ? s.Slice(0, sep) : s;
            s = sep >= 0 ? s.Slice(sep + 1) : ReadOnlySpan<char>.Empty;

            int eq = part.IndexOf('=');
            if (eq < 0) continue;
            ReadOnlySpan<char> key = part.Slice(0, eq);
            ReadOnlySpan<char> val = part.Slice(eq + 1);

            if (key.Equals("LINE", StringComparison.Ordinal))
                line = int.Parse(val, NumberStyles.Integer, CultureInfo.InvariantCulture);
            else if (key.Equals("DELAY", StringComparison.Ordinal))
                TimeSpan.TryParse(val, CultureInfo.InvariantCulture, out delay);
        }
        return (line, delay);
    }

    public static void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();
        HeavyMethod();
        sw.Stop();
        Console.WriteLine($"HeavyMethod (pierwsze wywolanie): {sw.Elapsed.TotalMilliseconds:F2} ms");

        var c = ParseClassic(Sample);
        var s = ParseSpan(Sample);
        Console.WriteLine($"Classic: LINE={c.Line}, DELAY={c.Delay}");
        Console.WriteLine($"Span:    LINE={s.Line}, DELAY={s.Delay}");

        const int N = 1000;
        long sink = 0;

        sink += ParseClassic(Sample).Line;
        sink += ParseSpan(Sample).Line;

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < N; i++) sink += ParseClassic(Sample).Line;
        long classicBytes = GC.GetAllocatedBytesForCurrentThread() - before;

        before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < N; i++) sink += ParseSpan(Sample).Line;
        long spanBytes = GC.GetAllocatedBytesForCurrentThread() - before;

        Console.WriteLine($"Classic: {classicBytes} B / {N} wywolan = {classicBytes / N} B na wywolanie");
        Console.WriteLine($"Span:    {spanBytes} B / {N} wywolan = {spanBytes / N} B na wywolanie");
        Console.WriteLine($"(sink={sink})");
    }
}