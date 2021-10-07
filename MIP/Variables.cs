using CleanCommit.Instance;
using Gurobi;
using System;
using System.Collections.Generic;
using System.Linq;
namespace CleanCommit.MIP
{
    public class Variables
    {


        public GRBVar[,] P; // time x units
        public GRBVar[,,] ReserveThermal; // time x units x reservetype;
        public GRBVar[,,] ReserveStorage; // time x Sunits x reservetype;
        public GRBVar[,,] Piecewise; // time x units x segments

        public GRBVar[,] Commit; // time x units
        public GRBVar[,] Start; // time x units
        public GRBVar[,] Stop; // time x units
        public GRBVar[,] RESDispatch; // time x resunits
        public GRBVar[,] TransmissionFlowAC; // lines x time
        public GRBVar[,] TransmissionFlowDC; // lines x time
        public GRBVar[,] NodeVoltAngle; // node x time
        public GRBVar[,] Charge; // time x storageunits
        public GRBVar[,] Discharge; // time x storageunits
        public GRBVar[,] Storage;  // time x storageunits
                                   //public GRBVar[,] Inflows; // time x inflow
        public GRBVar[] LossOfReserve; // time
        public GRBVar[,] NodalLossOfLoad; // node x time
        public GRBVar[,] NodalInjectionAC; // node x time
        public GRBVar[,] NodalInjectionDC; // node x time
        public GRBVar[,] DemandShed; // node x time
     //   public GRBVar[,] DemandShift; // node x time
        public GRBVar[,] RESIDUALDemand;
        //public GRBVar[,,] NodalRESGeneration;
        public List<GRBVar>[,] StartCostIntervall;
        public PiecewiseGeneration[] PiecewiseGeneration;

        protected int totalNodes;
        protected int totalTime;
        protected int totalUnits;
        protected int totalLinesAC;
        protected int totalLinesDC;
        protected int totalStorageUnits;
        protected int totalReserveTypes;
        protected int totalRES;
        protected int totalPiecewiseSegments;


        //helping
        public Dictionary<Unit, int> UnitID2Index;
        public Dictionary<StorageUnit, int> SUnitID2Index;
        public Dictionary<ResGeneration, int> RUnitID2Index;

        protected GRBModel Model;
        public PowerSystem PS;
        public ConstraintConfiguration CC;


        protected double[,] PDTF;
        readonly char Type;
        public Variables(PowerSystem ps, ConstraintConfiguration cc, GRBModel model)
        {
            Model = model;
            PS = ps;
            CC = cc;

            totalTime = CC.TotalTime;
            totalUnits = PS.Units.Count;
            totalNodes = PS.Nodes.Count;
            totalLinesAC = PS.LinesAC.Count;
            totalLinesDC = PS.LinesDC.Count;
            totalStorageUnits = PS.StorageUnits.Count;
            totalReserveTypes = CC.Reserves.Count;
            totalRES = PS.ResGenerations.Count;
            totalPiecewiseSegments = CC.PiecewiseSegments;
            PDTF = PS.PDTF;
            Type = CC.Relax ? GRB.CONTINUOUS : GRB.BINARY;
            CreateHelperDictionary();
        }

        private void CreateHelperDictionary()
        {
            UnitID2Index = new Dictionary<Unit, int>();
            for (int u = 0; u < totalUnits; u++)
            {
                var unit = PS.Units[u];
                UnitID2Index[unit] = u;
            }
            SUnitID2Index = new Dictionary<StorageUnit, int>();
            for (int s = 0; s < totalStorageUnits; s++)
            {
                var storageUnit = PS.StorageUnits[s];
                SUnitID2Index[storageUnit] = s;
            }
            RUnitID2Index = new Dictionary<ResGeneration, int>();
            for (int r = 0; r < totalRES; r++)
            {
                var RESunit = PS.ResGenerations[r];
                RUnitID2Index[RESunit] = r;
            }
        }

        public void IntialiseVariables()
        {
            Console.WriteLine("AddDispatchVariables");
            AddDispatchVariables();
            AddReserveVariables();
            Console.WriteLine("AddPieceWiseVariables");
            AddPieceWiseVariables();
            Console.WriteLine("AddRESDispatch");
            AddRESDispatch();
            Console.WriteLine("AddBinaryVariables");
            AddBinaryVariables();
            Console.WriteLine("AddStorageVariables");
            AddStorageVariables();
            Console.WriteLine("AddTransmissionVariables");
            AddTransmissionVariables();
            Console.WriteLine("AddNodalVariables");
            AddNodalVariables();
            if (CC.TimeDependantStartUpCost)
            {
                AddTimeDependantStartUpCostVariables();
            }
        }


