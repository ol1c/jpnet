var target = Argument("target", "Pack");
var configuration = Argument("configuration", "Release");

var artifacts = Directory("./artifacts");
var solution = GetFiles("./*.sln").Concat(GetFiles("./*.slnx")).First().FullPath;
var toolProject = "./ZtmGdansk/ZtmGdansk.csproj";

Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifacts);   // tworzy katalog, jesli go nie ma

    var dirs = GetDirectories("./**/bin").Concat(GetDirectories("./**/obj"));
    foreach (var dir in dirs)
    {
        Information("Usuwam {0}", dir);
        DeleteDirectory(dir, new DeleteDirectorySettings { Recursive = true, Force = true });
    }
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetRestore(solution);
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
    DotNetBuild(solution, new DotNetBuildSettings
    {
        Configuration = configuration,
        NoRestore = true
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetTest(solution, new DotNetTestSettings
    {
        Configuration = configuration,
        NoRestore = true,
        NoBuild = true
    });
});

Task("Pack")
    .IsDependentOn("Test")
    .Does(() =>
{
    DotNetPack(toolProject, new DotNetPackSettings
    {
        Configuration = configuration,
        NoRestore = true,
        NoBuild = true,
        OutputDirectory = artifacts
    });
});

RunTarget(target);
