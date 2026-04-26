using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ApplePaySimulation.Database.Entities
{
    public class User:IdentityUser
    {
        [Display(Name = "Wallet Balance")]
        [Range(0, double.MaxValue, ErrorMessage = "Balance cannot be negative.")]
        public decimal WalletBalance { get; set; } = 0;
        [Required(ErrorMessage ="The Name is Required.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }
        // Navigation Property (1:1) - بطاقة واحدة فقط
        public CreditCard CreditCard { get; set; }
    }
}
