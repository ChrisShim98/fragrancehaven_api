namespace fragrancehaven_api.DTOs
{
    public class AnalyticsFunctionDTO
    {
        public List<float> RevenueGainPerPeriod { get; set; }
        public List<float> RevenueLossPerPeriod { get; set; }
        public List<int> TotalUnitsSoldPerPeriod { get; set; }
        public List<int> UnitsRefundedAmountPerPeriod { get; set; }
    }
}