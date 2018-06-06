using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANFIS
{
    class RuleSet
    {
        double[] _a1, _b1;          
        double[] _a2, _b2;          
        double[] _a3, _b3;
        double[] _a4, _b4;
        double[] _a5, _b5;
        double[] _w0, _w1, _w2, _w3, _w4, _w5;        
        private int _m;

        public RuleSet(int numOfRules)
        {
            _m = numOfRules;
            _a1 = new double[numOfRules];
            _b1 = new double[numOfRules];
            _a2 = new double[numOfRules];
            _b2 = new double[numOfRules];
            _a3 = new double[numOfRules];
            _b3 = new double[numOfRules];
            _a4 = new double[numOfRules];
            _b4 = new double[numOfRules];
            _a5 = new double[numOfRules];
            _b5 = new double[numOfRules];
            _w0 = new double[numOfRules];
            _w1 = new double[numOfRules];
            _w2 = new double[numOfRules];
            _w3 = new double[numOfRules];
            _w4 = new double[numOfRules];
            _w5 = new double[numOfRules];
        }

        public int NumOfRules
        {
            get
            {
                return _m;
            }
        }

        public double Antecedent1(int ruleIndex, double x)
        {
            return Sigmoid(_a1[ruleIndex], _b1[ruleIndex], x);
        }

        public double Antecedent2(int ruleIndex, double x)
        {
            return Sigmoid(_a2[ruleIndex], _b2[ruleIndex], x);
        }

        public double Antecedent3(int ruleIndex, double x)
        {
            return Sigmoid(_a3[ruleIndex], _b3[ruleIndex], x);
        }

        public double Antecedent4(int ruleIndex, double x)
        {
            return Sigmoid(_a4[ruleIndex], _b4[ruleIndex], x);
        }

        public double Antecedent5(int ruleIndex, double x)
        {
            return Sigmoid(_a5[ruleIndex], _b5[ruleIndex], x);
        }

        private double Sigmoid(double a, double b, double var)
        {
            return 1 / (1 + (double)Math.Exp(b * (var - a)));
        }

        public double Alpha(int ruleIndex, double[] x)
        {
            return Antecedent1(ruleIndex, x[0]) * Antecedent2(ruleIndex, x[1]) * Antecedent3(ruleIndex, x[2]) * Antecedent4(ruleIndex, x[3]) * Antecedent5(ruleIndex, x[4]);
        }

        public double Konsekvens(int ruleIndex, double[] x)
        {
            return _w0[ruleIndex] * x[0] + _w1[ruleIndex] * x[1] + _w2[ruleIndex] * x[2] + _w3[ruleIndex] * x[3] + _w4[ruleIndex] * x[4] + _w5[ruleIndex];
        }

        internal void InitializeParams()            //TODO CHECK trebaju li svi biti između 0 i 1?
        {
            Random rand = new Random();
            RandomNum ran = new RandomNum();
            for (int i = 0; i < _m; i++)
            {
                _a1[i] = ran.GetDouble(1, -1);
                _b1[i] = ran.GetDouble(1, -1);
                _a2[i] = ran.GetDouble(1, -1);
                _b2[i] = ran.GetDouble(1, -1);
                _a3[i] = ran.GetDouble(1, -1);
                _b3[i] = ran.GetDouble(1, -1);
                _a4[i] = ran.GetDouble(1, -1);
                _b4[i] = ran.GetDouble(1, -1);
                _a5[i] = ran.GetDouble(1, -1);
                _b5[i] = ran.GetDouble(1, -1);
                _w0[i] = ran.GetDouble(1, -1);
                _w1[i] = ran.GetDouble(1, -1);
                _w2[i] = ran.GetDouble(1, -1);
                _w3[i] = ran.GetDouble(1, -1);
                _w4[i] = ran.GetDouble(1, -1);
                _w5[i] = ran.GetDouble(1, -1);
            }
        }

        public double GetA1(int ruleIndex)
        {
            return _a1[ruleIndex];
        }
        public void SetA1(int ruleIndex, double value)
        {
            _a1[ruleIndex] = value;
        }
        public double GetA2(int ruleIndex)
        {
            return _a2[ruleIndex];
        }
        public void SetA2(int ruleIndex, double value)
        {
            _a2[ruleIndex] = value;
        }
        public double GetA3(int ruleIndex)
        {
            return _a3[ruleIndex];
        }
        public void SetA3(int ruleIndex, double value)
        {
            _a3[ruleIndex] = value;
        }
        public double GetA4(int ruleIndex)
        {
            return _a4[ruleIndex];
        }
        public void SetA4(int ruleIndex, double value)
        {
            _a4[ruleIndex] = value;
        }
        public double GetA5(int ruleIndex)
        {
            return _a5[ruleIndex];
        }
        public void SetA5(int ruleIndex, double value)
        {
            _a5[ruleIndex] = value;
        }
        public double GetB1(int ruleIndex)
        {
            return _b1[ruleIndex];
        }
        public void SetB1(int ruleIndex, double value)
        {
            _b1[ruleIndex] = value;
        }
        public double GetB2(int ruleIndex)
        {
            return _b2[ruleIndex];
        }
        public void SetB2(int ruleIndex, double value)
        {
            _b2[ruleIndex] = value;
        }
        public double GetB3(int ruleIndex)
        {
            return _b3[ruleIndex];
        }
        public void SetB3(int ruleIndex, double value)
        {
            _b3[ruleIndex] = value;
        }
        public double GetB4(int ruleIndex)
        {
            return _b4[ruleIndex];
        }
        public void SetB4(int ruleIndex, double value)
        {
            _b4[ruleIndex] = value;
        }
        public double GetB5(int ruleIndex)
        {
            return _b5[ruleIndex];
        }
        public void SetB5(int ruleIndex, double value)
        {
            _b5[ruleIndex] = value;
        }

        public double GetW0(int ruleIndex)
        {
            return _w0[ruleIndex];
        }
        public void SetW0(int ruleIndex, double value)
        {
            _w0[ruleIndex] = value;
        }
        public double GetW1(int ruleIndex)
        {
            return _w1[ruleIndex];
        }
        public void SetW1(int ruleIndex, double value)
        {
            _w1[ruleIndex] = value;
        }
        public double GetW2(int ruleIndex)
        {
            return _w2[ruleIndex];
        }
        public void SetW2(int ruleIndex, double value)
        {
            _w2[ruleIndex] = value;
        }
        public double GetW3(int ruleIndex)
        {
            return _w3[ruleIndex];
        }
        public void SetW3(int ruleIndex, double value)
        {
            _w3[ruleIndex] = value;
        }
        public double GetW4(int ruleIndex)
        {
            return _w4[ruleIndex];
        }
        public void SetW4(int ruleIndex, double value)
        {
            _w4[ruleIndex] = value;
        }
        public double GetW5(int ruleIndex)
        {
            return _w5[ruleIndex];
        }
        public void SetW5(int ruleIndex, double value)
        {
            _w5[ruleIndex] = value;
        }
    }
}
