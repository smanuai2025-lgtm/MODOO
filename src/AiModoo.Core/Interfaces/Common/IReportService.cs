namespace AiModoo.Core.Interfaces.Common;

public interface IReportService
{
    Task<byte[]> GenerateInvoicePdfAsync(int invoiceId, CancellationToken ct = default);
    Task<byte[]> GenerateTrialBalancePdfAsync(DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
    Task<byte[]> GenerateIncomeStatementPdfAsync(DateTime fromDate, DateTime toDate, CancellationToken ct = default);
    Task<byte[]> GenerateBalanceSheetPdfAsync(DateTime asOfDate, CancellationToken ct = default);
    Task<byte[]> GenerateGeneralLedgerPdfAsync(int accountId, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
    Task<byte[]> GenerateTaxReportPdfAsync(DateTime fromDate, DateTime toDate, CancellationToken ct = default);
}
