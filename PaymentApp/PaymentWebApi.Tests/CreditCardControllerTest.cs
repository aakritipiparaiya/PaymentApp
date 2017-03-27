using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaymentWebApi.Controllers;
using PaymentWebApi.Models;
using Microsoft.Practices.Unity;
using PremiumPaymentGateway;
using CheapPaymentGateway;
using ExpensivePaymentGateway;
using System.ComponentModel;
using Rhino.Mocks;
using System.Net.Http;
using System.Web.Http;
using System.Net;

namespace PaymentWebApi.Tests
{
   

    [TestClass]
    public class CreditCardControllerTest
    {
        private IUnityContainer unityContainer;
        ICheapPaymentGateway cheapPaymentGateway;
        IPremiumPaymentGateway premiumPaymentGateway;
        IExpensivePaymentGateway expensivePaymentGateway;
        CreditCardController controller;

       [TestInitialize()]
        public void TestInitialize()
        {
            unityContainer = new UnityContainer();
            premiumPaymentGateway = MockRepository.GenerateStub<IPremiumPaymentGateway>();
            cheapPaymentGateway =  MockRepository.GenerateStub<ICheapPaymentGateway>();
            expensivePaymentGateway = MockRepository.GenerateStub<IExpensivePaymentGateway>();
            controller = new CreditCardController(unityContainer, cheapPaymentGateway, expensivePaymentGateway, premiumPaymentGateway);
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
        }

        [TestMethod]
        public void CreditCardNumberMandatory_DontSendNumber_Error()
        {
            CreditCard card = new CreditCard();
            card.Amount = 210;
            card.CardHolder = "Aakriti";
            var response = controller.ProcessPayment(card);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public void CreditCardNumberMandatory_invalidCardLengthNot16_Error()
        {
            CreditCard card = new CreditCard();
            card.Amount = 210;
            card.CardHolder = "Aakriti";
            card.CreditCardNo = "2421521532";
            var response = controller.ProcessPayment(card);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public void CreditCardNumberMandatory_invalidCardNotDigits_Error()
        {
            CreditCard card = new CreditCard();
            card.Amount = 210;
            card.CardHolder = "Aakriti";
            card.CreditCardNo = "2421fsd1532";
            var response = controller.ProcessPayment(card);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public void CardHolderMandatory_DontsSendCardHolder_Error()
        {
            CreditCard card = new CreditCard();
            card.Amount = 210;           
            card.CreditCardNo = "2421453325661532";
            var response = controller.ProcessPayment(card);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public void ExpirationMandatory_PastdateExpiration_Error()
        {
            CreditCard card = new CreditCard();
            card.Amount = 210;
            card.CreditCardNo = "2421453325661532";
            card.CardHolder = "Aakriti";
            card.ExpirationDate = new DateTime(2002, 05, 03);
            var response = controller.ProcessPayment(card);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public void ExpirationMandatory_DontsSendExpiration_Error()
        {
            CreditCard card = new CreditCard();
            card.Amount = 210;
            card.CreditCardNo = "2421453325661532";
            card.CardHolder = "Aakriti";
            var response = controller.ProcessPayment(card);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public void SecurityCodeOptional_DontsSendsecurity_NoError()
        {
            CreditCard card = new CreditCard();
            card.Amount = 210;
            card.CreditCardNo = "2421453325661532";
            card.CardHolder = "Aakriti";
            card.ExpirationDate = DateTime.MaxValue;            
            var response = controller.ProcessPayment(card);
            // if security code not passed , request format is still ok
            Assert.AreNotEqual(response.StatusCode, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public void SecurityCode3digitis_SecurityCodeInvalid_Error()
        {
            CreditCard card = new CreditCard();
            card.Amount = 210;
            card.CreditCardNo = "2421453325661532";
            card.CardHolder = "Aakriti";
            card.ExpirationDate = DateTime.MaxValue;
            card.SecurityCode = "2422";
            var response = controller.ProcessPayment(card);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public void AmountLessThan20_UseCheapPaymentGateway()
        {
            cheapPaymentGateway.Stub(r => r.DoPayment()).Return(true);

            CreditCard card = new CreditCard();
            card.Amount = 10;
            card.CreditCardNo = "2421453325661532";
            card.CardHolder = "Aakriti";
            card.ExpirationDate = DateTime.MaxValue;
            card.SecurityCode = "242";
            var response = controller.ProcessPayment(card);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [TestMethod]
        public void AmountBetween20and500_UseExpensivePaymentGateway()
        {
            cheapPaymentGateway.Stub(r => r.DoPayment()).Return(false);
            expensivePaymentGateway.Stub(r => r.DoPayment()).Return(true);

            CreditCard card = new CreditCard();
            card.Amount = 300;
            card.CreditCardNo = "2421453325661532";
            card.CardHolder = "Aakriti";
            card.ExpirationDate = DateTime.MaxValue;
            card.SecurityCode = "242";
            var response = controller.ProcessPayment(card);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [TestMethod]
        public void AmountBetween20and500_UseExpensivePaymentGateway_UseCheapifExpNotAvailable()
        {

            // if expensivePaymentGateway dont work then use cheapPaymentGateway
            cheapPaymentGateway.Stub(r => r.DoPayment()).Return(true);
            expensivePaymentGateway.Stub(r => r.DoPayment()).Return(false);

            CreditCard card = new CreditCard();
            card.Amount = 300;
            card.CreditCardNo = "2421453325661532";
            card.CardHolder = "Aakriti";
            card.ExpirationDate = DateTime.MaxValue;
            card.SecurityCode = "242";
            var response = controller.ProcessPayment(card);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [TestMethod]
        public void AmountMoreThan500_UsePremiumPaymentGateway_Try3TimesifNotAvailable()
        {
            // if PremiumPaymentGateway dont work 3 times than no payment
            cheapPaymentGateway.Stub(r => r.DoPayment()).Return(false);
            expensivePaymentGateway.Stub(r => r.DoPayment()).Return(false);
            premiumPaymentGateway.Stub(r => r.DoPayment()).Return(false);

            CreditCard card = new CreditCard();
            card.Amount = 900;
            card.CreditCardNo = "2421453325661532";
            card.CardHolder = "Aakriti";
            card.ExpirationDate = DateTime.MaxValue;
            card.SecurityCode = "242";
            var response = controller.ProcessPayment(card);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.InternalServerError);
        }

        [TestMethod]
        public void AmountMoreThan500_UsePremiumPaymentGateway_IfSuccessfull()
        {
            cheapPaymentGateway.Stub(r => r.DoPayment()).Return(false);
            expensivePaymentGateway.Stub(r => r.DoPayment()).Return(false);
            premiumPaymentGateway.Stub(r => r.DoPayment()).Return(true);

            CreditCard card = new CreditCard();
            card.Amount = 900;
            card.CreditCardNo = "2421453325661532";
            card.CardHolder = "Aakriti";
            card.ExpirationDate = DateTime.MaxValue;
            card.SecurityCode = "242";
            var response = controller.ProcessPayment(card);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }


    }
}
