namespace fragrancehaven_api.Entity
{
    public class PurchasedProduct
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BrandName { get; set; }
        public float PurchasedPrice { get; set; }
        public int Amount { get; set; } = 1;
    }
}