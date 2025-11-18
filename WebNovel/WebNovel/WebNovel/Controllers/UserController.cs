using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebNovel.Data;
using WebNovel.Models;
using WebNovel.Models.ViewModels;

namespace WebNovel.Controllers
{
    public class UserController : Controller
    {
        private DarkNovelDbContext db = new DarkNovelDbContext();
        public ActionResult Coin()
        {
            var activePackages = db.CoinPackages
                                   .Where(c => c.IsActive)
                                   .OrderBy(c => c.SortOrder)
                                   .ToList();
            return View("Coin", activePackages);
        }

        #region thanh toán paypal
        private string PaypalClientId = ConfigurationManager.AppSettings["PaypalClientId"];
        private string PaypalSecret = ConfigurationManager.AppSettings["PaypalSecret"];
        
        [HttpPost]
        public ActionResult PayWithPaypal(int packageId)
        {
            var package = db.CoinPackages.Find(packageId);
            if (package == null || package.PriceUSD <= 0)
                return RedirectToAction("FailureView");

            var apiContext = PaypalConfiguration.GetAPIContext();

            var payer = new Payer() { payment_method = "paypal" };
            var redirectUrls = new RedirectUrls()
            {
                cancel_url = Url.Action("FailureView", "User", null, Request.Url.Scheme),
                return_url = Url.Action("PaypalSuccess", "User", new { packageId = packageId }, Request.Url.Scheme)
            };

            var amount = new Amount()
            {
                currency = "USD",
                total = package.PriceUSD.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
            };

            var transactionList = new List<Transaction>
            {
                new Transaction
                {
                    description = $"Mua {package.CoinAmount} coin",
                    amount = amount
                }
            };

            var payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirectUrls
            };

            try
            {
                var createdPayment = payment.Create(apiContext);
                var approvalUrl = createdPayment.links.FirstOrDefault(
                    l => l.rel.Equals("approval_url", StringComparison.OrdinalIgnoreCase))?.href;
                return Redirect(approvalUrl);
            }
            catch (PayPal.PaymentsException ex)
            {
                ViewBag.PaypalError = ex.Message;
                return View("FailureView");
            }
        }

        #region tạo thanh toán cũ
        //public ActionResult PaypalSuccess(string paymentId, string token, string PayerID, int packageId)
        //{
        //    var apiContext = PaypalConfiguration.GetAPIContext();

        //    var paymentExecution = new PaymentExecution() { payer_id = PayerID };
        //    var payment = new Payment() { id = paymentId };

        //    var executedPayment = payment.Execute(apiContext, paymentExecution);

        //    if (executedPayment.state.ToLower() != "approved")
        //        return RedirectToAction("FailureView");



