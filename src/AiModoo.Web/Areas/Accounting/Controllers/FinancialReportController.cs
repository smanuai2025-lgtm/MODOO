using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Entities.Sales;
using AiModoo.Core.Enums;
using AiModoo.Core.Interfaces.Accounting;
using AiModoo.Core.Interfaces.Common;
using AiModoo.Core.Interfaces.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AiModoo.Web.Areas.Accounting.Controllers;

[Area("Accounting")]
[Authorize]
public class FinancialReportController : Controller
{
    private readonly IFinancialReportService _financialReportService;
    private readonly ITaxService _taxService;
    private readonly IReportService _reportService;
    private readonly IPartnerService _partnerService;
    private readonly IRepository<Account> _accountRepository;

    public FinancialReportController(
        IFinancialReportService financialReportService,
        ITaxService taxService,
        IReportService reportService,
        IPartnerService partnerService,
        IRepository<Account> accountRepository)
    {
        _financialReportService = financialReportService;
        _taxService = taxService;
        _reportService = reportService;
        _partnerService = partnerService;
        _accountRepository = accountRepository;
    }

    public IActionResult Index() => View();

    public async Task<IActionResult> TrialBalance(DateTime? fromDate, DateTime? toDate)
    {
        var report = await _financialReportService.GetTrialBalanceAsync(fromDate, toDate);
        return View(report);
    }

    public async Task<IActionResult> IncomeStatement(DateTime? fromDate, DateTime? toDate)
    {
        var from = fromDate ?? new DateTime(DateTime.Now.Year, 1, 1);
        var to = toDate ?? DateTime.Now;
        var report = await _financialReportService.GetIncomeStatementAsync(from, to);
        return View(report);
    }

    public async Task<IActionResult> BalanceSheet(DateTime? asOfDate)
    {
        var date = asOfDate ?? DateTime.Now;
        var report = await _financialReportService.GetBalanceSheetAsync(date);
        return View(report);
    }

    public async Task<IActionResult> GeneralLedger(int accountId, DateTime? fromDate, DateTime? toDate)
    {
        var from = fromDate ?? new DateTime(DateTime.Now.Year, 1, 1);
        var to = toDate ?? DateTime.Now;
        var report = await _financialReportService.GetGeneralLedgerAsync(accountId, from, to);
        ViewBag.AccountId = accountId;
        ViewBag.FromDate = from;
        ViewBag.ToDate = to;
        return View(report);
    }

    public async Task<IActionResult> AgedReceivable(DateTime? asOfDate)
    {
        var date = asOfDate ?? DateTime.Now;
        var report = await _financialReportService.GetAgedReceivableAsync(date);
        ViewBag.AsOfDate = date;
        return View(report);
    }

    public async Task<IActionResult> AgedPayable(DateTime? asOfDate)
    {
        var date = asOfDate ?? DateTime.Now;
        var report = await _financialReportService.GetAgedPayableAsync(date);
        ViewBag.AsOfDate = date;
        return View(report);
    }

    // Partner Statement (Customer / Vendor)
    public async Task<IActionResult> PartnerStatement(int? partnerId, DateTime? fromDate, DateTime? toDate, PartnerType? type)
    {
        await PopulatePartnerDropdown(type);
        await PopulateAccountDropdown();

        if (partnerId.HasValue)
        {
            var from = fromDate ?? new DateTime(DateTime.Now.Year, 1, 1);
            var to = toDate ?? DateTime.Now;
            var report = await _financialReportService.GetPartnerStatementAsync(partnerId.Value, from, to);
            ViewBag.FromDate = from;
            ViewBag.ToDate = to;
            ViewBag.SelectedPartnerId = partnerId;
            ViewBag.PartnerType = type;
            return View(report);
        }

        ViewBag.FromDate = fromDate ?? new DateTime(DateTime.Now.Year, 1, 1);
        ViewBag.ToDate = toDate ?? DateTime.Now;
        ViewBag.PartnerType = type;
        return View();
    }

