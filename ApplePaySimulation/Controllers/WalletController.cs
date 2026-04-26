using ApplePaySimulation.Database.Entities;
using ApplePaySimulation.Models.ViewModels;
using ApplePaySimulation.Models.ViewModels.Authentication;
using ApplePaySimulation.Repository.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ApplePaySimulation.Controllers
{
    [Authorize]
    public class WalletController : Controller
    {
        private readonly UserManager<User> _userRepo;
        private readonly IRepository<CreditCard> _cardRepo;
        private readonly IRepository<Transaction> _transactionRepo;
        private readonly SignInManager<User> _signInManager;
        public WalletController(
            UserManager<User> userRepo,
            IRepository<CreditCard> cardRepo,
            IRepository<Transaction> transactionRepo,
            SignInManager<User> signInManager)
        {
            _userRepo = userRepo;
            _cardRepo = cardRepo;
            _transactionRepo = transactionRepo; 
            _signInManager = signInManager;
        }

        // Returns the HTML for Prototype 2

        // ✅ Fix
        public async Task<IActionResult> Index()
        {
            var user = await _userRepo.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "User");

            var cards = await _cardRepo.GetAllByFilter(c => c.UserId == user.Id);
            var activeCard = cards.FirstOrDefault();

            List<dynamic> txList = new();
            if (activeCard != null)
            {
                var transactions = await _transactionRepo.GetAllByFilter(t => t.CreditCardId == activeCard.Id);
                txList = transactions.OrderByDescending(t => t.CreatedAt).Cast<dynamic>().ToList();
            }

            ViewBag.UserName = user.UserName ?? "User";
            ViewBag.FullName = user.FullName ?? "User";
            ViewBag.UserId = $"{user.Id.ToString()}";
            ViewBag.WalletBalance = user.WalletBalance;
            ViewBag.CardLast4 = activeCard?.CardNumber[^4..] ?? "••••";
            ViewBag.CardExpiry = activeCard != null
                                    ? $"{activeCard.ExpiryMonth:D2}/{activeCard.ExpiryYear % 100:D2}"
                                    : "—";
            ViewBag.Transactions = txList;

            return View();
        }



        // 2. Fetch Dashboard Data (AJAX Call)
        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            var user = await _userRepo.GetUserAsync(User);
            if (user == null) return NotFound();

            var card = await _cardRepo.GetAllByFilter(c => c.UserId == user.Id);
            var activeCard = card.FirstOrDefault();

            // Get transactions linked to this user's card (if they have one)
            var history = new object();
            if (activeCard != null)
            {
                var transactions = await _transactionRepo.GetAllByFilter(t => t.CreditCardId == activeCard.Id);
                history = transactions.OrderByDescending(t => t.CreatedAt)
                                      .Select(t => new {
                                         
                                          amountSAR = t.Amount,
                                          time = t.CreatedAt.ToString("MMM dd, yyyy")
                                      }).ToList();
            }

            return Json(new
            {
                balance = user.WalletBalance,
                cardLastFour = activeCard?.CardNumber ?? "None",
                history = history
            });
        }

        // 3. Add New Card (AJAX Call)
        [HttpPost]
      
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCard(AddCardViewModel model)
        {
            var user = await _userRepo.GetUserAsync(User);
            if (user == null) return NotFound();

            // Parse "MM/YY" → ExpiryMonth / ExpiryYear
            var parts = (model.Expiry ?? "").Split('/');
            int month = parts.Length > 0 && int.TryParse(parts[0], out var m) ? m : 0;
            int year = parts.Length > 1 && int.TryParse(parts[1], out var y) ? 2000 + y : 0;

            var newCard = new CreditCard
            {
                UserId = user.Id,
                CardNumber = model.CardNumber?.Replace(" ", ""),
                ExpiryMonth = month,
                ExpiryYear = year,
                CVV = model.Cvv
            };

            await _cardRepo.Create(newCard);

            return RedirectToAction("Index"); // ✅ triggers full reload with ViewBag
        }
        // Add this method to your WalletController
        [HttpPost]
        public async Task<IActionResult> AddFunds([FromBody] AddFundsViewModel model)
        {
            var user = await _userRepo.GetUserAsync(User);
            if (user == null) return NotFound();

            user.WalletBalance += model.Amount;
            var result = await _userRepo.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Json(new { success = true, newBalance = user.WalletBalance });
        }
    }
}
