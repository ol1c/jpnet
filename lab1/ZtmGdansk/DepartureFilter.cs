public static class DepartureFilter
{
    public static List<Departure> Filter(IEnumerable<Departure>? departures, string? line, int max)
    {
        if (departures is null || max <= 0) return new List<Departure>();

        return departures
            .Where(d => line is null ||
                        string.Equals(d.RouteShortName, line, StringComparison.OrdinalIgnoreCase))
            .OrderBy(d => d.EstimatedTime)
            .Take(max)
            .ToList();
    }
}
