namespace fragrancehaven_api.DTOs
{
    public class AnalyticsDTO
    {
        public float TotalRevenue { get; set; }
        public float TotalGain { get; set; }
        public float TotalLoss { get; set; }
        public List<string> PeriodLabel { get; set; }
        public List<float> RevenueGainPerPeriod { get; set; }
        public List<float> RevenueLossPerPeriod { get; set; }
        public int TotalUnitsSold { get; set; }
        public int TotalUnitsRefunded { get; set; }
        public List<int> TotalUnitsSoldPerPeriod { get; set; }
        public List<string> UnitSoldNamePerPeriod { get; set; }
        public List<int> UnitsSoldAmountPerPeriod { get; set; }
        public List<int> UnitsRefundedAmountPerPeriod { get; set; }

        public void GeneratePeriodLabels(DateFilter dateFilter)
        {
            PeriodLabel = new List<string>();

            switch (dateFilter.Period.ToLower())
            {
                case "today":
                    PeriodLabel.Add("Today");
                    break;
                case "yesterday":
                    PeriodLabel.Add("Yesterday");
                    break;
                case "thelastsevendays":
                    // Add labels for the last seven days including today
                    DateTime currentDate = DateTime.Now.Date.ToUniversalTime();
                    for (int i = 6; i >= 0; i--)
                    {
                        if (i == 0)
                        {
                            PeriodLabel.Add("Today");
                        }
                        else if (i == 1)
                        {
                            PeriodLabel.Add("Yesterday");
                        }
                        else
                        {
                            PeriodLabel.Add(currentDate.AddDays(-i).ToString("dddd"));
                        }
                    }
                    break;
                case "thelastfourweeks":
                    // Add labels for the last four weeks including the current week
                    DateTime currentWeekStart = DateTime.Now.Date.ToUniversalTime().AddDays(-(int)DateTime.Now.ToUniversalTime().DayOfWeek);
                    for (int i = 3; i >= 0; i--)
                    {
                        PeriodLabel.Add(i == 0 ? "Current Week" : currentWeekStart.AddDays(-7 * i).ToString("MMM dd") + " - " + currentWeekStart.AddDays(-7 * (i - 1) - 1).ToString("MMM dd"));
                    }
                    break;
                case "thelastthreemonths":
                    // Add labels for the last three months
                    DateTime currentMonthStart = new DateTime(DateTime.Now.ToUniversalTime().Year, DateTime.Now.ToUniversalTime().Month, 1);
                    for (int i = 2; i >= 0; i--)
                    {
                        PeriodLabel.Add(currentMonthStart.AddMonths(-i).ToString("MMMM"));
                    }
                    break;
                case "thelastsixmonths":
                    // Add labels for the last six months
                    DateTime current6MonthStart = new DateTime(DateTime.Now.ToUniversalTime().Year, DateTime.Now.ToUniversalTime().Month, 1);
                    for (int i = 5; i >= 0; i--)
                    {
                        PeriodLabel.Add(current6MonthStart.AddMonths(-i).ToString("MMMM"));
                    }
                    break;
                case "thelasttwelvemonths":
                    // Add labels for the last twelve months
                    DateTime current12MonthStart = new DateTime(DateTime.Now.ToUniversalTime().Year, DateTime.Now.ToUniversalTime().Month, 1);
                    for (int i = 11; i >= 0; i--)
                    {
                        PeriodLabel.Add(current12MonthStart.AddMonths(-i).ToString("MMMM"));
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
                            PeriodLabel.Add(date.ToString("MMM dd"));
                        }
                    }
                    else if (duration <= TimeSpan.FromDays(30))
                    {
                        // If duration is greater than 7 days but less than or equal to 30 days, display as weeks
                        DateTime endDate = dateFilter.EndDate;
                        for (DateTime startDate = endDate; startDate <= dateFilter.EndDate; startDate = startDate.AddDays(7))
                        {
                            DateTime startOfWeek = startDate.AddDays(-6);
                            // Ensure that the start of the week is within the specified period
                            if (startOfWeek < dateFilter.StartDate)
                            {
                                startOfWeek = dateFilter.StartDate;
                            }
                            PeriodLabel.Add(startOfWeek.ToString("MMM dd") + " - " + startDate.ToString("MMM dd"));
                        }
                    }
                    else
                    {
                        // If duration is greater than 30 days, display as months
                        DateTime currentDateHolder = dateFilter.EndDate;
                        while (currentDateHolder <= dateFilter.EndDate)
                        {
                            PeriodLabel.Add(currentDateHolder.ToString("MMMM"));
                            currentDateHolder = currentDateHolder.AddMonths(1);
                        }
                    }
                    break;

                default:
                    throw new ArgumentException("Invalid period specified");
            }
        }
    }
}