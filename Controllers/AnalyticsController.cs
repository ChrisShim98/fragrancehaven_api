using api.Controllers;
using fragrancehaven_api.DTOs;
using fragrancehaven_api.Entity;
using fragrancehaven_api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fragrancehaven_api.Controllers
{
    public class AnalyticsController : BaseApiController
    {
        private readonly IUnitOfWork _uow;
        public AnalyticsController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet] // GET: api/analytics
        public async Task<ActionResult<AnalyticsDTO>> GetAnalytics([FromQuery] DateFilter dateFilter)
        {
            if (dateFilter.StartDate > dateFilter.EndDate)
            {
                return BadRequest("Start date cannot be greater than end date.");
            }

            float totalGain = 0;
            float totalLoss = 0;
            int totalRefundedUnits = 0;
            int totalUnitsSold = 0;
            dateFilter.SetDates();

            List<Transaction> transactions = await _uow.transactionRepository.FindAllTransactionsAnalyticsAsync(dateFilter, false);
            List<Transaction> refundedTransactions = await _uow.transactionRepository.FindAllTransactionsAnalyticsAsync(dateFilter, true);

            // Set total revenue
            foreach (var transaction in transactions)
            {
                totalGain += transaction.TotalSpent;
                foreach (var unit in transaction.ProductsPurchased)
                {
                    totalUnitsSold += unit.Amount;
                }
            };
            // Subtract Refunded Units
            foreach (var refundedTransaction in refundedTransactions)
            {
                totalLoss += refundedTransaction.TotalSpent;
                foreach (var unit in refundedTransaction.ProductsPurchased)
                {
                    totalRefundedUnits += unit.Amount;
                }
            };

            AnalyticsFunctionDTO analyticsFunctionDTO = CalculateRevenuePerPeriod(dateFilter, totalGain, totalLoss, totalUnitsSold, transactions, refundedTransactions);
            // Set revenue per period
            AnalyticsDTO results = new AnalyticsDTO
            {
                TotalRevenue = totalGain - totalLoss,
                TotalGain = totalGain,
                TotalLoss = totalLoss,
                TotalUnitsSold = totalUnitsSold,
                TotalUnitsRefunded = totalRefundedUnits,
                RevenueGainPerPeriod = analyticsFunctionDTO.RevenueGainPerPeriod,
                RevenueLossPerPeriod = analyticsFunctionDTO.RevenueLossPerPeriod,
                TotalUnitsSoldPerPeriod = analyticsFunctionDTO.TotalUnitsSoldPerPeriod,
                UnitsRefundedAmountPerPeriod = analyticsFunctionDTO.UnitsRefundedAmountPerPeriod
            };

            // Set revenue labels
            results.GeneratePeriodLabels(dateFilter);

            return Ok(results);
        }

        private AnalyticsFunctionDTO CalculateRevenuePerPeriod(DateFilter dateFilter, float totalGain, float totalLoss, int totalUnitsSold, List<Transaction> transactions, List<Transaction> refundedTransactions)
        {
            List<float> revenueGainPerPeriod = new();
            List<float> revenueLossPerPeriod = new();
            List<int> totalUnitsSoldPerPeriod = new();
            List<int> unitsRefundedAmountPerPeriod = new();

            switch (dateFilter.Period.ToLower())
            {
                case "today":
                    revenueGainPerPeriod.Add(totalGain);
                    revenueLossPerPeriod.Add(totalLoss);
                    totalUnitsSoldPerPeriod.Add(totalUnitsSold);

                    int amountOfUnitsRefundedToday = refundedTransactions.Where(t => t.Status == "Refunded")
                        .SelectMany(t => t.ProductsPurchased)
                        .Sum(tPP => tPP.Amount);
                    unitsRefundedAmountPerPeriod.Add(amountOfUnitsRefundedToday);
                    break;
                case "yesterday":
                    revenueGainPerPeriod.Add(totalGain);
                    revenueLossPerPeriod.Add(totalLoss);
                    totalUnitsSoldPerPeriod.Add(totalUnitsSold);

                    int amountOfUnitsRefundedYesterday = refundedTransactions.Where(t => t.Status == "Refunded")
                        .SelectMany(t => t.ProductsPurchased)
                        .Sum(tPP => tPP.Amount);
                    unitsRefundedAmountPerPeriod.Add(amountOfUnitsRefundedYesterday);
                    break;
                case "thelastsevendays":
                    for (int i = 7; i >= 1; i--)
                    {
                        DateTime currentDate = dateFilter.EndDate.Date.AddDays(-i);
                        List<Transaction> sortedTransactions = transactions
                            .Where(t => t.DatePurchased.Date == currentDate)
                            .ToList();

                        List<Transaction> sortedRefundedTransactions = refundedTransactions
                            .Where(t => t.RefundedDate.Date == currentDate && t.Status == "Refunded")
                            .ToList();

                        float sortedTransactionsGainTotal = sortedTransactions.Sum(t => t.TotalSpent);
                        float sortedTransactionsLossTotal = sortedRefundedTransactions
                            .Sum(t => t.TotalSpent);

                        int sortUnitsSoldTotal = sortedTransactions
                            .SelectMany(transaction => transaction.ProductsPurchased)
                            .Sum(unit => unit.Amount);

                        int amountOfUnitsRefunded = sortedRefundedTransactions
                            .Where(t => t.Status == "Refunded")
                            .SelectMany(t => t.ProductsPurchased)
                            .Sum(tPP => tPP.Amount);

                        revenueGainPerPeriod.Add(sortedTransactionsGainTotal);
                        revenueLossPerPeriod.Add(sortedTransactionsLossTotal);
                        totalUnitsSoldPerPeriod.Add(sortUnitsSoldTotal);
                        unitsRefundedAmountPerPeriod.Add(amountOfUnitsRefunded);
                    }
                    break;
                case "thelastfourweeks":
                    DateTime currentWeekStart = DateTime.Now.Date.ToUniversalTime().AddDays(-(int)DateTime.Now.ToUniversalTime().DayOfWeek);
                    for (int i = 3; i >= 0; i--)
                    {
                        // Adjust the start and end dates for the current week
                        DateTime weekStart = currentWeekStart.AddDays(-i * 7);
                        DateTime weekEnd = weekStart.AddDays(6);

                        // Filter transactions for the current week
                        List<Transaction> sortedTransactions = transactions
                            .Where(t => t.DatePurchased.Date >= weekStart && t.DatePurchased.Date <= weekEnd)
                            .ToList();
                        List<Transaction> sortedRefundedTransactions = refundedTransactions
                            .Where(t => t.RefundedDate.Date >= weekStart && t.RefundedDate.Date <= weekEnd)
                            .ToList();

                        float sortedTransactionsGainTotal = sortedTransactions.Sum(t => t.TotalSpent);
                        float sortedTransactionsLossTotal = sortedRefundedTransactions.Sum(t => t.TotalSpent);

                        int sortUnitsSoldTotal = sortedTransactions
                            .SelectMany(t => t.ProductsPurchased)
                            .Sum(unit => unit.Amount);

                        int amountOfUnitsRefunded = sortedRefundedTransactions
                            .Where(t => t.Status == "Refunded")
                            .SelectMany(t => t.ProductsPurchased)
                            .Sum(tPP => tPP.Amount);

                        revenueGainPerPeriod.Add(sortedTransactionsGainTotal);
                        revenueLossPerPeriod.Add(sortedTransactionsLossTotal);
                        totalUnitsSoldPerPeriod.Add(sortUnitsSoldTotal);
                        unitsRefundedAmountPerPeriod.Add(amountOfUnitsRefunded);
                    }
                    break;
                case "thelastthreemonths":
                    DateTime currentMonthStart = new DateTime(DateTime.Now.ToUniversalTime().Year, DateTime.Now.ToUniversalTime().Month, 1);
                    for (int i = 2; i >= 0; i--)
                    {
                        // Adjust the start and end dates for the current month
                        DateTime monthStart = currentMonthStart.AddMonths(-i);
                        DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);

                        // Filter transactions for the current month
                        List<Transaction> sortedTransactions = transactions
                            .Where(t => t.DatePurchased.Date >= monthStart && t.DatePurchased.Date <= monthEnd)
                            .ToList();

                        List<Transaction> sortedRefundedTransactions = refundedTransactions
                            .Where(t => t.RefundedDate.Date >= monthStart && t.RefundedDate.Date <= monthEnd)
                            .ToList();

                        float sortedTransactionsGainTotal = sortedTransactions.Sum(t => t.TotalSpent);
                        float sortedTransactionsLossTotal = sortedRefundedTransactions.Sum(t => t.TotalSpent);

                        int sortUnitsSoldTotal = sortedTransactions
                            .SelectMany(transaction => transaction.ProductsPurchased)
                            .Sum(unit => unit.Amount);

                        int amountOfUnitsRefunded = sortedRefundedTransactions
                            .Where(t => t.Status == "Refunded")
                            .SelectMany(t => t.ProductsPurchased)
                            .Sum(tPP => tPP.Amount);

                        revenueGainPerPeriod.Add(sortedTransactionsGainTotal);
                        revenueLossPerPeriod.Add(sortedTransactionsLossTotal);
                        totalUnitsSoldPerPeriod.Add(sortUnitsSoldTotal);
                        unitsRefundedAmountPerPeriod.Add(amountOfUnitsRefunded);
                    }
                    break;
                case "thelastsixmonths":
                    DateTime current6MonthStart = new DateTime(DateTime.Now.ToUniversalTime().Year, DateTime.Now.ToUniversalTime().Month, 1);
                    for (int i = 5; i >= 0; i--)
                    {
                        // Adjust the start and end dates for the current month
                        DateTime monthStart = current6MonthStart.AddMonths(-i);
                        DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);

                        // Filter transactions for the current month
                        List<Transaction> sortedTransactions = transactions
                            .Where(t => t.DatePurchased.Date >= monthStart && t.DatePurchased.Date <= monthEnd)
                            .ToList();

                        List<Transaction> sortedRefundedTransactions = refundedTransactions
                            .Where(t => t.RefundedDate.Date >= monthStart && t.RefundedDate.Date <= monthEnd)
                            .ToList();

                        float sortedTransactionsGainTotal = sortedTransactions.Sum(t => t.TotalSpent);
                        float sortedTransactionsLossTotal = sortedRefundedTransactions.Sum(t => t.TotalSpent);

                        int sortUnitsSoldTotal = sortedTransactions
                            .SelectMany(transaction => transaction.ProductsPurchased)
                            .Sum(unit => unit.Amount);

                        int amountOfUnitsRefunded = sortedRefundedTransactions
                            .Where(t => t.Status == "Refunded")
                            .SelectMany(t => t.ProductsPurchased)
                            .Sum(tPP => tPP.Amount);

                        revenueGainPerPeriod.Add(sortedTransactionsGainTotal);
                        revenueLossPerPeriod.Add(sortedTransactionsLossTotal);
                        totalUnitsSoldPerPeriod.Add(sortUnitsSoldTotal);
                        unitsRefundedAmountPerPeriod.Add(amountOfUnitsRefunded);
                    }
                    break;
                case "thelasttwelvemonths":
                    DateTime current12MonthStart = new DateTime(DateTime.Now.ToUniversalTime().Year, DateTime.Now.ToUniversalTime().Month, 1);
                    for (int i = 11; i >= 0; i--)
                    {
                        // Adjust the start and end dates for the current month
                        DateTime monthStart = current12MonthStart.AddMonths(-i);
                        DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);

                        // Filter transactions for the current month
                        List<Transaction> sortedTransactions = transactions
                            .Where(t => t.DatePurchased.Date >= monthStart && t.DatePurchased.Date <= monthEnd)
                            .ToList();

                        List<Transaction> sortedRefundedTransactions = refundedTransactions
                            .Where(t => t.RefundedDate.Date >= monthStart && t.RefundedDate.Date <= monthEnd)
                            .ToList();

                        float sortedTransactionsGainTotal = sortedTransactions.Sum(t => t.TotalSpent);
                        float sortedTransactionsLossTotal = sortedRefundedTransactions.Sum(t => t.TotalSpent);

                        int sortUnitsSoldTotal = sortedTransactions
                            .SelectMany(transaction => transaction.ProductsPurchased)
                            .Sum(unit => unit.Amount);

                        int amountOfUnitsRefunded = sortedRefundedTransactions
                            .Where(t => t.Status == "Refunded")
                            .SelectMany(t => t.ProductsPurchased)
                            .Sum(tPP => tPP.Amount);

                        revenueGainPerPeriod.Add(sortedTransactionsGainTotal);
                        revenueLossPerPeriod.Add(sortedTransactionsLossTotal);
                        totalUnitsSoldPerPeriod.Add(sortUnitsSoldTotal);
                        unitsRefundedAmountPerPeriod.Add(amountOfUnitsRefunded);
                    }
                    break;
                case "custom":
                    // Calculate the duration between StartDate and EndDate
                    TimeSpan duration = dateFilter.EndDate - dateFilter.StartDate;
                    if (duration <= TimeSpan.FromDays(7))
                    {
                        // If duration is less than or equal to 7 days, display as days
                        for (DateTime date = dateFilter.StartDate; date <= dateFilter.EndDate; date = date.AddDays(1))
                        {
                            // Filter transactions for the current date
                            List<Transaction> sortedTransactions = transactions
                                .Where(t => t.DatePurchased.Date == date.Date)
                                .ToList();

                            List<Transaction> sortedRefundedTransactions = refundedTransactions
                                .Where(t => t.RefundedDate.Date == date.Date)
                                .ToList();

                            float sortedTransactionsGainTotal = sortedTransactions.Sum(t => t.TotalSpent);
                            float sortedTransactionsLossTotal = sortedRefundedTransactions.Sum(t => t.TotalSpent);

                            int sortUnitsSoldTotal = sortedTransactions
                                .SelectMany(transaction => transaction.ProductsPurchased)
                                .Sum(unit => unit.Amount);

                            int amountOfUnitsRefunded = sortedRefundedTransactions
                                .Where(t => t.Status == "Refunded")
                                .SelectMany(t => t.ProductsPurchased)
                                .Sum(tPP => tPP.Amount);

                            revenueGainPerPeriod.Add(sortedTransactionsGainTotal);
                            revenueLossPerPeriod.Add(sortedTransactionsLossTotal);
                            totalUnitsSoldPerPeriod.Add(sortUnitsSoldTotal);
                            unitsRefundedAmountPerPeriod.Add(amountOfUnitsRefunded);
                        }
                    }
                    else if (duration <= TimeSpan.FromDays(30))
                    {
                        DateTime endDate = dateFilter.EndDate;
                        DateTime startDate = dateFilter.StartDate;

                        while (startDate <= endDate)
                        {
                            DateTime endOfWeek = startDate.AddDays(6 - (int)startDate.DayOfWeek); // Saturday is DayOfWeek.Saturday == 6

                            if (endOfWeek > endDate)
                            {
                                endOfWeek = endDate;
                            }

                            DateTime startOfWeek = endOfWeek.AddDays(-6); // Start of the week is Sunday

                            if (startOfWeek < startDate)
                            {
                                startOfWeek = startDate;
                            }

                            List<Transaction> sortedTransactions = transactions
                                .Where(t => t.DatePurchased.Date >= startOfWeek.Date && t.DatePurchased.Date <= endOfWeek.Date)
                                .ToList();

                            List<Transaction> sortedRefundedTransactions = refundedTransactions
                                .Where(t => t.RefundedDate.Date >= startOfWeek.Date && t.RefundedDate.Date <= endOfWeek.Date)
                                .ToList();

                            float sortedTransactionsGainTotal = sortedTransactions.Sum(t => t.TotalSpent);
                            float sortedTransactionsLossTotal = sortedRefundedTransactions.Sum(t => t.TotalSpent);
                            int sortUnitsSoldTotal = sortedTransactions.SelectMany(transaction => transaction.ProductsPurchased).Sum(unit => unit.Amount);
                            int amountOfUnitsRefunded = sortedRefundedTransactions.Where(t => t.Status == "Refunded").SelectMany(t => t.ProductsPurchased).Sum(tPP => tPP.Amount);

                            revenueGainPerPeriod.Add(sortedTransactionsGainTotal);
                            revenueLossPerPeriod.Add(sortedTransactionsLossTotal);
                            totalUnitsSoldPerPeriod.Add(sortUnitsSoldTotal);
                            unitsRefundedAmountPerPeriod.Add(amountOfUnitsRefunded);

                            if (endOfWeek < endDate && startDate.AddDays(7) > endDate)
                            {
                                startOfWeek = endOfWeek.AddDays(1); // Start of the next week
                                endOfWeek = endDate; // Set the end date as the end of the next week

                                sortedTransactions = transactions
                                    .Where(t => t.DatePurchased.Date >= startOfWeek.Date && t.DatePurchased.Date <= endOfWeek.Date)
                                    .ToList();

                                sortedRefundedTransactions = refundedTransactions
                                    .Where(t => t.RefundedDate.Date >= startOfWeek.Date && t.RefundedDate.Date <= endOfWeek.Date)
                                    .ToList();

                                sortedTransactionsGainTotal = sortedTransactions.Sum(t => t.TotalSpent);
                                sortedTransactionsLossTotal = sortedRefundedTransactions.Sum(t => t.TotalSpent);
                                sortUnitsSoldTotal = sortedTransactions.SelectMany(transaction => transaction.ProductsPurchased).Sum(unit => unit.Amount);
                                amountOfUnitsRefunded = sortedRefundedTransactions.Where(t => t.Status == "Refunded").SelectMany(t => t.ProductsPurchased).Sum(tPP => tPP.Amount);

                                revenueGainPerPeriod.Add(sortedTransactionsGainTotal);
                                revenueLossPerPeriod.Add(sortedTransactionsLossTotal);
                                totalUnitsSoldPerPeriod.Add(sortUnitsSoldTotal);
                                unitsRefundedAmountPerPeriod.Add(amountOfUnitsRefunded);
                            }

                            startDate = endOfWeek.AddDays(1);
                        }
                    }
                    else
                    {
                        // If duration is greater than 30 days, display as months
                        DateTime currentDateHolder = dateFilter.StartDate.Date;
                        while (currentDateHolder <= dateFilter.EndDate)
                        {
                            // Filter transactions for the current month
                            List<Transaction> sortedTransactions = transactions
                                .Where(t => t.DatePurchased.Date.Month == currentDateHolder.Date.Month && t.DatePurchased.Date.Year == currentDateHolder.Date.Year)
                                .ToList();

                            List<Transaction> sortedRefundedTransactions = refundedTransactions
                                .Where(t => t.RefundedDate.Date.Month == currentDateHolder.Date.Month && t.RefundedDate.Date.Year == currentDateHolder.Date.Year)
                                .ToList();

                            float sortedTransactionsGainTotal = sortedTransactions.Sum(t => t.TotalSpent);
                            float sortedTransactionsLossTotal = sortedRefundedTransactions.Sum(t => t.TotalSpent);

                            int sortUnitsSoldTotal = sortedTransactions
                                .SelectMany(transaction => transaction.ProductsPurchased)
                                .Sum(unit => unit.Amount);

                            int amountOfUnitsRefunded = sortedRefundedTransactions
                                .Where(t => t.Status == "Refunded")
                                .SelectMany(t => t.ProductsPurchased)
                                .Sum(tPP => tPP.Amount);

                            revenueGainPerPeriod.Add(sortedTransactionsGainTotal);
                            revenueLossPerPeriod.Add(sortedTransactionsLossTotal);
                            totalUnitsSoldPerPeriod.Add(sortUnitsSoldTotal);
                            unitsRefundedAmountPerPeriod.Add(amountOfUnitsRefunded);

                            currentDateHolder = currentDateHolder.AddMonths(1);
                        }
                    }
                    break;


                default:
                    throw new ArgumentException("Invalid period label specified");
            }
            return new AnalyticsFunctionDTO
            {
                RevenueGainPerPeriod = revenueGainPerPeriod,
                RevenueLossPerPeriod = revenueLossPerPeriod,
                TotalUnitsSoldPerPeriod = totalUnitsSoldPerPeriod,
                UnitsRefundedAmountPerPeriod = unitsRefundedAmountPerPeriod
            };
        }
    }
}