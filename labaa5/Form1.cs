using System;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace labaa5
{
    public partial class Form1 : Form
    {
        private Table _NodesTable;
        SplineTuple[] spl; // Сплайн

        private struct SplineTuple
        {
            public double a, b, c, d, x;
        }

        
        public Form1()
        {
            InitializeComponent();


            Series splineSeries = new Series("Интерполяция");
            splineSeries.ChartType = SeriesChartType.Spline;
            Graph.Series.Add(splineSeries);

            Table.ColumnCount = 2;
            Table.Columns[0].HeaderCell.Value = "x";
            Table.Columns[1].HeaderCell.Value = "f(x)";

            Series functionSeries = new Series("10ln(2x/(1+x))");
            functionSeries.ChartType = SeriesChartType.Line;
            for (double x = Variant10.range.min; x <= Variant10.range.max; x += 0.1)
            {
                functionSeries.Points.AddXY(x, Variant10.function(x));
            }
            chart1.Series.Add(functionSeries);

            _NodesTable = new Table(Variant10.NodesCount);
        }

        private void AddNodeClick(object sender, EventArgs e)
        {
            try
            {
                _NodesTable.AddNode((double)PointNUD.Value);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            if (_NodesTable._Count > 1)
            {
                BuildSpline(_NodesTable);
            }
            RefreshTable();
            RefreshGraph();
        }

        private void RefreshTable()
        {
            Table.RowCount = 0;
            for (int i = 0; i < _NodesTable._Count; i++)
            {
                double nodeX = _NodesTable.GetX(i);

                Table.RowCount++;
                Table.Rows[Table.RowCount - 1].Cells[0].Value = nodeX.ToString();
                Table.Rows[Table.RowCount - 1].Cells[1].Value = _NodesTable.GetY(i);
            }
        }

        private void RefreshGraph()
        {
            Graph.Series[0].Points.Clear();
            if (_NodesTable._Count < 1) return;
            Graph.Series[0].ChartType = SeriesChartType.Line;
            for (double x = _NodesTable.GetX(0); x <= _NodesTable.GetX(_NodesTable._Count-1); x += 0.1) 
            {
                Graph.Series[0].Points.AddXY(x, Interpolate(x));
            }
        }

        // Построение сплайна
        public void BuildSpline(Table nodesTable)
        {
            int n = _NodesTable._Count;
            // Инициализация массива сплайнов
            spl = new SplineTuple[n];
            for (int i = 0; i < n; ++i)
            {
                spl[i].x = _NodesTable.GetX(i);
                spl[i].a = _NodesTable.GetY(i);
            }
            spl[0].c = spl[n - 1].c = 0.0;

            //Прямой ход метода прогонки
            double[] alpha = new double[n - 1];
            double[] beta = new double[n - 1];
            alpha[0] = beta[0] = 0.0;
            for (int i = 1; i < n - 1; ++i)
            {
                double xi = _NodesTable.GetX(i);
                double xi_1=_NodesTable.GetX(i - 1);
                double xi__1=_NodesTable.GetX(i + 1);

                double hi=xi-xi_1;
                double hi1 = xi__1 - xi;

                double yi = _NodesTable.GetY(i);
                double yi_1 = _NodesTable.GetY(i - 1);
                double yi__1 = _NodesTable.GetY(i + 1);

                double A = hi;
                double C = 2.0 * (hi + hi1);
                double B = hi1;
                double F = 6.0 * ((yi__1 -yi) / hi1 - (yi - yi_1) / hi);
                double z = (A * alpha[i - 1] + C);
                alpha[i] = -B / z;
                beta[i] = (F - A * beta[i - 1]) / z;
            }

            // Обратный ход метода прогонки
            for (int i = n - 2; i > 0; --i)
            {
                spl[i].c = alpha[i] * spl[i + 1].c + beta[i];
            }

            // По известным коэффициентам c[i] находим значения b[i] и d[i]

            for (int i = n - 1; i > 0; --i)
            {
                double hi = _NodesTable.GetX(i) - _NodesTable.GetX(i - 1);
                spl[i].d = (spl[i].c - spl[i - 1].c) / hi;
                spl[i].b = hi * (2.0 * spl[i].c + spl[i - 1].c) / 6.0 + (_NodesTable.GetY(i) - _NodesTable.GetY(i - 1)) / hi;
            }
        }


        // Вычисление значения интерполированной функции в произвольной точке
        public double Interpolate(double x)
        {
            if (spl == null)
            {
                return double.NaN; // Если сплайны ещё не построены - возвращаем NaN
            }

            int n = spl.Length;
            SplineTuple s;

            if (x <= spl[0].x) // Если x меньше точки сетки x[0] - пользуемся первым эл-тов массива
            {
                s = spl[0];
            }
            else if (x >= spl[n - 1].x) // Если x больше точки сетки x[n - 1] - пользуемся последним эл-том массива
            {
                s = spl[n - 1];
            }
            else // Иначе x лежит между граничными точками сетки - производим бинарный поиск нужного эл-та массива
            {
                int i = 0;
                int j = n - 1;
                while (i + 1 < j)
                {
                    int k = i + (j - i) / 2;
                    if (x <= spl[k].x)
                    {
                        j = k;
                    }
                    else
                    {
                        i = k;
                    }
                }
                s = spl[j];
            }

            double dx = x - s.x;
            // Вычисляем значение сплайна в заданной точке по схеме Горнера
            return s.a + (s.b + (s.c / 2.0 + s.d * dx / 6.0) * dx) * dx;
        }
        private void ClearButton(object sender, EventArgs e)
        {
            _NodesTable.Clear();
            RefreshGraph();
            RefreshTable();
        }
        public delegate double Function(double x);
        public class Variant10
        {
            public static readonly Function function =
                (double x) => { return 10 * Math.Log(2 * x / (1 + x)); };

            public static readonly (int min, int max) range = (1, 5);
            public const int NodesCount = 8;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void Graph_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
