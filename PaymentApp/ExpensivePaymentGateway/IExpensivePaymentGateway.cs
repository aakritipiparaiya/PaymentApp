using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpensivePaymentGateway
{
    public interface IExpensivePaymentGateway
    {
        bool DoPayment();
    }
}
