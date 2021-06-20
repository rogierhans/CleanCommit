using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanCommit.Instance
{
    public class TransmissionLineAC
    {
        public Node From;
        public Node To;

        public double MinCapacity;
        public double MaxCapacity;

        public double Susceptance;

        public TransmissionLineAC(Node from, Node to, double minCapacity, double maxCapacity,double susceptance) {
            From = from;
            To = to;

            MinCapacity = minCapacity;
            MaxCapacity = maxCapacity;
            Susceptance = susceptance;
        }

    }
    public class TransmissionLineDC
    {
        public Node From;
        public Node To;

        public double MinCapacity;
        public double MaxCapacity;

        public TransmissionLineDC(Node from, Node to, double minCapacity, double maxCapacity)
        {
            From = from;
            To = to;

            MinCapacity = minCapacity;
            MaxCapacity = maxCapacity;
        }

    }
}
