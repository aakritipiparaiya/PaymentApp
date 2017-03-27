using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheapPaymentGateway
{
    public class CheapPaymentGateway : ICheapPaymentGateway
    {
        public bool DoPayment()
        {
            // do processing. return true if successful payment else false
            return true;
        }
    }
}
