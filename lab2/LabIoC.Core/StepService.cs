namespace LabIoC.Core;

public class StepService(ITransactionContext context)
{
    public Guid GetCurrentTransaction() => context.TransactionId;
}