using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PremiumPaymentGateway
{
    public interface IPremiumPaymentGateway
    {
        bool DoPayment();
    }
}
