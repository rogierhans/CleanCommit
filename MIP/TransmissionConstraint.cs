using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using CleanCommit.Instance;
namespace CleanCommit.MIP
{
    public class TransmissionConstraint : Constraint
    {
        public TransmissionConstraint(PowerSystem ps, ConstraintConfiguration cc, GRBModel model, Variables solver) : base(ps, cc, model, solver) { }

        public override void AddConstraint()
        {
            AddLineLimits();
            AddDCTransmissionConstraints();
            AddZeroSumInjectionConstraints();
            if (CC.TransmissionMode == ConstraintConfiguration.TransmissionType.Copperplate)
            {
            }
            else if (CC.TransmissionMode == ConstraintConfiguration.TransmissionType.TradeBased)
            {
                AddTransmissionConstraints();
            }
            else if (CC.TransmissionMode == ConstraintConfiguration.TransmissionType.VoltAngles)
            {
                AddTransmissionConstraints();
                AddVoltAngleConstraint();
            }
            else if (CC.TransmissionMode == ConstraintConfiguration.TransmissionType.PTDF)
            {
                AddPDTFConstraints();
            }
            else
            {
                throw new Exception("no mode selected");
            }
        }

        private void AddVoltAngleConstraint()
        {
            for (int t = 0; t < totalTime; t++)
                Model.AddConstr(Variable.NodeVoltAngle[0, t] == 0, "");
            for (int l = 0; l < totalLinesAC; l++)
            {
                for (int t = 0; t < totalTime; t++)
                {
                    TransmissionLineAC line = PS.LinesAC[l];
                    int from = line.From.ID;
                    int to = line.To.ID;
                    Model.AddConstr(Variable.TransmissionFlowAC[l, t] == line.Susceptance * (Variable.NodeVoltAngle[from, t] - Variable.NodeVoltAngle[to, t]), "line voltangle" + l + "t" + t);
                }
            }
        }

        GRBConstr[,] PDTFConstraints;
        private void AddPDTFConstraints()
        {
            PDTFConstraints = new GRBConstr[totalTime, totalLinesAC];
            for (int t = 0; t < totalTime; t++)
            {
                for (int l = 0; l < totalLinesAC; l++)
                {

                    GRBLinExpr trans = new GRBLinExpr();
                    //n=1 instead of n=0 because we skip the reference node and we assume the first node is the reference node
                    for (int n = 1; n < totalNodes; n++)
                    {
                        trans += PDTF[l, n] * Variable.NodalInjectionAC[n, t];
                    }
                    PDTFConstraints[t, l] = Model.AddConstr(Variable.TransmissionFlowAC[l, t] == trans, "PDTF" + t + "l" + l);
                }
            }
        }
        GRBConstr[,] KirchoffLaw;
        private void AddTransmissionConstraints()
        {
            KirchoffLaw = new GRBConstr[totalTime, totalNodes];
            for (int t = 0; t < totalTime; t++)
            {
                for (int n = 0; n < totalNodes; n++)
                {
                    GRBLinExpr flowInMinusFlowOut = new GRBLinExpr();
                    var node = PS.Nodes[n];
                    for (int l = 0; l < totalLinesAC; l++)
                    {
                        var line = PS.LinesAC[l];
                        if (node == line.From)
                        {
                            flowInMinusFlowOut -= Variable.TransmissionFlowAC[l, t];
                        }
                        if (node == line.To)
                        {
                            flowInMinusFlowOut += Variable.TransmissionFlowAC[l, t];
                        }
                    }
                    KirchoffLaw[t, n] = Model.AddConstr(flowInMinusFlowOut == Variable.NodalInjectionAC[n, t], "KirchoffLaw" + n + "t" + t);
                }
            }
        }
        public GRBConstr[,] ACFlowUpperLimits;
        public GRBConstr[,] ACFlowLowerLimits;
        private void AddLineLimits()
        {
            ACFlowUpperLimits = new GRBConstr[totalLinesAC, totalTime];
            ACFlowLowerLimits = new GRBConstr[totalLinesAC, totalTime];
            for (int l = 0; l < totalLinesAC; l++)
            {
                for (int t = 0; t < totalTime; t++)
                {
                    var line = PS.LinesAC[l];
                    ACFlowUpperLimits[l, t] = Model.AddConstr(Variable.TransmissionFlowAC[l, t] <= line.MaxCapacity, "");
                    ACFlowLowerLimits[l, t] = Model.AddConstr(Variable.TransmissionFlowAC[l, t] >= line.MinCapacity, "");
                }
            }

        }


        private void AddDCTransmissionConstraints()
        {
            for (int t = 0; t < totalTime; t++)
            {
                for (int n = 0; n < totalNodes; n++)
                {
                    GRBLinExpr flowInMinusFlowOutDC = new GRBLinExpr();
                    var node = PS.Nodes[n];
                    for (int l = 0; l < totalLinesDC; l++)
                    {
                        var line = PS.LinesDC[l];
                        if (node == line.From)
                        {
                            flowInMinusFlowOutDC -= Variable.TransmissionFlowDC[l, t];
                        }
                        if (node == line.To)
                        {
                            flowInMinusFlowOutDC += Variable.TransmissionFlowDC[l, t];
                        }
                    }
                    Model.AddConstr(flowInMinusFlowOutDC == Variable.NodalInjectionDC[n, t], "DC" + n + "t" + t);
                }
            }
        }

        GRBConstr[] ZeroSumInjection;
        private void AddZeroSumInjectionConstraints()
        {
            ZeroSumInjection = new GRBConstr[totalTime];
            for (int t = 0; t < totalTime; t++)
            {
                GRBLinExpr TotalInjections = new GRBLinExpr();
                for (int n = 0; n < totalNodes; n++)
                {
                    TotalInjections += Variable.NodalInjectionAC[n, t];
                }
                ZeroSumInjection[t] = Model.AddConstr(TotalInjections == 0, "Zero Sum Injection" + t);
            }
        }
    }
}
