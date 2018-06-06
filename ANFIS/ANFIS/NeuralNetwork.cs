using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace ANFIS
{
    class NeuralNetwork
    {
        string draft;
        ProgressBar pr;
        int step;
        const int nperem = 5; //кол-во входных переменных
        double _learningRate;        //скорость обучения
        int _M;                     //кол-во правил
        const int _Dint = 9;        //число дискретных значений в интервале для функций x / y в наборе захваченных примеров 

        const int _Dint1 = 9;
        RuleSet _rules;
        int _N;                     //ко-во обучающих примеров

        double[,] Dx1;
        double[] Dz1;
        private double _learningRateSigm;

        public NeuralNetwork(int brojPravila, double learningRate, string filename, ProgressBar pr1) //для обучения
        {
            pr = pr1;
            _M = brojPravila;
            _learningRate = learningRate;
            _learningRateSigm = learningRate;

            Dx1 = new double[nperem, _Dint1 * _Dint1];
            Dz1 = new double[_Dint1 * _Dint1];

            InitialzieData(filename, ref Dx1, ref Dz1);
            _rules = new RuleSet(brojPravila);
            _rules.InitializeParams();
        }

        public NeuralNetwork(int brojPravila, double learningRate, string filesettings) //для работы
        {
            _M = brojPravila;
            _learningRate = learningRate;
            _learningRateSigm = learningRate;

            Dx1 = new double[nperem, _Dint1 * _Dint1];
            Dz1 = new double[_Dint1 * _Dint1];
            
            _rules = new RuleSet(brojPravila);
            _rules.InitializeParams();
            GetSettingsFromFile(filesettings);
        }

        private void InitialzieData(string filename, ref double[,] dx, ref double[] dz) //инициализирует обучающую выборку
        {
            try
            {
                using (StreamReader sr = File.OpenText(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory.ToString(), filename)))
                {
                    int j = 0;
                    do
                    {
                        string[] redak = sr.ReadLine().Split(' ');
                        redak = redak.Where(x => !string.IsNullOrEmpty(x)).ToArray();

                        dx[0, j] = double.Parse(redak[0], CultureInfo.InvariantCulture); //входные
                        dx[1, j] = double.Parse(redak[1], CultureInfo.InvariantCulture);
                        dx[2, j] = double.Parse(redak[2], CultureInfo.InvariantCulture);
                        dx[3, j] = double.Parse(redak[3], CultureInfo.InvariantCulture);
                        dx[4, j] = double.Parse(redak[4], CultureInfo.InvariantCulture);
                        dz[j] = double.Parse(redak[5], CultureInfo.InvariantCulture); //выходные оценки

                        j++;

                    } while (sr.Peek() != -1);

                    _N = j;
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void EpochTraining1(int numberOfEpochs)
        {
            step = 100 / (numberOfEpochs / 1000);
            for (int i = 0; i < numberOfEpochs; i++)
            {
                TrainWholeSetStochastic1();

                if (numberOfEpochs > 1000) //меняет progress bar 
                    if (i % 1000 == 0 && i > 0)
                        pr.BeginInvoke(new MethodInvoker(delegate { 
                            pr.Value += step;
                        }));
            }
        }

        public void TrainWholeSetStochastic1()
        {
            for (int i = 0; i < _N; i++)
            {
                SingleTrainingIterationStoch1(i);
            }
        }

        private void SingleTrainingIterationStoch1(int exampleIndex)
        {
            double o;
            double brojnik = 0;
            double nazivnik = 0;                    //знаменатель (сумма)
            double brojnik2;                        //для изменения параметров a и b
                                                    
            double[] x = new double[nperem];
            for (int i = 0; i < nperem; i++)
                x[i] = Dx1[i, exampleIndex];

            for (int i = 0; i < _M; i++)
            {
                double alfa = _rules.Alpha(i, x);
                nazivnik += alfa;
                brojnik += alfa * _rules.Konsekvens(i, x);
            }

            o = brojnik / nazivnik;

            for (int i = 0; i < _M; i++)
            {
                brojnik2 = 0;

                _rules.SetW0(i, _rules.GetW0(i) + _learningRate * (Dz1[exampleIndex] - o) * (_rules.Alpha(i, x) * Dx1[0, exampleIndex]) / nazivnik);
                _rules.SetW1(i, _rules.GetW1(i) + _learningRate * (Dz1[exampleIndex] - o) * (_rules.Alpha(i, x) * Dx1[1, exampleIndex]) / nazivnik);
                _rules.SetW2(i, _rules.GetW2(i) + _learningRate * (Dz1[exampleIndex] - o) * (_rules.Alpha(i, x) * Dx1[2, exampleIndex]) / nazivnik);
                _rules.SetW3(i, _rules.GetW3(i) + _learningRate * (Dz1[exampleIndex] - o) * (_rules.Alpha(i, x) * Dx1[3, exampleIndex]) / nazivnik);
                _rules.SetW4(i, _rules.GetW4(i) + _learningRate * (Dz1[exampleIndex] - o) * (_rules.Alpha(i, x) * Dx1[4, exampleIndex]) / nazivnik);
                _rules.SetW5(i, _rules.GetW5(i) + _learningRate * (Dz1[exampleIndex] - o) * (_rules.Alpha(i, x)) / nazivnik);

                for (int j = 0; j < _M; j++)
                {
                    if (i != j)
                    {
                        brojnik2 += _rules.Alpha(j, x)
                            * (_rules.Konsekvens(i, x) - _rules.Konsekvens(j, x));
                    }
                }

                _rules.SetA1(i, _rules.GetA1(i) + _learningRateSigm * (Dz1[exampleIndex] - o) * (brojnik2) / Math.Pow(nazivnik, 2)
                    * _rules.GetB1(i) * _rules.Alpha(i, x) * (1 - _rules.Antecedent1(i, Dx1[0, exampleIndex])));
                _rules.SetA2(i, _rules.GetA2(i) + _learningRateSigm * (Dz1[exampleIndex] - o) * (brojnik2) / Math.Pow(nazivnik, 2)
                    * _rules.GetB2(i) * _rules.Alpha(i, x) * (1 - _rules.Antecedent2(i, Dx1[1, exampleIndex])));
                _rules.SetA3(i, _rules.GetA3(i) + _learningRateSigm * (Dz1[exampleIndex] - o) * (brojnik2) / Math.Pow(nazivnik, 2)
                    * _rules.GetB3(i) * _rules.Alpha(i, x) * (1 - _rules.Antecedent3(i, Dx1[2, exampleIndex])));
                _rules.SetA4(i, _rules.GetA4(i) + _learningRateSigm * (Dz1[exampleIndex] - o) * (brojnik2) / Math.Pow(nazivnik, 2)
                    * _rules.GetB4(i) * _rules.Alpha(i, x) * (1 - _rules.Antecedent4(i, Dx1[3, exampleIndex])));
                _rules.SetA5(i, _rules.GetA5(i) + _learningRateSigm * (Dz1[exampleIndex] - o) * (brojnik2) / Math.Pow(nazivnik, 2)
                    * _rules.GetB5(i) * _rules.Alpha(i, x) * (1 - _rules.Antecedent5(i, Dx1[4, exampleIndex])));

                _rules.SetB1(i, _rules.GetB1(i) - _learningRateSigm * (Dz1[exampleIndex] - o) * (brojnik2) / Math.Pow(nazivnik, 2)
                    * (Dx1[0, exampleIndex] - _rules.GetA1(i)) * _rules.Alpha(i, x) * (1 - _rules.Antecedent1(i, Dx1[0, exampleIndex])));
                _rules.SetB2(i, _rules.GetB2(i) - _learningRateSigm * (Dz1[exampleIndex] - o) * (brojnik2) / Math.Pow(nazivnik, 2)
                    * (Dx1[1, exampleIndex] - _rules.GetA2(i)) * _rules.Alpha(i, x) * (1 - _rules.Antecedent2(i, Dx1[1, exampleIndex])));
                _rules.SetB3(i, _rules.GetB3(i) - _learningRateSigm * (Dz1[exampleIndex] - o) * (brojnik2) / Math.Pow(nazivnik, 2)
                    * (Dx1[2, exampleIndex] - _rules.GetA3(i)) * _rules.Alpha(i, x) * (1 - _rules.Antecedent3(i, Dx1[2, exampleIndex])));
                _rules.SetB4(i, _rules.GetB4(i) - _learningRateSigm * (Dz1[exampleIndex] - o) * (brojnik2) / Math.Pow(nazivnik, 2)
                    * (Dx1[3, exampleIndex] - _rules.GetA4(i)) * _rules.Alpha(i, x) * (1 - _rules.Antecedent4(i, Dx1[3, exampleIndex])));
                _rules.SetB5(i, _rules.GetB5(i) - _learningRateSigm * (Dz1[exampleIndex] - o) * (brojnik2) / Math.Pow(nazivnik, 2)
                    * (Dx1[4, exampleIndex] - _rules.GetA5(i)) * _rules.Alpha(i, x) * (1 - _rules.Antecedent5(i, Dx1[4, exampleIndex])));
            }
        }

        public double Loss(double expectedOutput, double output)
        {
            double sum = 0;
            for (int i = 0; i < _M; i++)
            {
                sum += (expectedOutput - output) * (expectedOutput - output);
            }
            return sum / 2;
        }

        public double Error()
        {
            double sum = 0;
            for (int i = 0; i < _N; i++)
            {
                sum += Loss(Dz1[i], NetworkOutput1(i));
            }
            return sum / _N;
        }

        public double NetworkOutput1(int exampleIndex)
        {
            double brojnik = 0;
            double nazivnik = 0;
            double[] x = new double[nperem];

            x[0] = Dx1[0, exampleIndex];
            x[1] = Dx1[1, exampleIndex];
            x[2] = Dx1[2, exampleIndex];
            x[3] = Dx1[3, exampleIndex];
            x[4] = Dx1[4, exampleIndex];

            for (int i = 0; i < _M; i++)
            {
                double alfa = _rules.Alpha(i, x);
                nazivnik += alfa;
                brojnik += alfa * _rules.Konsekvens(i, x);
            }

            return brojnik / nazivnik;
        }

        public int NetworkOutput(double x0, double x1, double x2, double x3, double x4) //Исправили
        {
            double brojnik = 0;
            double nazivnik = 0;
            double a;
            int answer;

            double[] x = new double[nperem];
            x[0] = x0;
            x[1] = x1;
            x[2] = x2;
            x[3] = x3;
            x[4] = x4;

            for (int i = 0; i < _M; i++)
            {
                double alfa = _rules.Alpha(i, x);
                nazivnik += alfa;
                brojnik += alfa * _rules.Konsekvens(i, x);
            }

            a = Math.Round(brojnik / nazivnik);
            if (a <= 0) a = 1;
            else if (a >= 5) a = 5;
            answer = Convert.ToInt32(a);
            return answer;
        }

        public void WriteDataToFile(string fileName)
        {
            using (FileStream fs = File.Open(fileName, FileMode.Open))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                for (int i = 0; i < _M; i++)
                {
                    draft = Math.Round(_rules.GetA1(i), 5).ToString(CultureInfo.InvariantCulture)+" "+ Math.Round(_rules.GetA2(i), 5).ToString(CultureInfo.InvariantCulture)+ " " +
                        Math.Round(_rules.GetA3(i), 5).ToString(CultureInfo.InvariantCulture)+ " " +
                        Math.Round(_rules.GetA4(i), 5).ToString(CultureInfo.InvariantCulture)+ " " +
                                                     Math.Round(_rules.GetA5(i), 5).ToString(CultureInfo.InvariantCulture)+ " " +
                                                     Math.Round(_rules.GetB1(i), 5).ToString(CultureInfo.InvariantCulture)+ " " +
                                                     Math.Round(_rules.GetB2(i), 5).ToString(CultureInfo.InvariantCulture)+ " " +
                                                     Math.Round(_rules.GetB3(i), 5).ToString(CultureInfo.InvariantCulture)+ " " +
                                                     Math.Round(_rules.GetB4(i), 5).ToString(CultureInfo.InvariantCulture)+ " " +
                                                     Math.Round(_rules.GetB5(i), 5).ToString(CultureInfo.InvariantCulture);
                    sw.WriteLine(draft);
                }
            }

            using (FileStream fs = File.Open("lin_" + fileName, FileMode.Open))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                for (int i = 0; i < _M; i++)
                {
                    draft = Math.Round(_rules.GetW0(i), 5).ToString(CultureInfo.InvariantCulture) + " " +
                                             Math.Round(_rules.GetW1(i), 5).ToString(CultureInfo.InvariantCulture) + " " +
                                             Math.Round(_rules.GetW2(i), 5).ToString(CultureInfo.InvariantCulture) + " " +
                                             Math.Round(_rules.GetW3(i), 5).ToString(CultureInfo.InvariantCulture) + " " +
                                             Math.Round(_rules.GetW4(i), 5).ToString(CultureInfo.InvariantCulture) + " " +
                                             Math.Round(_rules.GetW5(i), 5).ToString(CultureInfo.InvariantCulture);
                    sw.WriteLine(draft);
                }
            }
        }

        public void GetSettingsFromFile(string fileName)
        {
            using (StreamReader sr = File.OpenText(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory.ToString(), fileName)))
            {
                int j = 0;
                do
                {
                    string[] redak = sr.ReadLine().Split(' ');
                    redak = redak.Where(x => !string.IsNullOrEmpty(x)).ToArray();

                    _rules.SetA1(j, double.Parse(redak[0], CultureInfo.InvariantCulture));
                    _rules.SetA2(j, double.Parse(redak[1], CultureInfo.InvariantCulture));
                    _rules.SetA3(j, double.Parse(redak[2], CultureInfo.InvariantCulture));
                    _rules.SetA4(j, double.Parse(redak[3], CultureInfo.InvariantCulture));
                    _rules.SetA5(j, double.Parse(redak[4], CultureInfo.InvariantCulture));
                    _rules.SetB1(j, double.Parse(redak[5], CultureInfo.InvariantCulture));
                    _rules.SetB2(j, double.Parse(redak[6], CultureInfo.InvariantCulture));
                    _rules.SetB3(j, double.Parse(redak[7], CultureInfo.InvariantCulture));
                    _rules.SetB4(j, double.Parse(redak[8], CultureInfo.InvariantCulture));
                    _rules.SetB5(j, double.Parse(redak[9], CultureInfo.InvariantCulture));

                    j++;

                } while (j!=5);
            }
            using (StreamReader sr = File.OpenText(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory.ToString(), "lin_" + fileName)))
            {
                int j = 0;
                do
                {
                    string[] redak = sr.ReadLine().Split(' ');
                    redak = redak.Where(x => !string.IsNullOrEmpty(x)).ToArray();

                    _rules.SetW0(j, double.Parse(redak[0], CultureInfo.InvariantCulture));
                    _rules.SetW1(j, double.Parse(redak[1], CultureInfo.InvariantCulture));
                    _rules.SetW2(j, double.Parse(redak[2], CultureInfo.InvariantCulture));
                    _rules.SetW3(j, double.Parse(redak[3], CultureInfo.InvariantCulture));
                    _rules.SetW4(j, double.Parse(redak[4], CultureInfo.InvariantCulture));
                    _rules.SetW5(j, double.Parse(redak[5], CultureInfo.InvariantCulture));

                    j++;

                } while (j!=5);
            }
        }
    }
}
