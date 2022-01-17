using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using CleanCommit.Instance;
using System.IO;
namespace CleanCommit.MIP
{
    [Serializable]
    public class Solution
    {
        public double ComputationTime;
        public double GurobiCost;
        public double GurobiCostGeneration;
        public double GurobiCostCycle;
        public double GurobiCostLOL;
        public double GurobiCostLOR;
        public double GurobiCostDR;
        public double Gap;
        public int NumConstrs;
        public int NumVars;
        public int NumBinVars;
        public double[] LossOfReserve; // node x time
        public double[,] P; // time x units
        public double[,] Dispatch; // time x units
        public double[,] Commit; // time x units
        public double[,] Start; // time x units
        public double[,] Stop; // time x units
        public double[,] RESDispatch; // time x resunits
        public double[,] TransmissionFlowAC; // lines x time
        public double[,] TransmissionFlowDC; // lines x time
        public double[,] NodeVoltAngle; // node x time
        public double[,] Charge; // time x storageunits
        public double[,] Discharge; // time x storageunits
        public double[,] Storage;  // time x storageunits
                                   //public GRBVar[,] Inflows; // time x inflow

        public double[,] NodalLossOfLoad; // node x time
        public double[,] RESIDUALDemand; // node x time
        public double[,] DemandShed; // node x time;
        public double[,] NodalInjectionAC; // node x time
        public double[,] NodalInjectionDC; // node x time
        public double[,] P2GGeneration;//node x time
        public double[,] NodalShadowPrice; // node x time
        public double[,] LineUpperLimitShadowPrice; // lines x time
        public double[,] LineLowerLimitShadowPrice; // lines x time
        public double[,,] ReserveThermal; // time x units x reservetype;
        public double[,,] ReserveStorage; // time x Sunits x reservetype;

        public double[,,] Piecewise; // time x units x segments
        public PiecewiseGeneration[] PiecewiseGeneration;
        public double[,,] StartCostIntervall;

        public int DRCounter = 0;
        public int LOLCounter = 0;
        public PowerSystem PS;
        public ConstraintConfiguration CC;



        static public Solution GetFromBin(string filename)
        {
            return BinarySerialization.ReadFromBinaryFile<Solution>(filename);
        }

        public void ToBin(string filename)
        {
            BinarySerialization.WriteToBinaryFile<Solution>(filename, this);
        }
        public Solution() { }

        public Solution(GRBModel model, Objective objective, Variables vars, PowerSystem ps, ConstraintConfiguration cc, TransmissionConstraint TC, PowerBalanceContraint PBC)
        {
            PiecewiseGeneration = vars.PiecewiseGeneration;
            PS = ps;
            CC = cc;
            GurobiCost = objective.CurrentObjective.Value;
            GurobiCostGeneration = objective.GenerationCost.X;
            GurobiCostCycle = objective.CycleCost.X;
            GurobiCostLOL = objective.LOLCost.X;
            GurobiCostLOR = objective.LORCost.X;
            GurobiCostDR = objective.DRCost.X;
            Gap = (CC.Relax ? 0 : model.MIPGap);

            ComputationTime = model.Runtime;
            NumConstrs = model.NumConstrs;
            NumVars = model.NumVars;
            NumBinVars = model.NumBinVars;

            P = Get(vars.P);
            ReserveThermal = Get(vars.ReserveThermal);
            ReserveStorage = Get(vars.ReserveStorage);
            Piecewise = Get(vars.Piecewise);
            Commit = Get(vars.Commit);
            Start = Get(vars.Start);
            Stop = Get(vars.Stop);
            RESDispatch = Get(vars.RESDispatch);
            TransmissionFlowAC = Get(vars.TransmissionFlowAC);
            TransmissionFlowDC = Get(vars.TransmissionFlowDC);
            StartCostIntervall = Get(vars.StartCostIntervall);
            NodeVoltAngle = Get(vars.NodeVoltAngle);
            Charge = Get(vars.Charge);
            Discharge = Get(vars.Discharge);
            Storage = Get(vars.Storage);
            LossOfReserve = Get(vars.LossOfReserve);
            NodalLossOfLoad = Get(vars.NodalLossOfLoad);
            RESIDUALDemand = Get(vars.RESIDUALDemand);
            DemandShed = Get(vars.DemandShed);
            NodalInjectionAC = Get(vars.NodalInjectionAC);
            NodalInjectionDC = Get(vars.NodalInjectionDC);
            P2GGeneration = Get(vars.P2GGeneration);
            if (CC.Relax)
            {
                NodalShadowPrice = Get(PBC.NodalPowerBalance,model);
                LineLowerLimitShadowPrice = Get(TC.ACFlowLowerLimits, model);
                LineUpperLimitShadowPrice = Get(TC.ACFlowUpperLimits, model);
            }

            CalculateDispatch();
            CalculateLOL();
        }



