using AiModoo.Core.Entities.Accounting;

namespace AiModoo.Core.Interfaces.Accounting;

public interface IFiscalPeriodService
{
    Task<IReadOnlyList<FiscalYear>> GetAllYearsAsync(CancellationToken ct = default);
    Task<FiscalYear> CreateYearAsync(FiscalYear year, bool generatePeriods = true, CancellationToken ct = default);
    Task<FiscalPeriod?> GetCurrentPeriodAsync(CancellationToken ct = default);
    Task<FiscalPeriod?> GetPeriodForDateAsync(DateTime date, CancellationToken ct = default);
    Task ClosePeriodAsync(int periodId, CancellationToken ct = default);
    Task CloseYearAsync(int yearId, CancellationToken ct = default);
    Task<JournalEntry> GenerateClosingEntryAsync(int yearId, CancellationToken ct = default);
}
