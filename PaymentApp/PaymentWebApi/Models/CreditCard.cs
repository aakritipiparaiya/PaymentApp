using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaymentWebApi.Models
{
    public class CreditCard
    {
        public string CreditCardNo { get; set; }
        public string CardHolder { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string SecurityCode { get; set; }
        public decimal? Amount { get; set; }
    }
}