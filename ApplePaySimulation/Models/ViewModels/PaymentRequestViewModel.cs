namespace ApplePaySimulation.Models.ViewModels
{
    public class PaymentRequestViewModel
    {
        public decimal AmountSAR { get; set; }
        public string BuyerWalletId { get; set; } 
        public string CurrencyOriginal { get; set; } // e.g., "USD"
        public decimal AmountOriginal { get; set; }
    }
}
