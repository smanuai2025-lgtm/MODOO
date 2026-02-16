namespace AiModoo.Core.Exceptions;

public class FiscalPeriodClosedException : BusinessRuleException
{
    public int PeriodId { get; }

    public FiscalPeriodClosedException(int periodId)
        : base("FISCAL_PERIOD_CLOSED", $"Fiscal period {periodId} is closed. No modifications allowed.")
    {
        PeriodId = periodId;
    }
}
