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
        // Add this action inside TerminalController, after ProcessPayment
        [HttpGet]
        public async Task<IActionResult> GetHistory()
        {
            try
            {
                string sellerId = _configuration["SELLERID"];

                var sellerCards = await _cardRepo.GetAllByFilter(c => c.UserId == sellerId);
                var sellerCard = sellerCards.FirstOrDefault();
                if (sellerCard == null)
                    return Json(new { success = false, transactions = Array.Empty<object>() });

                var transactions = await _transactionRepo.GetAllByFilter(t => t.CreditCardId == sellerCard.Id);

                var result = transactions
                    .OrderByDescending(t => t.CreatedAt)
                    .Select(t => new
                    {
                        amount = t.Amount,
                        type = t.TransactionType,
                        createdAt = t.CreatedAt.ToString("dd MMM yyyy, hh:mm tt")
                    });

                return Json(new { success = true, transactions = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
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

                //  Get the Buyer and their linked Credit Card
                var buyerCards = await _cardRepo.GetAllByFilter(c => c.UserId == buyerId);
                var buyerCard = buyerCards.FirstOrDefault();
                if (buyerCard == null)
                    return BadRequest("Buyer  is not configured properly.");

                // 3. Process the financial transfer
                decimal fee = int.Parse(_configuration["Fee"] ?? "0");
                buyer.WalletBalance -= (request.AmountSAR + fee);
                seller.WalletBalance += request.AmountSAR;

                await _userRepo.UpdateAsync(buyer);
                await _userRepo.UpdateAsync(seller);

                // 4. Create the Transaction Record
                var SellerTransaction = new Transaction
                {
                    Amount = request.AmountSAR,
                    CreatedAt = DateTime.Now,
                    CreditCardId = sellerCard.Id,
                    TransactionType = "receive"
                };
                var BuyerTransaction = new Transaction
                {
                    Amount = request.AmountSAR,
                    CreatedAt = DateTime.Now,
                    CreditCardId = buyerCard.Id,
                    TransactionType="send"
                };
                await _transactionRepo.Create(SellerTransaction);
                await _transactionRepo.Create(BuyerTransaction);
                return Json(new
                {
                    success = true,
                    message = "Payment successful"
                   
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
