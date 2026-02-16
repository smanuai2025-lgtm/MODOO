using AiModoo.Core.DTOs.Accounting;
using AiModoo.Core.Interfaces.Accounting;
using AiModoo.Core.Interfaces.Common;
using FastReport;
using FastReport.Export.PdfSimple;
using FastReport.Utils;

namespace AiModoo.Services.Reports;

public class ReportService : IReportService
{
    private readonly IInvoiceService _invoiceService;
    private readonly IFinancialReportService _financialReportService;
    private readonly ITaxService _taxService;
    private readonly IAccountService _accountService;

    public ReportService(
        IInvoiceService invoiceService,
        IFinancialReportService financialReportService,
        ITaxService taxService,
        IAccountService accountService)
    {
        _invoiceService = invoiceService;
        _financialReportService = financialReportService;
        _taxService = taxService;
        _accountService = accountService;
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(int invoiceId, CancellationToken ct = default)
    {
        var invoice = await _invoiceService.GetByIdAsync(invoiceId, ct)
            ?? throw new Exception($"Invoice {invoiceId} not found.");

        using var report = new Report();
        var page = new ReportPage();
        report.Pages.Add(page);
        page.CreateUniqueName();

        var band = new DataBand();
        page.Bands.Add(band);
        band.CreateUniqueName();

        var typeLabel = invoice.InvoiceType switch
        {
            Core.Enums.InvoiceType.CustomerInvoice => "Customer Invoice | فاتورة عميل",
            Core.Enums.InvoiceType.VendorBill => "Vendor Bill | فاتورة مورد",
            Core.Enums.InvoiceType.CustomerCreditNote => "Credit Note | إشعار دائن",
            Core.Enums.InvoiceType.VendorDebitNote => "Debit Note | إشعار مدين",
            _ => "Invoice | فاتورة"
        };

        AddReportTitle(band, typeLabel, $"#{invoice.Number}");

        float y = Units.Centimeters * 2.5f;
        AddText(band, 0, y, 10, $"Date: {invoice.Date:yyyy-MM-dd}", false);
        AddText(band, 10, y, 9, $"Due: {invoice.DueDate:yyyy-MM-dd}", false);
        y += Units.Centimeters * 0.5f;
        AddText(band, 0, y, 10, $"Partner: {invoice.PartnerName ?? "-"}", false);
        AddText(band, 10, y, 9, $"Ref: {invoice.Reference ?? "-"}", false);
        y += Units.Centimeters * 1;

        // Lines header
        AddText(band, 0, y, 19, $"{"Description",-25} {"Qty",8} {"Price",10} {"Disc",8} {"Tax",8} {"Total",10}", true);
        y += Units.Centimeters * 0.6f;

        foreach (var line in invoice.Lines)
        {
            var desc = line.Description.Length > 25 ? line.Description[..25] : line.Description;
            AddText(band, 0, y, 19, $"{desc,-25} {line.Quantity,8:N2} {line.UnitPrice,10:N2} {line.Discount,8:N2} {line.TaxAmount,8:N2} {line.Total,10:N2}", false, true);
            y += Units.Centimeters * 0.5f;
        }

        y += Units.Centimeters * 0.5f;
        AddText(band, 12, y, 7, $"SubTotal:  {invoice.SubTotal,10:N2}", false, true);
        y += Units.Centimeters * 0.5f;
        AddText(band, 12, y, 7, $"Tax:       {invoice.TaxTotal,10:N2}", false, true);
        y += Units.Centimeters * 0.5f;
        AddText(band, 12, y, 7, $"Total:     {invoice.Total,10:N2}", true, true);
        y += Units.Centimeters * 0.5f;
        AddText(band, 12, y, 7, $"Paid:      {invoice.AmountPaid,10:N2}", false, true);
        y += Units.Centimeters * 0.5f;
        AddText(band, 12, y, 7, $"Due:       {invoice.AmountDue,10:N2}", true, true);

        band.Height = y + Units.Centimeters * 1.5f;
        report.Prepare();
        return ExportToPdf(report);
    }

    public async Task<byte[]> GenerateTrialBalancePdfAsync(DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        var data = await _financialReportService.GetTrialBalanceAsync(fromDate, toDate, ct);

        using var report = new Report();
        var page = new ReportPage();
        report.Pages.Add(page);
        page.CreateUniqueName();

        var band = new DataBand();
        page.Bands.Add(band);
        band.CreateUniqueName();

        AddReportTitle(band, "Trial Balance | ميزان المراجعة",
            $"{fromDate?.ToString("yyyy-MM-dd") ?? "Start"} to {toDate?.ToString("yyyy-MM-dd") ?? "Now"}");

        float y = Units.Centimeters * 2.5f;

        // Table header
        AddText(band, 0, y, 19, $"{"Code",-8} {"Account",-24} {"Debit",12} {"Credit",12}", true, true);
        y += Units.Centimeters * 0.6f;

        foreach (var line in data.Lines)
        {
            var acctName = line.AccountName.Length > 24 ? line.AccountName[..24] : line.AccountName;
            AddText(band, 0, y, 19, $"{line.AccountCode,-8} {acctName,-24} {line.Debit,12:N2} {line.Credit,12:N2}", false, true);
            y += Units.Centimeters * 0.5f;
        }

        y += Units.Centimeters * 0.3f;
        AddText(band, 0, y, 19, $"Total Debit:  {data.TotalDebit:N2}", false, true);
        y += Units.Centimeters * 0.6f;
        AddText(band, 0, y, 19, $"Total Credit: {data.TotalCredit:N2}", false, true);
        y += Units.Centimeters * 0.6f;
        AddText(band, 0, y, 19, $"Difference:   {(data.TotalDebit - data.TotalCredit):N2}", true, true);

        band.Height = y + Units.Centimeters * 1.5f;
        report.Prepare();
        return ExportToPdf(report);
    }

    public async Task<byte[]> GenerateIncomeStatementPdfAsync(DateTime fromDate, DateTime toDate, CancellationToken ct = default)
    {
        var data = await _financialReportService.GetIncomeStatementAsync(fromDate, toDate, ct);

        using var report = new Report();
        var page = new ReportPage();
        report.Pages.Add(page);
        page.CreateUniqueName();

        var band = new DataBand();
        page.Bands.Add(band);
        band.CreateUniqueName();

        AddReportTitle(band, "Income Statement | قائمة الدخل", $"{fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}");

        float y = Units.Centimeters * 2.5f;
        foreach (var section in data.Sections)
        {
            AddText(band, 0, y, 19, section.Title, true);
            y += Units.Centimeters * 0.7f;

            foreach (var line in section.Lines)
            {
                var indent = line.Level > 0 ? "  " : "";
                AddText(band, 0.5f, y, 18.5f, $"{indent}{line.AccountCode}  {line.AccountName,-25} {line.Amount,12:N2}", false, true);
                y += Units.Centimeters * 0.5f;
            }

            AddText(band, 0, y, 19, $"Total: {section.SectionTotal:N2}", true, true);
            y += Units.Centimeters * 0.8f;
        }

        AddText(band, 0, y, 19, $"Grand Total / Net Income: {data.GrandTotal:N2}", true);
        band.Height = y + Units.Centimeters * 1.5f;
        report.Prepare();
        return ExportToPdf(report);
    }

    public async Task<byte[]> GenerateBalanceSheetPdfAsync(DateTime asOfDate, CancellationToken ct = default)
    {
        var data = await _financialReportService.GetBalanceSheetAsync(asOfDate, ct);

        using var report = new Report();
        var page = new ReportPage();
        report.Pages.Add(page);
        page.CreateUniqueName();

        var band = new DataBand();
        page.Bands.Add(band);
        band.CreateUniqueName();

        AddReportTitle(band, "Balance Sheet | الميزانية العمومية", $"As of {asOfDate:yyyy-MM-dd}");

        float y = Units.Centimeters * 2.5f;
        foreach (var section in data.Sections)
        {
            AddText(band, 0, y, 19, section.Title, true);
            y += Units.Centimeters * 0.7f;

            foreach (var line in section.Lines)
            {
                var indent = line.Level > 0 ? "  " : "";
                AddText(band, 0.5f, y, 18.5f, $"{indent}{line.AccountCode}  {line.AccountName,-25} {line.Amount,12:N2}", false, true);
                y += Units.Centimeters * 0.5f;
            }

            AddText(band, 0, y, 19, $"Total: {section.SectionTotal:N2}", true, true);
            y += Units.Centimeters * 0.8f;
        }

        band.Height = y + Units.Centimeters * 1;
        report.Prepare();
        return ExportToPdf(report);
    }

    public async Task<byte[]> GenerateGeneralLedgerPdfAsync(int accountId, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        var from = fromDate ?? new DateTime(DateTime.Now.Year, 1, 1);
        var to = toDate ?? DateTime.Now;
        var data = await _financialReportService.GetGeneralLedgerAsync(accountId, from, to, ct);

        using var report = new Report();
        var page = new ReportPage();
        report.Pages.Add(page);
        page.CreateUniqueName();

        var band = new DataBand();
        page.Bands.Add(band);
        band.CreateUniqueName();

        var accountName = data.Count > 0 ? $"{data[0].AccountCode} - {data[0].AccountName}" : "Account";
        AddReportTitle(band, "General Ledger | دفتر الأستاذ العام", $"{accountName} | {from:yyyy-MM-dd} to {to:yyyy-MM-dd}");

        float y = Units.Centimeters * 2.5f;
        AddText(band, 0, y, 19, $"{"Date",-12} {"Ref",-12} {"Label",-18} {"Debit",10} {"Credit",10} {"Balance",10}", true, true);
        y += Units.Centimeters * 0.6f;

        foreach (var line in data)
        {
            var label = (line.Label ?? "").Length > 18 ? (line.Label ?? "")[..18] : line.Label ?? "";
            var refText = (line.Reference ?? "").Length > 12 ? (line.Reference ?? "")[..12] : line.Reference ?? "";
            AddText(band, 0, y, 19, $"{line.Date:yyyy-MM-dd}  {refText,-12} {label,-18} {line.Debit,10:N2} {line.Credit,10:N2} {line.RunningBalance,10:N2}", false, true);
            y += Units.Centimeters * 0.5f;
        }

        band.Height = y + Units.Centimeters * 1;
        report.Prepare();
        return ExportToPdf(report);
    }

    public async Task<byte[]> GenerateTaxReportPdfAsync(DateTime fromDate, DateTime toDate, CancellationToken ct = default)
    {
        var data = await _taxService.GetTaxReportAsync(fromDate, toDate, ct);

        using var report = new Report();
        var page = new ReportPage();
        report.Pages.Add(page);
        page.CreateUniqueName();

        var band = new DataBand();
        page.Bands.Add(band);
        band.CreateUniqueName();

        AddReportTitle(band, "Tax Report | تقرير الضرائب", $"{fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}");

        float y = Units.Centimeters * 2.5f;
        AddText(band, 0, y, 19, $"{"Tax",-18} {"Type",-10} {"Rate",6} {"Taxable",12} {"Tax Amt",10}", true, true);
        y += Units.Centimeters * 0.6f;

        foreach (var line in data.Lines)
        {
            AddText(band, 0, y, 19, $"{line.TaxName,-18} {line.Type,-10} {line.Rate,5:N1}% {line.TaxableAmount,12:N2} {line.TaxAmount,10:N2}", false, true);
            y += Units.Centimeters * 0.5f;
        }

        y += Units.Centimeters * 0.5f;
        AddText(band, 0, y, 19, $"Sales Tax:    {data.TotalSalesTax:N2}", false, true);
        y += Units.Centimeters * 0.5f;
        AddText(band, 0, y, 19, $"Purchase Tax: {data.TotalPurchaseTax:N2}", false, true);
        y += Units.Centimeters * 0.5f;
        AddText(band, 0, y, 19, $"Net Payable:  {data.NetTax:N2}", true, true);

        band.Height = y + Units.Centimeters * 1.5f;
        report.Prepare();
        return ExportToPdf(report);
    }

    private static void AddReportTitle(DataBand band, string title, string subtitle)
    {
        var titleText = new TextObject();
        band.Objects.Add(titleText);
        titleText.Bounds = new System.Drawing.RectangleF(0, 0, Units.Centimeters * 19, Units.Centimeters * 1);
        titleText.Text = title;
        titleText.Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);
        titleText.HorzAlign = HorzAlign.Center;

        var subtitleText = new TextObject();
        band.Objects.Add(subtitleText);
        subtitleText.Bounds = new System.Drawing.RectangleF(0, Units.Centimeters * 1, Units.Centimeters * 19, Units.Centimeters * 0.6f);
        subtitleText.Text = subtitle;
        subtitleText.Font = new System.Drawing.Font("Arial", 10);
        subtitleText.HorzAlign = HorzAlign.Center;
    }

    private static void AddText(DataBand band, float xCm, float yCm, float widthCm, string text, bool bold, bool mono = false)
    {
        var obj = new TextObject();
        band.Objects.Add(obj);
        obj.Bounds = new System.Drawing.RectangleF(Units.Centimeters * xCm, yCm, Units.Centimeters * widthCm, Units.Centimeters * 0.5f);
        obj.Text = text;
        var fontName = mono ? "Courier New" : "Arial";
        obj.Font = new System.Drawing.Font(fontName, 9, bold ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular);
    }

    private static byte[] ExportToPdf(Report report)
    {
        using var stream = new MemoryStream();
        var pdfExport = new PDFSimpleExport();
        report.Export(pdfExport, stream);
        return stream.ToArray();
    }
}
