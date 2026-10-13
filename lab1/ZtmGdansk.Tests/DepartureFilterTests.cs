using Xunit;

public class DepartureFilterTests
{
    static readonly DateTimeOffset T0 = new(2026, 10, 11, 15, 0, 0, TimeSpan.Zero);

    static Departure Dep(string line, int minutes) =>
        new(line, "Test", T0.AddMinutes(minutes), T0.AddMinutes(minutes), 0, "REALTIME");

    static List<Departure> Sample() => new()
    {
        Dep("12", 20), Dep("6", 5), Dep("N1", 15), Dep("6", 25), Dep("12", 10)
    };

    [Fact]
    public void NoLine_ReturnsAll_SortedByEstimatedTime()
    {
        var result = DepartureFilter.Filter(Sample(), null, 10);

        Assert.Equal(5, result.Count);
        Assert.Equal(result.OrderBy(d => d.EstimatedTime).ToList(), result);
    }

    [Fact]
    public void ByLine_ReturnsOnlyThatLine()
    {
        var result = DepartureFilter.Filter(Sample(), "6", 10);

        Assert.Equal(2, result.Count);
        Assert.All(result, d => Assert.Equal("6", d.RouteShortName));
    }

    [Theory]
    [InlineData("n1")]
    [InlineData("N1")]
    public void LineFilter_IsCaseInsensitive(string line)
    {
        var result = DepartureFilter.Filter(Sample(), line, 10);

        Assert.Single(result);
        Assert.Equal("N1", result[0].RouteShortName);
    }

    [Fact]
    public void Max_LimitsResultCount_AndKeepsEarliest()
    {
        var result = DepartureFilter.Filter(Sample(), null, 2);

        Assert.Equal(2, result.Count);
        Assert.Equal(T0.AddMinutes(5), result[0].EstimatedTime);
        Assert.Equal(T0.AddMinutes(10), result[1].EstimatedTime);
    }

    [Fact]
    public void UnknownLine_ReturnsEmpty()
    {
        Assert.Empty(DepartureFilter.Filter(Sample(), "999", 10));
    }

    [Fact]
    public void NullInput_ReturnsEmpty()
    {
        Assert.Empty(DepartureFilter.Filter(null, "6", 10));
    }
}
