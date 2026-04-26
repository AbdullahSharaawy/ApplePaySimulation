using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ApplePaySimulation.Database.Entities
{
    public class CreditCard
    {
        public int Id { get; set; }
        [Required]
        public string CardNumber { get; set; }
        [Required]
        public string CVV { get; set; } // التوكن الآمن من بوابة الدفع
        [Required(ErrorMessage = "Expiry month is required.")]
        [Range(1, 12, ErrorMessage = "Month must be between 1 and 12.")]
        [Display(Name = "Expiry Month")]
        public int ExpiryMonth { get; set; } // مثال: 12
        [Required(ErrorMessage = "Expiry year is required.")]
        [Range(2026, 2040, ErrorMessage = "Please enter a valid future year.")]

        [Display(Name = "Expiry Year")]

        public int ExpiryYear { get; set; }  // مثال: 2028 أو 28

        // (اختياري) حقل مساعد لعرض التاريخ في الواجهة بصيغة MM/YY
        // NotMapped تعني أن هذا الحقل لن يتم إنشاؤه في قاعدة البيانات
        [NotMapped]
        [Display(Name = "Expire")]
        public string FormattedExpiry => $"{ExpiryMonth:D2}/{ExpiryYear % 100:D2}";
        // Foreign Key
        [Required]
        [Display(Name = "Card Owner")]
        [ForeignKey("User")]
        public string UserId { get; set; }

        // Navigation Property (1:1)
        public User User { get; set; }

        // Navigation Property (1:Many) - البطاقة لها عدة عمليات
        public ICollection<Transaction> Transactions { get; set; }
    }
}