    // Account Statement (Chart of Accounts)
    public async Task<IActionResult> AccountStatement(int? accountId, DateTime? fromDate, DateTime? toDate)
    {
        await PopulateAccountDropdown();
        await PopulatePartnerDropdown(null);

        if (accountId.HasValue)
        {
            var from = fromDate ?? new DateTime(DateTime.Now.Year, 1, 1);
            var to = toDate ?? DateTime.Now;
            var report = await _financialReportService.GetAccountStatementAsync(accountId.Value, from, to);
            ViewBag.FromDate = from;
            ViewBag.ToDate = to;
            ViewBag.SelectedAccountId = accountId;
            return View(report);
        }

        ViewBag.FromDate = fromDate ?? new DateTime(DateTime.Now.Year, 1, 1);
        ViewBag.ToDate = toDate ?? DateTime.Now;
        return View();
    }

    // Tax Report
    public async Task<IActionResult> TaxReport(DateTime? fromDate, DateTime? toDate)
    {
        var from = fromDate ?? new DateTime(DateTime.Now.Year, 1, 1);
        var to = toDate ?? DateTime.Now;
        var report = await _taxService.GetTaxReportAsync(from, to);
        ViewBag.FromDate = from;
        ViewBag.ToDate = to;
        return View(report);
    }

    // PDF Export endpoints
    [HttpGet]
    public async Task<IActionResult> ExportTrialBalancePdf(DateTime? fromDate, DateTime? toDate)
    {
        var pdf = await _reportService.GenerateTrialBalancePdfAsync(fromDate, toDate);
        return File(pdf, "application/pdf", $"TrialBalance_{DateTime.Now:yyyyMMdd}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportIncomeStatementPdf(DateTime? fromDate, DateTime? toDate)
    {
        var from = fromDate ?? new DateTime(DateTime.Now.Year, 1, 1);
        var to = toDate ?? DateTime.Now;
        var pdf = await _reportService.GenerateIncomeStatementPdfAsync(from, to);
        return File(pdf, "application/pdf", $"IncomeStatement_{DateTime.Now:yyyyMMdd}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportBalanceSheetPdf(DateTime? asOfDate)
    {
        var date = asOfDate ?? DateTime.Now;
        var pdf = await _reportService.GenerateBalanceSheetPdfAsync(date);
        return File(pdf, "application/pdf", $"BalanceSheet_{DateTime.Now:yyyyMMdd}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportGeneralLedgerPdf(int accountId, DateTime? fromDate, DateTime? toDate)
    {
        var pdf = await _reportService.GenerateGeneralLedgerPdfAsync(accountId, fromDate, toDate);
        return File(pdf, "application/pdf", $"GeneralLedger_{DateTime.Now:yyyyMMdd}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportTaxReportPdf(DateTime? fromDate, DateTime? toDate)
    {
        var from = fromDate ?? new DateTime(DateTime.Now.Year, 1, 1);
        var to = toDate ?? DateTime.Now;
        var pdf = await _reportService.GenerateTaxReportPdfAsync(from, to);
        return File(pdf, "application/pdf", $"TaxReport_{DateTime.Now:yyyyMMdd}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportInvoicePdf(int invoiceId)
    {
        var pdf = await _reportService.GenerateInvoicePdfAsync(invoiceId);
        return File(pdf, "application/pdf", $"Invoice_{invoiceId}_{DateTime.Now:yyyyMMdd}.pdf");
    }

    private async Task PopulatePartnerDropdown(PartnerType? type)
    {
        var partners = await _partnerService.GetAllAsync(type);
        ViewBag.Partners = new SelectList(partners.Select(p => new { p.Id, Display = $"{p.Name} {(p.NameAr != null ? "- " + p.NameAr : "")}" }), "Id", "Display");
    }

    private async Task PopulateAccountDropdown()
    {
        var accounts = await _accountRepository.Query()
            .Where(a => a.IsActive)
            .OrderBy(a => a.Code)
            .ToListAsync();
        ViewBag.Accounts = new SelectList(accounts.Select(a => new { a.Id, Display = $"{a.Code} - {a.Name} {(a.NameAr != null ? "- " + a.NameAr : "")}" }), "Id", "Display");
    }
}
