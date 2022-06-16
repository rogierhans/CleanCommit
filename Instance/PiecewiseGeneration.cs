using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanCommit.Instance;
namespace CleanCommit
{
    [Serializable]
    public class PiecewiseGeneration
    {
        //piecewise function approximating the quadratic function
        //it is assumed the PieceWiseCost are monotonic increasing

        public int Pieces;
        public double[] PiecewiseSlope;
        public double[] PiecewiseLengths;
        public double[] PiecewiseStartUpLimit;
        public double[] PiecewiseShutDownLimit;
        public Unit Unit;
        public PiecewiseGeneration() { }
        public PiecewiseGeneration(Unit unit, int pieces)
        {
            Unit = unit;
            Pieces = pieces;
            SetSegmentLength(pieces);
            SetSegmentSlope(pieces);
            SetSegmentLimits(pieces);
        }
        public double DeterminePiecewiseCost(double p)
        {
            if (p < Unit.PMin) { return GetCost(p); }
            double totalP = p - Unit.PMin;
            double totalCost = GetCost(Unit.PMin);
            for (int s = 0; s < PiecewiseLengths.Length; s++)
            {
                double length = Math.Min(totalP, PiecewiseLengths[s]);
                totalCost += PiecewiseSlope[s] * length;
                totalP -= length;
            }
            return totalCost;
        }

        //Creates uniform segmentslength from minimal generation to maximal generation
        private void SetSegmentLength(int segments)
        {
            PiecewiseLengths = new double[segments];
            double segementLength = (Unit.PMax - Unit.PMin) / segments;
            for (int s = 0; s < segments; s++)
            {
                PiecewiseLengths[s] = segementLength;
            }

        }
        //Calculates and set the slope for each piecewise segment
        private void SetSegmentSlope(int segments)
        {
            PiecewiseSlope = new double[segments];
            double cumulativeLength = Unit.PMin;
            for (int s = 0; s < segments; s++)
            {
                var startpoint = cumulativeLength;
                var endpoint = cumulativeLength + PiecewiseLengths[s];
                PiecewiseSlope[s] = GetSlopeBetweenPoints(startpoint, endpoint);
                cumulativeLength += PiecewiseLengths[s];
            }
            MonotonicityCheck(PiecewiseSlope);
        }

        //Precalculates the segmentLimits for the tightformulation;
        private void SetSegmentLimits(int segments)
        {
            PiecewiseStartUpLimit = new double[segments];
            PiecewiseShutDownLimit = new double[segments];
            double cumulativeLength = Unit.PMin;
            for (int s = 0; s < segments; s++)
            {
                PiecewiseStartUpLimit[s] = GetCul(cumulativeLength, cumulativeLength + PiecewiseLengths[s], Unit.SU);
                PiecewiseShutDownLimit[s] = GetCul(cumulativeLength, cumulativeLength + PiecewiseLengths[s], Unit.SD);
                cumulativeLength += PiecewiseLengths[s];
            }
        }

        //Checks if the new piece-wise formulation is monotonic increasing
        private void MonotonicityCheck(double[] piecewiseCost)
        {
            for (int s = 0; s < piecewiseCost.Length - 1; s++)
            {
                if (piecewiseCost[s] > piecewiseCost[s + 1] + 0.000001)
                {
                    Console.WriteLine("{0} - {1} =  {2}", piecewiseCost[s], piecewiseCost[s + 1], piecewiseCost[s + 1] - piecewiseCost[s]);
                    throw new Exception("Error quadractic function not convex and/or piecewisefunction not monotonic");
                }
            }
        }

        private double GetSlopeBetweenPoints(double startP, double endP)
        {
            double startCost = GetCost(startP);
            double endCost = GetCost(endP);

            return (endP - startP ==0)?0:(endCost - startCost) / (endP - startP);
        }


        public double GetCost(double p)
        {
            //Console.WriteLine("{0} {1} {2}", Unit.B, Unit.C, p);
            return Unit.B * p + Unit.C * p * p;
        }


        //returns the limits for piece-wise linear segments
        // See "On Mixed Integer Programming Formulations for the Unit Commitment Problem" from Bernard Knueven et al.
        public double GetCul(double PiecewiseCumalativeMaxPrev, double PiecewiseCumalativeMax, double limit)
        {

            if (PiecewiseCumalativeMax <= limit)
            {
                return 0;
            }
            else if (PiecewiseCumalativeMaxPrev < limit && limit < PiecewiseCumalativeMax)
            {
                return PiecewiseCumalativeMax - limit;
            }
            else if (PiecewiseCumalativeMaxPrev >= limit)
            {
                return PiecewiseCumalativeMax - PiecewiseCumalativeMaxPrev;
            }
            throw new Exception("Piecewisecase error");
        }
    }
}
