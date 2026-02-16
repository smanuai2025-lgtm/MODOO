using AiModoo.Core.DTOs.Accounting;

namespace AiModoo.Core.Interfaces.Accounting;

public interface IFinancialReportService
{
    Task<TrialBalanceReportDto> GetTrialBalanceAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default);
    Task<FinancialReportDto> GetIncomeStatementAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<FinancialReportDto> GetBalanceSheetAsync(DateTime asOfDate, CancellationToken ct = default);
    Task<List<GeneralLedgerDto>> GetGeneralLedgerAsync(int accountId, DateTime fromDate, DateTime toDate, CancellationToken ct = default);
    Task<List<AgedBalanceDto>> GetAgedReceivableAsync(DateTime asOfDate, CancellationToken ct = default);
    Task<List<AgedBalanceDto>> GetAgedPayableAsync(DateTime asOfDate, CancellationToken ct = default);
    Task<PartnerStatementDto> GetPartnerStatementAsync(int partnerId, DateTime fromDate, DateTime toDate, CancellationToken ct = default);
    Task<AccountStatementDto> GetAccountStatementAsync(int accountId, DateTime fromDate, DateTime toDate, CancellationToken ct = default);
}
