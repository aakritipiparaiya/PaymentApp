using CheapPaymentGateway;
using ExpensivePaymentGateway;
using Microsoft.Practices.Unity;
using PremiumPaymentGateway;
using PaymentWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PaymentWebApi.Controllers
{
    public class CreditCardController : ApiController
    {
        private readonly IUnityContainer container = null;
        private readonly ICheapPaymentGateway cheapPaymentGateway = null;
        private readonly IExpensivePaymentGateway expensivePaymentGateway = null;
        private readonly IPremiumPaymentGateway premiumPaymentGateway = null;

        public CreditCardController(IUnityContainer container, ICheapPaymentGateway cheapPayment, IExpensivePaymentGateway expensivePayment, IPremiumPaymentGateway premiumPayment)
        {
            this.container = container;
            this.cheapPaymentGateway = cheapPayment;
            this.expensivePaymentGateway = expensivePayment;
            this.premiumPaymentGateway = premiumPayment;
        }
        [HttpPost]
        public HttpResponseMessage ProcessPayment([FromBody] CreditCard card)
        {
            bool paymentProcessedSuccessfully = false;
              
            try
            {
                if (card != null)
                {
                    // check all the values are valid 
                    if ((!string.IsNullOrEmpty(card.CreditCardNo) && card.CreditCardNo.Length == 16 && card.CreditCardNo.All(char.IsDigit)) &&
                        !string.IsNullOrEmpty(card.CardHolder) &&
                        (card.ExpirationDate.HasValue ? card.ExpirationDate.Value >= DateTime.Today : false) &&                        
                        (!string.IsNullOrEmpty(card.SecurityCode) ?
                        card.SecurityCode.Length == 3 && card.SecurityCode.All(char.IsDigit) : true)                         
                        &&
                         (card.Amount.HasValue ? card.Amount.Value >=0 : false))

                    {
                        // if amount less than 20 use ICheapPaymentGateway
                        if (card.Amount < 20)
                        {
                            paymentProcessedSuccessfully = this.cheapPaymentGateway.DoPayment();                         
                        }
                        if(card.Amount > 21 && card.Amount <= 500)
                        {
                            // if amount= £20 - £500 , use IExpensivePaymentGateway
                            paymentProcessedSuccessfully = this.expensivePaymentGateway.DoPayment();
                            // if this fail, try 1 time with  ICheapPaymentGateway
                            if (!paymentProcessedSuccessfully)
                            {
                                paymentProcessedSuccessfully = this.cheapPaymentGateway.DoPayment();
                            }
                        }
                        if (card.Amount > 500)
                        {
                            // if amount > £500 , use IPremiumPaymentGateway
                            int i = 0;
                            while (i < 3)
                            {
                                paymentProcessedSuccessfully =   this.premiumPaymentGateway.DoPayment();
                                //if payment fails try 3 times else exit
                                if (!paymentProcessedSuccessfully)
                                { i++; }
                                else
                                    break;

                            }
                        }    
                        
                        if(paymentProcessedSuccessfully)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, "Payment is processed");
                        }                   
                    }
                    else
                    {
                        // Used BadRequest status code which is 400 (instead of 403 as it was for Foridden)
                        // in Spec it was written to show -  The request is invalid :  403 Bad Request
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "The request is invalid");
                    }
                }

                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error:" + ex.Message);
            }
        }
    }
}
            
    
     