using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpensivePaymentGateway
{
    public class ExpensivePaymentGateway : IExpensivePaymentGateway
    {
        public bool DoPayment()
        {
            // do processing. return true if successful payment else false
            Random no = new Random();
            int randomNo = no.Next(1, 10);
            // this is just to make service works sometimes and fail other times.
            if (randomNo % 2 == 0)
            {
                return false;
            }
            return true;
        }
    }
}
