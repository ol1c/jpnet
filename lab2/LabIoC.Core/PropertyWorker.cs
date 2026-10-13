namespace LabIoC.Core;

public class PropertyWorker
{
    public ILogger? Logger { get; set; }

    public void Work()
    {
        Logger?.Log("Praca wykonana przez PropertyWorker");
    }
}