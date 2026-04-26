using System.ComponentModel.DataAnnotations;

namespace ApplePaySimulation.Models.ViewModels.Authentication
{
    public class ResetPasswordEditViewModel
    {
        [Required(ErrorMessage = "The New Password is Required")]
        public string NewPassword { get; set; }
        [Required(ErrorMessage = "The Confirm Password is Required")]
        public string ConfirmPassword { get; set; }
        public string userId { get; set; }
        public string token { get; set; }
    }
}
