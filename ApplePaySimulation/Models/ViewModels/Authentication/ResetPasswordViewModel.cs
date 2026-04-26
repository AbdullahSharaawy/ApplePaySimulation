using System.ComponentModel.DataAnnotations;

namespace ApplePaySimulation.Models.ViewModels.Authentication
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please Enter a valid Email.")]
        public string Email { get; set; }
    }
}
