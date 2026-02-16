using AiModoo.Core.Entities.Accounting;

namespace AiModoo.Core.Interfaces.Accounting;

public interface ITaxService
{
    Task<IReadOnlyList<Tax>> GetAllActiveAsync(CancellationToken ct = default);
    Task<Tax?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Tax> CreateAsync(Tax tax, CancellationToken ct = default);
    Task UpdateAsync(Tax tax, CancellationToken ct = default);
    Task<decimal> CalculateTaxAmount(decimal amount, int taxId, CancellationToken ct = default);

    // Tax Groups
    Task<IReadOnlyList<TaxGroup>> GetAllGroupsAsync(CancellationToken ct = default);
    Task<TaxGroup?> GetGroupByIdAsync(int id, CancellationToken ct = default);
    Task<TaxGroup> CreateGroupAsync(TaxGroup group, CancellationToken ct = default);
    Task<decimal> CalculateGroupTaxAmount(decimal amount, int groupId, CancellationToken ct = default);

    // Tax Report
    Task<TaxReportResult> GetTaxReportAsync(DateTime fromDate, DateTime toDate, CancellationToken ct = default);
}

public class TaxReportResult
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalSalesTax { get; set; }
    public decimal TotalPurchaseTax { get; set; }
    public decimal NetTax { get; set; }
    public List<TaxReportLine> Lines { get; set; } = new();
}

public class TaxReportLine
{
    public string TaxName { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public string Type { get; set; } = string.Empty; // Sales or Purchase
}
