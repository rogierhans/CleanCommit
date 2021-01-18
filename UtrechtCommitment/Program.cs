using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtrechtCommitment
{
    class Program
    {
        static void Main(string[] args)
        {
            var ps = new PowerSystem();
            Solution s = new Solution(ps);
            s.Solve();
        }
    }
}
