using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ABI.Robotics.Model
{
    class Factory
    {
        //public String _name;
        //public String _address;

    }

    class OperationCell
    {

    }

    class Operator
    {

    }

    class Board : PhysicalObject
    {
        private int _numOfItem;

        public int NumOfItem
        {
            get { return _numOfItem; }
            set
            {
                if (value < 0)
                {
                    _numOfItem = 0;
                }
                else if (value > int.MaxValue)
                {
                    _numOfItem = int.MaxValue;
                }
                else
                {
                    _numOfItem = value;
                }

            }
        }

        public List<Bread> breadList = new List<Bread>();

    }

    class Bread : PhysicalObject
    {
        enum BunType
        {
            EnglishMuffin,
            Baguette
        }

        private int _numOfCut;

        public int NumOfCut
        {
            get { return _numOfCut; }
            set
            {
                if (value < 0)
                {
                    _numOfCut = 0;
                }
                else if (value > int.MaxValue)
                {
                    _numOfCut = int.MaxValue;
                }
                else
                {
                    _numOfCut = value;
                }
            }
        }

        private int _distanceBtwCut;

        public int DistBtwCut
        {
            get { return _distanceBtwCut; }
            set
            {
                if (value < 0)
                {
                    _distanceBtwCut = 0;
                }
                else if (value > int.MaxValue)
                {
                    _distanceBtwCut = int.MaxValue;
                }
                else
                {
                    _distanceBtwCut = value;
                }
            }
        }

        public List<Cut> cutList = new List<Cut>();


    }

    public class BreadBlob
    {
        public enum VarianceType
        {
            All,
            Q1,
            Q2,
            Q3,
            Q4,
            QAverage,
            S1,
            S2,
            S3,
            S4,
            S5,
            S6,
            S7,
            S8,
            S9,
            Savg,
            L1,
            L2,
            L3,
            L4,
            L5,
            L6,
            L7,
            L8,
            L9,
            L10,
            L11,
            L12,
            L13,
            L14,
            L15,
            L16,
            Lavg

        }

        public struct BlobVariance
        {
            public double All { get; set; }
            public double Q1 { get; set; }
            public double Q2 { get; set; }
            public double Q3 { get; set; }
            public double Q4 { get; set; }
            public double QAverage { get; set; }
            public double S1 { get; set; }
            public double S2 { get; set; }
            public double S3 { get; set; }
            public double S4 { get; set; }
            public double S5 { get; set; }
            public double S6 { get; set; }
            public double S7 { get; set; }
            public double S8 { get; set; }
            public double S9 { get; set; }
            public double Savg { get; set; }
            public double L1 { get; set; }
            public double L2 { get; set; }
            public double L3 { get; set; }
            public double L4 { get; set; }
            public double L5 { get; set; }
            public double L6 { get; set; }
            public double L7 { get; set; }
            public double L8 { get; set; }
            public double L9 { get; set; }
            public double L10 { get; set; }
            public double L11 { get; set; }
            public double L12 { get; set; }
            public double L13 { get; set; }
            public double L14 { get; set; }
            public double L15 { get; set; }
            public double L16 { get; set; }
            public double Lavg { get; set; }
        }

        private int _X;
        private int _Y;
        private int _sideCount;
        private byte[,] _PixelArray;
        private BlobVariance _Variance;
        private byte _BackGroundThreshold;
        private int _TopDownThreshold;
        //private List<List<byte>> _OriginalPixelArray = new List<List<byte>>();
        private List<byte> _PixelList = new List<byte>();
        private Dictionary<String, List<byte>> _DictionaryPixelList = new Dictionary<String, List<byte>>();
        private List<List<byte>> _SectionPixelList = new List<List<byte>>();
        
        public BreadBlob()
        {
            _X = 0;
            _Y = 0;
            _BackGroundThreshold = 0;
            _TopDownThreshold = 0;
        }

        public BreadBlob(int X, int Y, byte BackGroundThreshold, int TopDownThreshold)
        {
            _X = X;
            _Y = Y;
            _BackGroundThreshold = BackGroundThreshold;
            _TopDownThreshold = TopDownThreshold;
        }

        public BreadBlob(int X, int Y, byte BackGroundThreshold, int TopDownThreshold, int SideCount)
        {
            _X = X;
            _Y = Y;
            _BackGroundThreshold = BackGroundThreshold;
            _TopDownThreshold = TopDownThreshold;
            _sideCount = SideCount;
        }
        public int X { get { return _X; } set { _X = value; } }
        public int Y { get { return _Y; } set { _Y = value; } }
        public int SideCount { get { return _sideCount; } set { _sideCount = value; } }

        public byte[,] PixelArray
        {
            get { return _PixelArray; }
            set
            {
                List<double> submatrixTemplist = new List<double>();

                List<List<Matrix<double>>> matrixList = new List<List<Matrix<double>>>();
                List<Matrix<double>> matrixList_temp = new List<Matrix<double>>();
                List<Matrix<double>> matrixList_1x1 = new List<Matrix<double>>();
                List<Matrix<double>> matrixList_2x2 = new List<Matrix<double>>();
                List<Matrix<double>> matrixList_3x3 = new List<Matrix<double>>();
                List<Matrix<double>> matrixList_4x4 = new List<Matrix<double>>();

                List<List<double>> matrixVarianceList = new List<List<double>>();
                List<double> matrixVarianceList_temp = new List<double>();
                List<double> matrixVarianceList_1x1 = new List<double>();
                List<double> matrixVarianceList_2x2 = new List<double>();
                List<double> matrixVarianceList_3x3 = new List<double>();
                List<double> matrixVarianceList_4x4 = new List<double>();

                double[,] inputArray = new double[value.GetLength(0), value.GetLength(1)];
                Array.Copy(value, inputArray, value.Length);
                Matrix<double> inputMatrix = DenseMatrix.OfArray(inputArray);
                

                for (int _sideCount = 1; _sideCount <= 4; _sideCount++)
                {
                    matrixVarianceList_temp.Clear();
                    matrixList_temp.Clear();


                    for (int i = 0; i < _sideCount; i++)
                    {
                        for (int j = 0; j < _sideCount; j++)
                        {
                            matrixList_temp.Add(inputMatrix.SubMatrix(inputMatrix.RowCount / _sideCount * i, inputMatrix.RowCount / _sideCount, inputMatrix.ColumnCount / _sideCount * j, inputMatrix.ColumnCount / _sideCount));
                        }
                    }
                    foreach (DenseMatrix listItem in matrixList_temp)
                    {
                        submatrixTemplist.Clear();
                        foreach (var tuple in listItem.EnumerateIndexed(Zeros.AllowSkip))
                        {
                            submatrixTemplist.Add(tuple.Item3);
                        }
                        matrixVarianceList_temp.Add(MathNet.Numerics.Statistics.Statistics.Variance(submatrixTemplist));
                    }
                    matrixVarianceList.Add(matrixVarianceList_temp);
                }

                #region 1x1
                int sideCount = 1;
                for (int i = 0; i < sideCount; i++)
                {
                    for (int j = 0; j < sideCount; j++)
                    {
                        matrixList_1x1.Add(inputMatrix.SubMatrix(inputMatrix.RowCount / sideCount * i, inputMatrix.RowCount / sideCount, inputMatrix.ColumnCount / sideCount * j, inputMatrix.ColumnCount / sideCount));
                    }
                }
                foreach (DenseMatrix listItem in matrixList_1x1)
                {
                    submatrixTemplist.Clear();
                    foreach (var tuple in listItem.EnumerateIndexed(Zeros.AllowSkip))
                    {
                        submatrixTemplist.Add(tuple.Item3);
                    }
                    matrixVarianceList_1x1.Add(MathNet.Numerics.Statistics.Statistics.Variance(submatrixTemplist));
                }
                _Variance.All = MathNet.Numerics.Statistics.Statistics.Variance(matrixVarianceList_1x1);
                #endregion
                
                
                #region 2x2

                sideCount = 2;
                for (int i = 0; i < sideCount; i++)
                {
                    for (int j = 0; j < sideCount; j++)
                    {
                        matrixList_2x2.Add(inputMatrix.SubMatrix(inputMatrix.RowCount / sideCount * i, inputMatrix.RowCount / sideCount, inputMatrix.ColumnCount / sideCount * j, inputMatrix.ColumnCount / sideCount));
                    }
                }
                foreach (DenseMatrix listItem in matrixList_2x2)
                {
                    submatrixTemplist.Clear();
                    foreach (var tuple in listItem.EnumerateIndexed(Zeros.AllowSkip))
                    {
                        submatrixTemplist.Add(tuple.Item3);
                    }
                    matrixVarianceList_2x2.Add(MathNet.Numerics.Statistics.Statistics.Variance(submatrixTemplist));
                }

                _Variance.Q1 = matrixVarianceList_2x2[2];
                _Variance.Q2 = matrixVarianceList_2x2[0];
                _Variance.Q3 = matrixVarianceList_2x2[1];
                _Variance.Q4 = matrixVarianceList_2x2[3];
                _Variance.QAverage = matrixVarianceList_2x2.Average();
                #endregion

                
                #region 3x3

                sideCount = 3;
                for (int i = 0; i < sideCount; i++)
                {
                    for (int j = 0; j < sideCount; j++)
                    {
                        matrixList_3x3.Add(inputMatrix.SubMatrix(inputMatrix.RowCount / sideCount * i, inputMatrix.RowCount / sideCount, inputMatrix.ColumnCount / sideCount * j, inputMatrix.ColumnCount / sideCount));
                    }
                }
                
                foreach (DenseMatrix listItem in matrixList_3x3)
                {
                    submatrixTemplist.Clear();
                    foreach (var tuple in listItem.EnumerateIndexed(Zeros.AllowSkip))
                    {
                        submatrixTemplist.Add(tuple.Item3);
                    }
                    matrixVarianceList_3x3.Add(MathNet.Numerics.Statistics.Statistics.Variance(submatrixTemplist));
                }

                _Variance.S1 = matrixVarianceList_3x3[0];
                _Variance.S2 = matrixVarianceList_3x3[1];
                _Variance.S3 = matrixVarianceList_3x3[2];
                _Variance.S4 = matrixVarianceList_3x3[3];
                _Variance.S5 = matrixVarianceList_3x3[4];
                _Variance.S6 = matrixVarianceList_3x3[5];
                _Variance.S7 = matrixVarianceList_3x3[6];
                _Variance.S8 = matrixVarianceList_3x3[7];
                _Variance.S9 = matrixVarianceList_3x3[8];


                matrixVarianceList_3x3.Sort();
                List<double> matrixVarianceList_smallElement_3x3 = new List<double>();
                for (int i = 0; i <= 7; i++)
                {
                    matrixVarianceList_smallElement_3x3.Add(matrixVarianceList_3x3[i]);
                }
                _Variance.Savg = matrixVarianceList_smallElement_3x3.Average();

#endregion

#region 4x4
                
                sideCount = 4;
                for(int i = 0; i < sideCount; i++)
                {
                    for(int j = 0; j < sideCount; j++)
                    {
                        matrixList_4x4.Add(inputMatrix.SubMatrix(inputMatrix.RowCount / sideCount * i, inputMatrix.RowCount / sideCount, inputMatrix.ColumnCount / sideCount * j, inputMatrix.ColumnCount / sideCount));
                    }
                }
                
                foreach (DenseMatrix listItem in matrixList_4x4)
                {
                    submatrixTemplist.Clear();
                    foreach (var tuple in listItem.EnumerateIndexed(Zeros.AllowSkip))
                    {
                        submatrixTemplist.Add(tuple.Item3);
                    }
                    matrixVarianceList_4x4.Add(MathNet.Numerics.Statistics.Statistics.Variance(submatrixTemplist));
                }

                _Variance.L1 = matrixVarianceList_4x4[0];
                _Variance.L2 = matrixVarianceList_4x4[1];
                _Variance.L3 = matrixVarianceList_4x4[2];
                _Variance.L4 = matrixVarianceList_4x4[3];
                _Variance.L5 = matrixVarianceList_4x4[4];
                _Variance.L6 = matrixVarianceList_4x4[5];
                _Variance.L7 = matrixVarianceList_4x4[6];
                _Variance.L8 = matrixVarianceList_4x4[7];
                _Variance.L9 = matrixVarianceList_4x4[8];
                _Variance.L10 = matrixVarianceList_4x4[9];
                _Variance.L11 = matrixVarianceList_4x4[10];
                _Variance.L12 = matrixVarianceList_4x4[11];
                _Variance.L13 = matrixVarianceList_4x4[12];
                _Variance.L14 = matrixVarianceList_4x4[13];
                _Variance.L15 = matrixVarianceList_4x4[14];
                _Variance.L16 = matrixVarianceList_4x4[15];
                matrixVarianceList_4x4.RemoveAt(10);
                matrixVarianceList_4x4.RemoveAt(9);
                matrixVarianceList_4x4.RemoveAt(6);
                matrixVarianceList_4x4.RemoveAt(5);

                matrixVarianceList_4x4.Sort();
                List<double> matrixVarianceList_smallElement_4x4 = new List<double>();
                for (int i = 4; i <= matrixVarianceList_4x4.Count - 6; i++)
                {
                    matrixVarianceList_smallElement_4x4.Add(matrixVarianceList_4x4[i]);
                }
                _Variance.Lavg = matrixVarianceList_smallElement_4x4.Average();
                //_Variance.All = MathNet.Numerics.Statistics.Statistics.Variance(newList);
#endregion
    
            }
        }
        public byte BackGroundThreshold { get { return _BackGroundThreshold; } set { _BackGroundThreshold = value; } }
        public int TopDownThreshold { get { return _TopDownThreshold; } set { _TopDownThreshold = value; } }
        public BlobVariance Variance { get { return _Variance; } private set { _Variance = value; } }

        private double GetQAverage()
        {
            return (_Variance.Q1 + _Variance.Q2 + _Variance.Q3 + _Variance.Q4) / 4;
        }

        public bool IsTop()
        {
            return (GetQAverage() > _TopDownThreshold);
        }

        public int GetVariance(VarianceType varianceType)
        {
            switch (varianceType)
            {
                case VarianceType.All:
                    //return Convert.ToInt16(_Variance.All);
                    return 0;
                case VarianceType.Q1:
                    return Convert.ToInt16(_Variance.Q1);
                case VarianceType.Q2:
                    return Convert.ToInt16(_Variance.Q2);
                case VarianceType.Q3:
                    return Convert.ToInt16(_Variance.Q3);
                case VarianceType.Q4:
                    return Convert.ToInt16(_Variance.Q4);
                case VarianceType.QAverage:
                    return Convert.ToInt16(_Variance.QAverage);
                case VarianceType.S1:
                    return Convert.ToInt16(_Variance.S1);
                case VarianceType.S2:
                    return Convert.ToInt16(_Variance.S2);
                case VarianceType.S3:
                    return Convert.ToInt16(_Variance.S3);
                case VarianceType.S4:
                    return Convert.ToInt16(_Variance.S4);
                case VarianceType.S5:
                    return Convert.ToInt16(_Variance.S5);
                case VarianceType.S6:
                    return Convert.ToInt16(_Variance.S6);
                case VarianceType.S7:
                    return Convert.ToInt16(_Variance.S7);
                case VarianceType.S8:
                    return Convert.ToInt16(_Variance.S8);
                case VarianceType.S9:
                    return Convert.ToInt16(_Variance.S9);
                case VarianceType.Savg:
                    return Convert.ToInt16(_Variance.Savg);
                case VarianceType.L1:
                    return Convert.ToInt16(_Variance.L1);
                case VarianceType.L2:
                    return Convert.ToInt16(_Variance.L2);
                case VarianceType.L3:
                    return Convert.ToInt16(_Variance.L3);
                case VarianceType.L4:
                    return Convert.ToInt16(_Variance.L4);
                case VarianceType.L5:
                    return Convert.ToInt16(_Variance.L5);
                case VarianceType.L6:
                    return Convert.ToInt16(_Variance.L6);
                case VarianceType.L7:
                    return Convert.ToInt16(_Variance.L7);
                case VarianceType.L8:
                    return Convert.ToInt16(_Variance.L8);
                case VarianceType.L9:
                    return Convert.ToInt16(_Variance.L9);
                case VarianceType.L10:
                    return Convert.ToInt16(_Variance.L10);
                case VarianceType.L11:
                    return Convert.ToInt16(_Variance.L11);
                case VarianceType.L12:
                    return Convert.ToInt16(_Variance.L12);
                case VarianceType.L13:
                    return Convert.ToInt16(_Variance.L13);
                case VarianceType.L14:
                    return Convert.ToInt16(_Variance.L14);
                case VarianceType.L15:
                    return Convert.ToInt16(_Variance.L15);
                case VarianceType.L16:
                    return Convert.ToInt16(_Variance.L16);
                case VarianceType.Lavg:
                    return Convert.ToInt16(_Variance.Lavg);
                default:
                    return 0;
            }
        }
    }

    public class StaticsCalculator
    {
        private double _Mean;
        private double _Variance;
        private double _StandardDeviation;
        private List<double> _NumberList = new List<double>();

        public StaticsCalculator()
        {
            _Mean = 0;
            _Variance = 0;
            _StandardDeviation = 0;
        }

        public StaticsCalculator(List<byte> List)
        {
            _Mean = 0;
            _Variance = 0;
            _StandardDeviation = 0;
            foreach (byte num in List)
            {
                _NumberList.Add((double)num);
            }
            UpdateVariance();
            UpdateStdDev();

        }
        public double Mean { get { return _Mean; } private set { _Mean = value; } }
        public double Variance { get { return _Variance; } private set { _Variance = value; } }
        public double StandardDeviation { get { return _StandardDeviation; } private set { _StandardDeviation = value; } }
        public List<byte> NumberList
        {
            get
            {
                List<byte> TempByteList = new List<byte>();
                foreach (double num in _NumberList)
                {
                    TempByteList.Add((byte)num);
                }
                return TempByteList;
            }
            set
            {
                foreach (byte num in value)
                {
                    _NumberList.Add((double)num);
                }
                UpdateVariance();
                UpdateStdDev();
            }
        }

        public void Add(double num)
        {
            _NumberList.Add(num);
            UpdateVariance();
            UpdateStdDev();
        }
        public void Add(byte num)
        {
            _NumberList.Add((double)num);
            UpdateVariance();
            UpdateStdDev();
        }
        private void UpdateVariance()
        {
            if (_NumberList.Count != 0)
            {
                _Mean = Convert.ToDouble(_NumberList.Average(num => Convert.ToDouble(num)));
            }

            for (int i = 0; i < _NumberList.Count; i++)
            {
                double difference = _NumberList[i] - _Mean;
                _Variance += (difference * difference);
            }
            if (_NumberList.Count != 0)
            {
                _Variance /= _NumberList.Count;
            }
        }

        private void UpdateStdDev()
        {
            _StandardDeviation = Math.Sqrt(_Variance);
        }

        public double CalculateWhat()
        {
            int MeanWhat = 0;
            int SDWhat = 0;

            for(int i = 0; i < _NumberList.Count; i++)
            {
                Mean += _NumberList[i];
                SDWhat += (int)(_NumberList[i] * _NumberList[i]);
            }

            MeanWhat /= _NumberList.Count;

            return Math.Sqrt(SDWhat/_NumberList.Count - MeanWhat * MeanWhat);
        }

        
    }

    class Cut
    {
        public int Type { get; set; }
        public double Angle_X { get; set; }
        public double Angle_Z { get; set; }
        public double Angle_Plow { get; set; }
        public int Depth { get; set; }
        public struct Center
        {
            int X { get; set; }
            int Y { get; set; }
            int Z { get; set; }
        }

        class CutPt
        {

        }
    }
}
