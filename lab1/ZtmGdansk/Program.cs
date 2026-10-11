using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;

Console.OutputEncoding = Encoding.UTF8;

string? stopArg = null;
string? lineFilter = null;
int max = 10;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "-l" or "--line":
            lineFilter = i + 1 < args.Length ? args[++i] : null;
            break;
        case "-n" or "--count":
            if (i + 1 < args.Length && int.TryParse(args[++i], out var n) && n > 0) max = n;
            break;
        case "-h" or "--help":
            PrintHelp();
            return 0;
        default:
            stopArg ??= args[i];
            break;
    }
}

if (stopArg is null || !int.TryParse(stopArg, out int stopId))
{
    Console.Error.WriteLine("Blad: podaj numeryczny identyfikator przystanku (stopId).");
    PrintHelp();
    return 1;
}

try
{
    using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    var url = $"https://ckan2.multimediagdansk.pl/departures?stopId={stopId}";
    var data = await http.GetFromJsonAsync<DeparturesResponse>(url);

    var list = DepartureFilter.Filter(data?.Departures, lineFilter, max);

    var updated = data?.LastUpdate.ToLocalTime().ToString("HH:mm:ss", CultureInfo.InvariantCulture) ?? "-";
    Console.WriteLine($"Przystanek {stopId} | stan na {updated}" +
                      (lineFilter is null ? "" : $" | linia {lineFilter}"));
    Console.WriteLine(new string('-', 66));

    if (list.Count == 0)
    {
        Console.WriteLine("Brak odjazdow (zly stopId, inna linia albo brak kursow).");
        return 0;
    }

    Console.WriteLine($"{"Linia",-6}{"Kierunek",-26}{"Odjazd",-8}{"Za",-9}{"Opoznienie",-11}Tryb");
    foreach (var d in list)
    {
        var est = d.EstimatedTime.ToLocalTime();
        int mins = (int)Math.Round((d.EstimatedTime - DateTimeOffset.UtcNow).TotalMinutes);
        string za = mins <= 0 ? "teraz" : $"{mins} min";
        int delayMin = (int)Math.Round(d.DelayInSeconds / 60.0);
        string delay = delayMin == 0 ? "o czasie" : $"{delayMin:+0;-0} min";
        string head = d.Headsign.Length > 24 ? d.Headsign[..24] : d.Headsign;

        Console.WriteLine($"{d.RouteShortName,-6}{head,-26}{est:HH:mm,-8}{za,-9}{delay,-11}{d.Status}");
    }
    return 0;
}
catch (HttpRequestException ex)
{
    Console.Error.WriteLine($"Blad sieci/API: {ex.Message}");
    return 2;
}
catch (TaskCanceledException)
{
    Console.Error.WriteLine("Przekroczono czas oczekiwania na API.");
    return 2;
}

static void PrintHelp()
{
    Console.WriteLine("Uzycie: ztm-gdansk <stopId> [-l|--line <numer>] [-n|--count <ile>]");
    Console.WriteLine("Przyklad: ztm-gdansk 2019 --line 6 -n 5");
}

public record DeparturesResponse(
    [property: JsonPropertyName("lastUpdate")] DateTimeOffset LastUpdate,
    [property: JsonPropertyName("departures")] List<Departure> Departures);

public record Departure(
    [property: JsonPropertyName("routeShortName")] string RouteShortName,
    [property: JsonPropertyName("headsign")] string Headsign,
    [property: JsonPropertyName("estimatedTime")] DateTimeOffset EstimatedTime,
    [property: JsonPropertyName("theoreticalTime")] DateTimeOffset TheoreticalTime,
    [property: JsonPropertyName("delayInSeconds")] int DelayInSeconds,
    [property: JsonPropertyName("status")] string Status);
