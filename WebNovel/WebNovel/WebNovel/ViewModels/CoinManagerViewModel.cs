using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebNovel.Models;

namespace WebNovel.ViewModels
{
    public class CoinManagerViewModel
    {
        public List<CoinPackage> ActivePackages { get; set; }
        public List<PromoCode> ActivePromos { get; set; }
        public List<Wallet> UserWallets { get; set; }
    }
}