        //    return RedirectToAction("SuccessView");
        //}
        #endregion
        #region tạo thanh toán mới
        public ActionResult PaypalSuccess(string paymentId, string token, string PayerID, int packageId)
        {
            var apiContext = PaypalConfiguration.GetAPIContext();

            var paymentExecution = new PaymentExecution() { payer_id = PayerID };
            var payment = new Payment() { id = paymentId };

            var executedPayment = payment.Execute(apiContext, paymentExecution);

            if (executedPayment.state.ToLower() != "approved")
                return RedirectToAction("FailureView");

            // --- Kiểm tra đăng nhập ---
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            var package = db.CoinPackages.Find(packageId);
            if (package == null)
                return RedirectToAction("FailureView");

            // --- Lấy hoặc tạo ví ---
            var wallet = db.Wallets.FirstOrDefault(w => w.UserId == userId);
            if (wallet == null)
            {
                wallet = new Wallet
                {
                    UserId = userId,
                    CoinBalance = 0,
                    TotalCoinsSpent = 0,
                    TotalCoinsEarned = 0,
                    TotalTopUp = 0,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    LastUpdated = DateTime.Now
                };
                db.Wallets.Add(wallet);
            }

            // --- Tính toán coin & số dư ---
            decimal beforeBalance = wallet.CoinBalance;
            decimal coinReceived = package.CoinAmount;
            decimal afterBalance = beforeBalance + coinReceived;

            // --- Cập nhật ví ---
            wallet.CoinBalance = afterBalance; 
            wallet.TotalCoinsEarned += coinReceived; 
            wallet.TotalTopUp += package.PriceUSD;
            wallet.LastUpdated = DateTime.Now;
            wallet.UpdatedAt = DateTime.Now;

            // --- Ghi lịch sử mua coin ---
            var purchaseHistory = new CoinPurchaseHistory
            {
                UserId = userId,
                PackageId = packageId,
                CoinsPurchased = package.CoinAmount,
                BonusCoins = 0,
                PricePaid = package.PriceUSD,
                Currency = "USD",
                PaymentMethod = "PayPal",
                PaymentGateway = "PayPal",
                TransactionId = paymentId,
                GatewayTransactionId = executedPayment.id,
                PaymentStatus = "Completed",
                PaymentDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            db.PurchaseHistories.Add(purchaseHistory);
            db.SaveChanges(); 

            // --- Ghi giao dịch chi tiết ---
            var transaction = new CoinTransaction
            {
                UserId = userId,
                TransactionType = "Purchase",
                Amount = coinReceived,     
                BalanceBefore = beforeBalance,  
                BalanceAfter = afterBalance,
                Description = $"Nạp {coinReceived} coin qua PayPal (PackageID: {packageId})",
                ReferenceId = paymentId,
                RelatedPurchaseId = purchaseHistory.Id,
                CreatedAt = DateTime.Now
            };
            db.CoinTransactions.Add(transaction);

            db.SaveChanges();

            // --- Dữ liệu hiển thị ---
            ViewBag.PackageName = package.Name;
            ViewBag.CoinAdded = coinReceived;
            ViewBag.PaymentId = paymentId;

            return View("SuccessView");
        }



        #endregion


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    
        public ActionResult SuccessView()
        {
            return View();
        }

        public ActionResult FailureView()
        {
            return View();
        }
        #endregion
        #region Bookmarks 
        public ActionResult Bookmarks()
        {
            // 1. Kiểm tra đăng nhập
            if (Session["IsLoggedIn"] == null || (bool)Session["IsLoggedIn"] == false)
            {
                // Nhớ sửa "Bookmark" -> "Bookmarks" (có s) cho đúng tên hàm
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Bookmarks", "User") });
            }

            // 2. Lấy ReaderId (Ưu tiên lấy từ Session)
            int readerId = 0;
            if (Session["ReaderId"] != null)
            {
                readerId = (int)Session["ReaderId"];
            }
            else
            {
                int userId = (int)Session["UserId"];
                var reader = db.Readers.FirstOrDefault(r => r.UserId == userId);
                if (reader == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy hồ sơ Độc giả.";
                    // Trả về list Bookmark rỗng để không lỗi View
                    return View(new List<Bookmark>());
                }
                readerId = reader.Id;
                Session["ReaderId"] = reader.Id;
            }

            // 3. Truy vấn trực tiếp Model Bookmark
            // Không dùng .Select() để biến đổi, giữ nguyên kiểu Bookmark
            var bookmarks = db.Bookmarks
                .Where(b => b.ReaderId == readerId)
                .Include(b => b.Novel)                                      // Lấy thông tin truyện
                .Include(b => b.Novel.Author)                               // Lấy tác giả (thông qua truyện)
                .Include(b => b.Novel.NovelGenres.Select(ng => ng.Genre))   // Lấy thể loại (thông qua truyện)
                .OrderByDescending(b => b.CreatedAt)                        // Mới nhất lên đầu
                .ToList();

            // 4. Trả về thẳng Model Bookmark
            return View(bookmarks);
        }
        #endregion
    }
}