        private void AddDispatchVariables()
        {
            P = new GRBVar[totalTime, PS.Units.Count];
            ApplyFunction((t, u) =>
            {
                P[t, u] = Model.AddVar(0, double.MaxValue, 0.0, GRB.CONTINUOUS, "P_" + u + "_" + t);
            });
        }

        private void AddReserveVariables()
        {
            ReserveThermal = new GRBVar[totalTime, PS.Units.Count, totalReserveTypes];
            ApplyFunction((t, u) =>
            {
                for (int reserve = 0; reserve < totalReserveTypes; reserve++)
                {
                    ReserveThermal[t, u, reserve] = Model.AddVar(0, double.MaxValue, 0.0, GRB.CONTINUOUS, "Reserve_" + u + "_" + t + "_" + reserve);
                }
            });
        }

        private void AddPieceWiseVariables()
        {
            PiecewiseGeneration = new PiecewiseGeneration[totalUnits];
            for (int u = 0; u < totalUnits; u++)
            {
                var unit = PS.Units[u];
                PiecewiseGeneration[u] = new PiecewiseGeneration(unit, CC.PiecewiseSegments);
            }
            Piecewise = new GRBVar[totalTime, PS.Units.Count, totalPiecewiseSegments];
            ApplyFunction((t, u) =>
            {
                for (int s = 0; s < totalPiecewiseSegments; s++)
                {
                    Piecewise[t, u, s] = Model.AddVar(0, double.MaxValue, 0.0, GRB.CONTINUOUS, "PWS_" + u + "_" + t);
                }
            });
        }
        private void AddRESDispatch()
        {

            RESDispatch = new GRBVar[totalTime, totalRES];
            for (int t = 0; t < totalTime; t++)
            {
                for (int r = 0; r < totalRES; r++)
                {
                    var RES = PS.ResGenerations[r];
                    var maxRES = RES.GetValue(t,CC.TimeOffSet);
                    RESDispatch[t, r] = Model.AddVar(0, maxRES, 0.0, GRB.CONTINUOUS, "RES_" + t + "_" + r);
                }
            }
        }

        public void AddBinaryVariables()
        {
            Commit = new GRBVar[totalTime, PS.Units.Count];
            Start = new GRBVar[totalTime, PS.Units.Count];
            Stop = new GRBVar[totalTime, PS.Units.Count];
            ApplyFunction((t, u) =>
            {
                Unit unit = PS.Units[u];
                Commit[t, u] = Model.AddVar(0.0, 1, 0.0, Type, "U" + u + "," + t);
                Start[t, u] = Model.AddVar(0.0, 1, 0.0, Type, "V" + u + "," + t);
                Stop[t, u] = Model.AddVar(0.0, 1, 0.0, Type, "W" + u + "," + t);
            });

        }

