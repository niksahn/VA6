using System;
using static labaa5.Form1;

namespace labaa5
{
    public class Table
    {
        private int _Size;
        public int _Count;

        private double[] _XTable;
        private double[] _YTable;

        public Table(int n)
        {
            _Size = n;
            _Count = 0;
            _XTable = new double[n];
            for (int i = 0; i < n; i++) _XTable[i] = 10;
            _YTable = new double[n];
        }

        public void AddNode(double val)
        {
            foreach(double x in _XTable)
            {
                if (x == val) throw new Exception("Такое значение X уже существует");
            }
            if (_Count < _Size)
            {
                _XTable[_Count++] = val;
                Array.Sort(_XTable);
                for(int i = 0; i < _Size; i++)
                {
                    _YTable[i] = Variant10.function(_XTable[i]);
                }
            }
            else
            {
                throw new Exception("Максимальное количество узлов");
            }
        }

      

        public double GetX(int n)
        {
            return _XTable[n];
        }

        public double GetY(int n)
        {
            return _YTable[n];
        }

        public void Clear()
        {
            _Count = 0;
            for (int i = 0; i < _Size; i++)
            {
                _XTable[i] = 10;
            }
        }
    }
}
