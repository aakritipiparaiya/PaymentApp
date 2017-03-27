using CheapPaymentGateway;
using ExpensivePaymentGateway;
using PremiumPaymentGateway;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace PaymentWebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            var container = new UnityContainer();
            container.RegisterType<ICheapPaymentGateway, CheapPaymentGateway.CheapPaymentGateway>(new HierarchicalLifetimeManager());
            container.RegisterType<IExpensivePaymentGateway, ExpensivePaymentGateway.ExpensivePaymentGateway>(new HierarchicalLifetimeManager());
            container.RegisterType<IPremiumPaymentGateway, PremiumPaymentGateway.PremiumPaymentGateway>(new HierarchicalLifetimeManager());
            config.DependencyResolver = new UnityResolver(container);
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
