using ApplePaySimulation.Database.Entities;
using ApplePaySimulation.Models.ViewModels;
using ApplePaySimulation.Repository.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ApplePaySimulation.Controllers
{
    
    public class TerminalController : Controller
    {
        private readonly UserManager<User> _userRepo;
        private readonly IRepository<CreditCard> _cardRepo;
        private readonly IRepository<Transaction> _transactionRepo;
        private readonly IConfiguration _configuration;

        public TerminalController(
            UserManager<User> userRepo,
            IRepository<CreditCard> cardRepo,
            IRepository<Transaction> transactionRepo,
            IConfiguration configuration)
        {
            _userRepo = userRepo;
            _cardRepo = cardRepo;
            _transactionRepo = transactionRepo;
            _configuration = configuration;
        }

        // GET /Terminal  → renders Views/Terminal/Index.cshtml
        public IActionResult Index()
        {
            return View();
        }

        // POST /Terminal/ProcessPayment  (called by fetch() in the terminal JS)
        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestViewModel request)
        {
            try
            {
                // 1. Get the Seller and their linked Credit Card
                string sellerId = _configuration["SELLERID"];

                var seller = await _userRepo.FindByIdAsync(sellerId);
                if (seller == null)
                    return BadRequest("Seller not found in the database.");

                var sellerCards = await _cardRepo.GetAllByFilter(c => c.UserId == sellerId);
                var sellerCard = sellerCards.FirstOrDefault();
                if (sellerCard == null)
                    return BadRequest("Seller terminal is not configured properly.");

                // 2. Identify the Buyer from the Wallet ID
                string buyerId = request.BuyerWalletId;
                var buyer = await _userRepo.FindByIdAsync(buyerId);
                if (buyer == null)
                    return BadRequest("Buyer not found.");

                if (buyer.WalletBalance < request.AmountSAR)
                    return BadRequest("Insufficient funds in buyer's wallet.");

                // 3. Process the financial transfer
                decimal fee = int.Parse(_configuration["Fee"] ?? "0");
                buyer.WalletBalance -= (request.AmountSAR + fee);
                seller.WalletBalance += request.AmountSAR;

                await _userRepo.UpdateAsync(buyer);
                await _userRepo.UpdateAsync(seller);

                // 4. Create the Transaction Record
                var transaction = new Transaction
                {
                    Amount = request.AmountSAR,
                    CreatedAt = DateTime.Now,
                    CreditCardId = sellerCard.Id
                };
                await _transactionRepo.Create(transaction);

                return Json(new
                {
                    success = true,
                    message = "Payment successful",
                    reference = $"XN-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
