﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanCommit.Instance
{
    [Serializable]
    public class Unit
    {
        //unique number
        public string ID;

        //name
        public string Name;

        //number of units
        public int Count;

        //generation limits
        public double pMin, pMax;

        //generation cost as a quadratic function f(p) = a + bp + c^2
        public double A, B, C;

        public double CO2Fixed, CO2Variable;
        // public double  linearB;


        //cycle info
        public double RU, RD;
        public double SU, SD;
        public int minUpTime;
        public int minDownTime;

        //discretised timedependent startupcosts
        public double[] StartCostInterval;
        public int[] StartInterval;

        //time dependent as exponentail function
        public double FSC = -1;
        public double VSC;
        public double Lambda;

        public string PrintType;
        public  Unit() { }
        public Unit(string iD, string name, int count, double pMin, double pMax, double a, double b, double c, double rU, double rD, double sU, double sD, int minUpTime, int minDownTime, double[] startCostInterval, int[] startInterval, double fSC, double vSC, double lambda)
        {
            ID = iD;
            Name = name;
            Count = count;
            this.pMin = pMin;
            this.pMax = pMax;
            A = a;
            B = b;
            C = c;
            RU = (int)Math.Min(rU, pMax);
            RD = (int)Math.Min(rD, pMax);
            SU = (int)Math.Min(sU, pMax);
            SD = (int)Math.Min(sD, pMax);
            this.minUpTime = Math.Max(2, minUpTime);
            this.minDownTime = Math.Max(2, minDownTime);
            StartCostInterval = startCostInterval;
            StartInterval = startInterval;
            FSC = fSC;
            VSC = vSC;
            Lambda = lambda;
        }

        public Unit(string id, int count)
        {
            ID = id;
            Count = count;
            Name = "Generator" + ID;
        }

        public void SetGenerationLimits(double pMin, double pMax)
        {

            this.pMin = pMin;
            this.pMax = pMax;
        }

        public void SetGenerationCost(double a, double b, double c)
        {
            A = a;
            B = b;
            C = c;
        }

        public void SetRampLimits(double ru, double rd, double su, double sd)
        {

            RU = (int)Math.Min(ru, pMax);
            RD = (int)Math.Min(rd, pMax);
            SU = (int)Math.Min(su, pMax);
            SD = (int)Math.Min(sd, pMax);
        }

        public void SetMinTime(int minUpTime, int minDownTime)
        {
            if (minUpTime < 1 || minDownTime < 1)
            {
                throw new Exception("error MinimumUP/DOWNTime less than 1 ");
            }
            this.minUpTime = minUpTime;
            this.minDownTime =  minDownTime;
        }

        public void SetSUInterval(double[] startCostInterval, int[] startInterval)
        {
            StartCostInterval = startCostInterval;
            StartInterval = startInterval;
        }

        public void SetSUFunction(double f, double v, double lambda)
        {
            FSC = f;
            VSC = v;
            Lambda = lambda;
            DiscretiseTimeDependantStartupCost();
        }

        private void DiscretiseTimeDependantStartupCost()
        {
            StartInterval = new int[] { 0, 8, 16 };
            StartCostInterval = GetCostInterval(StartInterval);
        }

        private double[] GetCostInterval(int[] interval)
        {
            return interval.Select(t => StartupCost(t)).ToArray();
        }

        private double StartupCost(int timePast)
        {
            return FSC + VSC * (1 - Math.Exp(timePast * -Lambda));
        }

        public double DetermineStartupCost(int timePast)
        {
            for (int i = 0; i < StartInterval.Length - 1; i++)
            {
                if (StartInterval[i] <= timePast && timePast <= StartInterval[i + 1])
                    return StartCostInterval[i];
            }
            return StartCostInterval.Last();
        }

        public void GetInfo()
        {
            Console.WriteLine(Name);
            Console.WriteLine("GEN:{0} - {1}", pMin, pMax);
            Console.WriteLine("COS:{0}+{1}p+{2}P^2", A, B, C);
            Console.WriteLine("RAM:{0} - {1}", RU, RD);
            Console.WriteLine("STA:{0} - {1}", SU, SD);
            Console.WriteLine("MIN:{0} - {1}", minUpTime, minDownTime);
            Console.WriteLine("EXP:{0} + {1} * (1 - e^-{2}l)", FSC, VSC, Lambda);
            Console.WriteLine("INT:{0}", "[" + String.Join(":", StartInterval) + "]");
            Console.WriteLine("COS:{0}", "[" + String.Join(":", StartCostInterval) + "]");
        }

        public string ToRow()
        {
            List<string> Properties = new List<string>
            {
                ID.ToString(),
                pMin.ToString(),
                pMax.ToString(),
                Math.Round(A, 5).ToString(),
                Math.Round(B, 5).ToString(),
                Math.Round(C, 5).ToString(),
                Math.Round(RU).ToString(),
                Math.Round(RD).ToString(),
                Math.Round(SU).ToString(),
                Math.Round(SD).ToString(),
                Math.Max(minUpTime, 1).ToString(),
                Math.Max(minDownTime, 1).ToString()
            };
            if (FSC != -1)
            {
                Properties.Add(FSC.ToString());
                Properties.Add(VSC.ToString());
                Properties.Add(Math.Round(Lambda, 3).ToString());
            }
            else
            {
                Properties.Add(String.Join(":", StartInterval));
                Properties.Add(String.Join(":", StartCostInterval));
            }
            return String.Join("&", Properties) + "\\\\";
        }
    }
}
