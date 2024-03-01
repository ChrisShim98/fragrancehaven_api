namespace fragrancehaven_api.DTOs
{
    public class DateFilter
    {
        public string Period { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MyProperty { get; set; }

        public void SetDates()
        {
            switch (Period.ToLower())
            {
                case "today":
                    StartDate = DateTime.Now.Date;
                    EndDate = DateTime.Now.AddTicks(-1);
                    break;
                case "yesterday":
                    StartDate = DateTime.Now.Date.AddDays(-1);
                    EndDate = StartDate.AddDays(1).AddTicks(-1);
                    break;
                case "thelastsevendays":
                    StartDate = DateTime.Now.Date.AddDays(-6);
                    EndDate = DateTime.Now.AddDays(1).AddTicks(-1);
                    break;
                case "thelastfourweeks":
                    DateTime today = DateTime.Now.Date;
                    DateTime lastSaturday = today.AddDays(-(int)today.DayOfWeek);
                    StartDate = lastSaturday.AddDays(-27);
                    EndDate = today.AddDays(1).AddTicks(-1);
                    break;
                case "thelastthreemonths":
                    StartDate = DateTime.Now.Date.AddMonths(-3);
                    EndDate = DateTime.Now.AddDays(1).AddTicks(-1);
                    break;
                case "thelastsixmonths":
                    StartDate = DateTime.Now.Date.AddMonths(-6);
                    EndDate = DateTime.Now.AddDays(1).AddTicks(-1);
                    break;
                case "thelasttwelvemonths":
                    StartDate = DateTime.Now.Date.AddMonths(-12);
                    EndDate = DateTime.Now.AddDays(1).AddTicks(-1);
                    break;
                case "custom":
                    // Custom period, do nothing as StartDate and EndDate will be set separately
                    break;
                default:
                    throw new ArgumentException("Invalid period specified");
            }

            // Convert local time to UTC
            StartDate = StartDate.ToUniversalTime();
            EndDate = EndDate.ToUniversalTime();
        }
    }
}