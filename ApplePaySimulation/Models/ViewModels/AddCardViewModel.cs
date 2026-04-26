namespace ApplePaySimulation.Models.ViewModels
{
    public class AddCardViewModel
    {
         public string CardNumber { get; set; }
    public string Expiry { get; set; }  // For MM/YY from form
    public string Cvv { get; set; }
    public string CardHolderName { get; set; }
    public int ExpiryMonth { get; set; }  // Parsed from Expiry
    public int ExpiryYear { get; set; }   // Parsed from Expiry
    }
}