        private void CalculateLOL()
        {
            for (int t = 0; t < NodalLossOfLoad.GetLength(1); t++)
            {
                bool LossOfLoad = false;
                bool DR = false;
                for (int n = 0; n < NodalLossOfLoad.GetLength(0); n++)
                {
                    LossOfLoad |= NodalLossOfLoad[n, t] > 0.01;
                    DR |= DemandShed[n, t] > 0.01;
                }
                if (LossOfLoad) LOLCounter++;
                if (DR) DRCounter++;
            }
        }

        private void CalculateDispatch()
        {
            Dispatch = new double[P.GetLength(0), P.GetLength(1)];
            for (int t = 0; t < P.GetLength(0); t++)
            {
                for (int g = 0; g < P.GetLength(1); g++)
                {
                    var unit = PS.Units[g];
                    Dispatch[t, g] = P[t, g] + unit.pMin * Commit[t, g];
                }
            }
        }

        public void ToCSV(string filename)
        {
            var lines = new List<string>();
            lines.AddRange(MArrayToString("ComputationTime", ComputationTime));
            lines.AddRange(MArrayToString("GurobiCost", GurobiCost));
            lines.AddRange(MArrayToString("GurobiCostGeneration", GurobiCostGeneration));
            lines.AddRange(MArrayToString("GurobiCostCycle", GurobiCostCycle));
            lines.AddRange(MArrayToString("GurobiCostLOL", GurobiCostLOL));
            lines.AddRange(MArrayToString("GurobiCostLOR", GurobiCostLOR));
            lines.AddRange(MArrayToString("GurobiCostDR", GurobiCostDR));
            lines.AddRange(MArrayToString("Gap", Gap));
            lines.AddRange(MArrayToString("LOLCounter", LOLCounter));
            lines.AddRange(MArrayToString("DRCounter", DRCounter));
            lines.AddRange(MArrayToString("NumConstrs", NumConstrs));
            lines.AddRange(MArrayToString("NumVars", NumVars));
            lines.AddRange(MArrayToString("NumBinVars", NumBinVars));
            lines.AddRange(MArrayToString("LossOfReserve", LossOfReserve));
            lines.AddRange(MArrayToString("P", P));
            lines.AddRange(MArrayToString("Dispatch", Dispatch));
            lines.AddRange(MArrayToString("Commit", Commit));
            lines.AddRange(MArrayToString("Start", Start));
            lines.AddRange(MArrayToString("Stop", Stop));
            lines.AddRange(MArrayToString("RESDispatch", RESDispatch));
            lines.AddRange(MArrayToString("TransmissionFlowAC", TransmissionFlowAC));
            lines.AddRange(MArrayToString("TransmissionFlowDC", TransmissionFlowDC));
            lines.AddRange(MArrayToString("NodeVoltAngle", NodeVoltAngle));
            lines.AddRange(MArrayToString("Charge", Charge));
            lines.AddRange(MArrayToString("Discharge", Discharge));
            lines.AddRange(MArrayToString("Storage", Storage));
            lines.AddRange(MArrayToString("NodalLossOfLoad", NodalLossOfLoad));
            lines.AddRange(MArrayToString("RESIDUALDemand", RESIDUALDemand));
            lines.AddRange(MArrayToString("DemandResponse", DemandShed));
            lines.AddRange(MArrayToString("NodalInjectionAC", NodalInjectionAC));
            lines.AddRange(MArrayToString("NodalInjectionDC", NodalInjectionDC));
            lines.AddRange(MArrayToString("P2GGeneration", P2GGeneration));
            if (CC.Relax)
            {
                lines.AddRange(MArrayToString("NodalShadowPrice", NodalShadowPrice));
                lines.AddRange(MArrayToString("LineLowerLimitShadowPrice", LineLowerLimitShadowPrice));
                lines.AddRange(MArrayToString("LineUpperLimitShadowPrice", LineUpperLimitShadowPrice));
            }
            lines.AddRange(MArrayToString("ReserveThermal", ReserveThermal));
            lines.AddRange(MArrayToString("ReserveStorage", ReserveStorage));
            // lines.AddRange(MArrayToString("Piecewise", Piecewise));
            File.WriteAllLines(filename, lines);
        }

