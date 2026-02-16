namespace AiModoo.Core.Exceptions;

public class UnbalancedEntryException : BusinessRuleException
{
    public decimal TotalDebit { get; }
    public decimal TotalCredit { get; }

    public UnbalancedEntryException(decimal totalDebit, decimal totalCredit)
        : base("UNBALANCED_ENTRY", $"Journal entry is not balanced. Total Debit: {totalDebit}, Total Credit: {totalCredit}")
    {
        TotalDebit = totalDebit;
        TotalCredit = totalCredit;
    }
}
