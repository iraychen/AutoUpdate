using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdateClient.Common
{
    public class StateCenter
    {
        private StateCenter()
        {

        }

        public static readonly StateCenter instance = new StateCenter();

        public bool HasRepalced = true;

    }
}