        public List<string> MArrayToString(string identifier, double input)
        {
            var line = identifier + "=" + input;
            return new List<string>() { line };
        }

        public List<string> MArrayToString(string identifier, double[] input)
        {
            List<string> lines = new List<string>
            {
                "<" + identifier + ">"
            };
            if (input.GetLength(0) != 0)
            {
                List<object> values = new List<object>();
                for (int i = 0; i < input.GetLength(0); i++)
                {

                    values.Add(input[i]);

                }
                lines.Add(String.Join(";", values.Select(x => x.ToString())));
            }
            lines.Add("</" + identifier + ">");
            return lines;
        }
        public List<string> MArrayToString(string identifier, double[,] input)
        {
            List<string> lines = new List<string>
            {
                "<" + identifier + ">"
            };
            if (input.GetLength(0) != 0 && input.GetLength(1) != 0)
            {
                for (int i = 0; i < input.GetLength(0); i++)
                {
                    List<object> values = new List<object>();
                    for (int j = 0; j < input.GetLength(1); j++)
                    {
                        values.Add(input[i, j]);
                    }
                    lines.Add(String.Join(";", values.Select(x => x.ToString())));
                }
            }
            lines.Add("</" + identifier + ">");
            return lines;
        }

        public List<string> MArrayToString(string identifier, double[,,] input)
        {
            List<string> lines = new List<string>
            {
                "<" + identifier + ">"
            };
            if (input.GetLength(0) != 0 && input.GetLength(1) != 0 && input.GetLength(2) != 0)
            {
                for (int n = 0; n < input.GetLength(0); n++)
                {
                    double[,] newValues = new double[input.GetLength(1), input.GetLength(2)];
                    for (int i = 0; i < input.GetLength(1); i++)
                    {
                        for (int j = 0; j < input.GetLength(2); j++)
                        {
                            newValues[i, j] = input[n, i, j];
                        }
                    }
                    lines.AddRange(MArrayToString(identifier + n, newValues));
                }
            }
            lines.Add("</" + identifier + ">");
            return lines;
        }

        private double[] Get(GRBVar[] var)
        {
            var results = new double[var.GetLength(0)];
            for (int i = 0; i < var.GetLength(0); i++)
            {
                results[i] = var[i].X;
            }
            return results;
        }

        private double[,] Get(GRBVar[,] var)
        {
            var results = new double[var.GetLength(0), var.GetLength(1)];
            for (int i = 0; i < var.GetLength(0); i++)
            {
                for (int j = 0; j < var.GetLength(1); j++)
                    results[i, j] = var[i, j].X;
            }
            return results;
        }
        private double[,] Get(GRBConstr[,] var, GRBModel model)
        {
            var results = new double[var.GetLength(0), var.GetLength(1)];
            if (CC.Relax && !(model.IsMIP == 1))
                for (int i = 0; i < var.GetLength(0); i++)
                {
                    for (int j = 0; j < var.GetLength(1); j++)
                        results[i, j] = var[i, j].Pi;
                }

            return results;
        }
        private double[,,] Get(GRBVar[,,] var)
        {
            var results = new double[var.GetLength(0), var.GetLength(1), var.GetLength(2)];
            for (int i = 0; i < var.GetLength(0); i++)
            {
                for (int j = 0; j < var.GetLength(1); j++)
                    for (int k = 0; k < var.GetLength(2); k++)
                        results[i, j, k] = var[i, j, k].X;
            }
            return results;
        }
    }
}