        public void AddTimeDependantStartUpCostVariables()
        {
            StartCostIntervall = new List<GRBVar>[totalTime, totalUnits];
            ApplyFunction((t, u) =>
            {
                Unit unit = PS.Units[u];
                StartCostIntervall[t, u] = new List<GRBVar>();
                for (int e = 0; e < unit.StartInterval.Length; e++)
                {
                    StartCostIntervall[t, u].Add(Model.AddVar(0.0, 1.0, 0.0, Type, "SCI_" + u + "_" + t + "_" + e));
                }
            });
        }
        private void AddNodalVariables()
        {
            NodalLossOfLoad = new GRBVar[totalNodes, totalTime];
            DemandShed = new GRBVar[totalNodes, totalTime];
            LossOfReserve = new GRBVar[totalTime];
            NodalInjectionAC = new GRBVar[totalNodes, totalTime];
            NodalInjectionDC = new GRBVar[totalNodes, totalTime];
            RESIDUALDemand = new GRBVar[totalNodes, totalTime];
            for (int t = 0; t < totalTime; t++)
            {
                LossOfReserve[t] = Model.AddVar(0, double.MaxValue, 0.0, GRB.CONTINUOUS, "NodalLoR_" + t);
                for (int n = 0; n < totalNodes; n++)
                {
                    var node = PS.Nodes[n];
                    RESIDUALDemand[n, t] = Model.AddVar(0, node.NodalDemand(t, CC.TimeOffSet), 0.0, GRB.CONTINUOUS, "ResidualDemand" + n + "_" + t);
                    NodalLossOfLoad[n, t] = Model.AddVar(0, node.NodalDemand(t,CC.TimeOffSet), 0.0, GRB.CONTINUOUS, "NodalLoL_" + n + "_" + t);
                    DemandShed[n, t] = Model.AddVar(0, node.DemandResonsePotential, 0.0, GRB.CONTINUOUS, "DemandResponse" + n + "_" + t);
                    NodalInjectionAC[n, t] = Model.AddVar(double.MinValue, double.MaxValue, 0.0, GRB.CONTINUOUS, "NodalInjectionAC_" + t);
                    NodalInjectionDC[n, t] = Model.AddVar(double.MinValue, double.MaxValue, 0.0, GRB.CONTINUOUS, "NodalInjectionDC_" + t);

                    Model.AddConstr(RESIDUALDemand[n, t] == (node.NodalDemand(t, CC.TimeOffSet) - DemandShed[n, t] - NodalLossOfLoad[n, t]), "ResidualLoad_"+ n +"_" + t);
                }
            }
        }
        private void AddTransmissionVariables()
        {
            TransmissionFlowAC = new GRBVar[totalLinesAC, totalTime];
            TransmissionFlowDC = new GRBVar[totalLinesDC, totalTime];
            NodeVoltAngle = new GRBVar[totalNodes, totalTime];
            for (int l = 0; l < totalLinesAC; l++)
            {
                var line = PS.LinesAC[l];
                for (int t = 0; t < totalTime; t++)
                {
                    TransmissionFlowAC[l, t] = Model.AddVar(double.MinValue, double.MaxValue, 0.0, GRB.CONTINUOUS, "TransAC_" + l + "_" + t);
                }
            }
            for (int l = 0; l < totalLinesDC; l++)
            {
                var line = PS.LinesDC[l];
                for (int t = 0; t < totalTime; t++)
                {
                    TransmissionFlowDC[l, t] = Model.AddVar(line.MinCapacity, line.MaxCapacity, 0.0, GRB.CONTINUOUS, "TransDC_" + l + "_" + t);
                }
            }
            for (int n = 0; n < totalNodes; n++)
            {
                for (int t = 0; t < totalTime; t++)
                {
                    NodeVoltAngle[n, t] = Model.AddVar(double.MinValue, double.MaxValue, 0.0, GRB.CONTINUOUS, "NodeVoltAngle_" + n + "_" + t);
                }
            }
        }
        private void AddStorageVariables()
        {
            Storage = new GRBVar[totalTime, totalStorageUnits];
            Charge = new GRBVar[totalTime, totalStorageUnits];
            Discharge = new GRBVar[totalTime, totalStorageUnits];
            ReserveStorage = new GRBVar[totalTime, totalStorageUnits, totalReserveTypes];
            for (int t = 0; t < totalTime; t++)
            {
                for (int s = 0; s < totalStorageUnits; s++)
                {
                    var StorageUnit = PS.StorageUnits[s];
                    Storage[t, s] = Model.AddVar(0, StorageUnit.MaxEnergy, 0.0, GRB.CONTINUOUS, "Storage_" + t + "_" + s);
                    Charge[t, s] = Model.AddVar(0, StorageUnit.MaxCharge, 0.0, GRB.CONTINUOUS, "Charge_" + t + "_" + s);
                    Discharge[t, s] = Model.AddVar(0, StorageUnit.MaxDischarge, 0.0, GRB.CONTINUOUS, "Discharge_" + t + "_" + s);
                    for (int reserve = 0; reserve < totalReserveTypes; reserve++)
                    {
                        ReserveStorage[t, s, reserve] = Model.AddVar(0, StorageUnit.MaxDischarge, 0.0, GRB.CONTINUOUS, "Discharge_" + t + "_" + s);
                    }
                }
            }

        }
        public void ApplyFunction(Action<int, int> action)
        {
            for (int t = 0; t < totalTime; t++)
            {
                for (int u = 0; u < totalUnits; u++)
                {
                    action(t, u);
                }
            }
        }
        public GRBLinExpr SummationTotal(int total, Func<int, GRBLinExpr> func)
        {
            //Console.WriteLine(total + " "+ func.ToString());
            if (total == 0) return func(0);
            else return U.GetNumbers(total).Select(i => func(i)).Aggregate((a, b) => a + b);
        }
    }
}
