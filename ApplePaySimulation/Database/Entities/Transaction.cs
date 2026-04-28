using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplePaySimulation.Database.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Transaction amount is required.")]
        [Range(0.01, 1000000, ErrorMessage = "Amount must be greater than zero.")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }
        [Display(Name = "Date & Time")]

        public DateTime CreatedAt { get; set; }= DateTime.Now;
        public string TransactionType { get; set; } 
        // Foreign Key
        [Required]
        [ForeignKey("CreditCard")]
        public int CreditCardId { get; set; }
        public CreditCard CreditCard { get; set; }
    }
}
