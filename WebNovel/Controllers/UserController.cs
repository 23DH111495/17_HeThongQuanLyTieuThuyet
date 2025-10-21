using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
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

        #region tạo thanh toán
        public ActionResult PaypalSuccess(string paymentId, string token, string PayerID, int packageId)
        {
            var apiContext = PaypalConfiguration.GetAPIContext();

            var paymentExecution = new PaymentExecution() { payer_id = PayerID };
            var payment = new Payment() { id = paymentId };

            var executedPayment = payment.Execute(apiContext, paymentExecution);

            if (executedPayment.state.ToLower() != "approved")
                return RedirectToAction("FailureView");

           

            return RedirectToAction("SuccessView");
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
    }
}