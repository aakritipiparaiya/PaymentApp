using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheapPaymentGateway
{
    public interface ICheapPaymentGateway
    {
        bool DoPayment();
    }
}
