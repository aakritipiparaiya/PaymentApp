using CheapPaymentGateway;
using ExpensivePaymentGateway;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PremiumPaymentGateway;
using PaymentWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace PaymentWebApi.Tests
{
    [TestClass]
    public class UnityResolverTest
    {
        [TestMethod]
        public void UnityResolver_Resolves_Registered_CheapPaymentGateway_Test()
        {
            var container = new UnityContainer();
            container.RegisterInstance<ICheapPaymentGateway>(new CheapPaymentGateway.CheapPaymentGateway());

            var resolver = new UnityResolver(container);
            var instance = resolver.GetService(typeof(ICheapPaymentGateway));

            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void UnityResolver_Resolves_Registered_ExpensivePaymentGateway_Test()
        {
            var container = new UnityContainer();
            container.RegisterInstance<IExpensivePaymentGateway>(new ExpensivePaymentGateway.ExpensivePaymentGateway());

            var resolver = new UnityResolver(container);
            var instance = resolver.GetService(typeof(IExpensivePaymentGateway));

            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void UnityResolver_Resolves_Registered_PremiumPaymentGateway_Test()
        {
            var container = new UnityContainer();
            container.RegisterInstance<IPremiumPaymentGateway>(new PremiumPaymentGateway.PremiumPaymentGateway());

            var resolver = new UnityResolver(container);
            var instance = resolver.GetService(typeof(IPremiumPaymentGateway));

            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void UnityResolver_DoesNot_Resolve_NonRegistered_InterfaceNotUsed_Test()
        {
            var container = new UnityContainer();

            var resolver = new UnityResolver(container);
            var instance = resolver.GetService(typeof(InterfaceNotUsed));

            Assert.IsNull(instance);
        }

        [TestMethod]
        public void UnityResolver_Resolves_Registered_ContactRepository_Through_ContactsController_Test()
        {
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute("default",
                "api/{controller}/{id}", new { id = RouteParameter.Optional });

            var container = new UnityContainer();
            container.RegisterInstance<ICheapPaymentGateway>(new CheapPaymentGateway.CheapPaymentGateway());

            config.DependencyResolver = new UnityResolver(container);

            var server = new HttpServer(config);
            var client = new HttpClient(server);

            client.GetAsync("http://anything/api/contacts").ContinueWith(task =>
            {
                var response = task.Result;
                Assert.IsNotNull(response.Content);
            });
        }
    }
}
