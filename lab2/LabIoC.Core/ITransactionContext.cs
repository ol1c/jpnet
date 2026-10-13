namespace LabIoC.Core;

public interface ITransactionContext
{
    Guid TransactionId { get; }
}