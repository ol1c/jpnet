namespace LabIoC.Core;

public class TransactionContext : ITransactionContext
{
    public Guid TransactionId { get; } = Guid.NewGuid();
}