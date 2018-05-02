using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeSoftPrinterApp.Common
{
    public class UserError: Exception
    {
        public UserError()
            :base()
        {
            
        }
        public UserError(string Message)
            : base(Message)
        {

        }
    }
}