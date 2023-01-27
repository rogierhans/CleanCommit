using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using CleanCommit.Instance;
namespace CleanCommit.MIP
{

    class ConsoleOverwrite : GRBCallback
    {

        public ConsoleOverwrite()
        {
        }
        protected override void Callback()
        {
            if (this.where == GRB.Callback.MESSAGE)
            {
                String text = this.GetStringInfo(GRB.Callback.MSG_STRING);
                Console.Write(text);
            }
        }
    }
}
