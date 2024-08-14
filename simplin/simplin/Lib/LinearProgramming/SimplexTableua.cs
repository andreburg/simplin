using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinearProgramming.LinearProgramming
{
    internal class SimplexTableua
    {
        public SimplexTableua previous;

        int objective;

        List<string> X = new string[] { }.ToList();
        List<string> Xbv = new string[] { }.ToList();

        List<string> Xint = new string[] { }.ToList();

        bool feasible = false;
        bool optimal = false;

        bool isInfeasible = false;
        bool isOptimal = false;

        public int pivotRowIndex, pivotColumnIndex = -1;
        private static MatrixBuilder<double> M = Matrix<double>.Build;
        private static VectorBuilder<double> V = Vector<double>.Build;

        Matrix<double> C = M.DenseOfArray(new double[,] { });
        Matrix<double> A = M.DenseOfArray(new double[,] { });
        Matrix<double> b = M.DenseOfArray(new double[,] { });
        public double z = 0;

        public SimplexTableua(List<string> X, List<string> Xbv, Matrix<double> C, Matrix<double> A, Matrix<double> b, int objective, List<string> Xint)
        {
            this.X = X;
            this.Xbv = Xbv;
            this.C = C;
            this.A = A;
            this.b = b;
            this.objective = objective;
            this.Xint = Xint;

            this.IteratateSimplex();
        }
        public SimplexTableua(SimplexTableua previous)
        {
            SetFromPrevious((SimplexTableua)previous.MemberwiseClone());
            this.IteratateSimplex();
        }

        public SimplexTableua(SimplexTableua previous, string constraintName, Tuple<Matrix<double>, double> constraint)
        {
            SetFromPrevious((SimplexTableua)previous.MemberwiseClone());

            int nextIndex = this.X.Where((name) => !name.Contains('x')).Select(name => int.Parse(name.Substring(1))).Max() + 1;
            this.X.Add(constraintName + nextIndex);
            this.Xbv.Add(constraintName + nextIndex);

            this.C = this.C.InsertColumn(this.C.ColumnCount, V.DenseOfArray(new double[] { 0 }));
            this.A = this.A.InsertColumn(this.A.ColumnCount, V.DenseOfArray((this.A.SubMatrix(0, this.A.RowCount, 0, 1) * 0).ToColumnMajorArray()));
            this.A = this.A.InsertRow(this.A.RowCount, V.DenseOfArray(constraint.Item1.ToRowMajorArray()));
            this.b = b.InsertRow(b.RowCount,V.DenseOfArray(new double[] { constraint.Item2 }));

            this.IteratateSimplex();
        }

        private void IteratateSimplex()
        {
            SolveTableua();

            this.feasible = this.b.ForAll(e => Math.Round(e, 10, MidpointRounding.AwayFromZero) >= 0);
            this.optimal = this.C.ForAll(e => (e * this.objective) >= 0);

            if (!this.feasible) PivotDual();
            else if (!this.optimal) PivotSimplex();

            GetTable();

            if (isInfeasible) return;

            if (!(this.feasible && this.optimal)) this.Next();
            else
            {
                List<string> branchXbv = GetBasicVariables(this.Xbv, this.pivotRowIndex, this.pivotColumnIndex, this.X);
                bool signResFulfilled = Xint.All(x => !branchXbv.Contains(x) || (Math.Round(b.ToColumnMajorArray()[branchXbv.ToList().IndexOf(x)], 10, MidpointRounding.AwayFromZero) % 1 == 0));

                if (!signResFulfilled)
                {
                    double minRhs = b.ToColumnMajorArray().Select((val, i) => (Math.Abs(val - 0.5) % 1) + (Xint.Contains(branchXbv[i]) ? 0 : 1)).Min();
                    string basicInt = Xint.Where((x) => branchXbv.Contains(x)).ToList().Find((x) => Math.Round(Math.Abs((0.5 - b.ToColumnMajorArray()[branchXbv.ToList().IndexOf(x)]) % 1), 10) == Math.Round(minRhs, 10));
                    Branch(basicInt);
                }
            }
        }

        private void SolveTableua()
        {
            Matrix<double> Cbv = V.DenseOfArray(this.Xbv.ToList().Select((bv) =>  (this.C * -1).ToColumnMajorArray()[this.X.IndexOf(bv)]).ToArray()).ToRowMatrix();
            Matrix<double> B = M.DenseOfColumnArrays(this.Xbv.ToList().Select((bv) => this.A.ToColumnArrays()[this.X.IndexOf(bv)]).ToArray());
            Matrix<double> Cbv_B_1 = Cbv * B.Inverse();
            this.C = (Cbv_B_1 * this.A) - (this.C * -1);
            this.A = (B.Inverse() * this.A).Map((value) => Math.Round(value, 10, MidpointRounding.ToEven));
            this.z = (Cbv_B_1 * this.b).ToColumnMajorArray().Min() - (this.z * -1);
            this.b = B.Inverse() * this.b;
        }

        private void PivotDual()
        {
            try
            {
                double minRhs = this.b.AsColumnMajorArray().ToList().Where(e => e < 0).Min();
                this.pivotRowIndex = this.b.Find((e) => e == minRhs).Item1;
                Matrix<double> pivotRowNeg = this.A.SubMatrix(this.pivotRowIndex, 1, 0, this.A.ColumnCount).PointwiseMinimum(0);
                Matrix<double> pivotRowRatio = this.C.PointwiseDivide(pivotRowNeg).PointwiseAbs();
                double minRatio = pivotRowRatio.ToRowMajorArray().Where((val) => val.IsFinite() && val > 0).Min();
                this.pivotColumnIndex = pivotRowRatio.Find((val) => val == minRatio).Item2;
            }
            catch (Exception ex)
            {
                this.isInfeasible = true;
            }
        }

        private void PivotSimplex()
        {
            double pivotValue = this.objective >= 0 
                ? this.C.ToRowMajorArray().Select(value => value * this.objective).Where(value => value < 0).Min() 
                : this.C.ToRowMajorArray().Select(value => value * this.objective).Where(value => value > 0).Max();
            this.pivotColumnIndex = this.C.Find((val) => val == pivotValue).Item2;
            Matrix<double> pivotColumn = this.A.SubMatrix(0, this.A.RowCount, this.pivotColumnIndex, 1);
            Matrix<double> pivotColumnRatio = this.b.PointwiseDivide(pivotColumn);
            Matrix<double> degenRows = pivotColumnRatio.Map(value => value == 0 ? 1.0 : 0.0).PointwiseMultiply(pivotColumn.Map(value => value > 0 ? 1.0 : 0.0));
            double minRatio = pivotColumnRatio.ToColumnMajorArray().Where((val) => val > 0).Min();
            Tuple<int, int, double> degenRowElement = degenRows.Find((val) => val == 1);
            this.pivotRowIndex = degenRowElement != null ? degenRowElement.Item1 : pivotColumnRatio.Find((val) => val == minRatio).Item1;
        }

        private List<string> GetBasicVariables(List<string> Xbv, int pivotRowIndex, int pivotColumnIndex, List<string> X)
        {
            List<string> newXbv = Xbv;
            // Update the basic variables using the previous table's pivot column and row.
            newXbv = Xbv;
            newXbv[pivotRowIndex] = X[pivotColumnIndex];
            return newXbv;
        }

        public SimplexTableua Next()
        {
            return new SimplexTableua(this);
        }

        public Tuple<SimplexTableua, SimplexTableua> Branch(string variable)
        {
            double branchValue = this.b.ToColumnMajorArray()[this.Xbv.ToList().IndexOf(variable)];
            double lb = - (branchValue - Math.Floor(branchValue));
            double ub = branchValue - Math.Ceiling(branchValue);

            Matrix<double> branchRow = this.A.SubMatrix(this.Xbv.ToList().IndexOf(variable), 1, 0, this.A.ColumnCount);

            Matrix<double> br = branchRow * 0;
            br[0, X.ToList().IndexOf(variable)] = 1;
            Matrix<double> lbr = (-(branchRow - br)).Append(M.SparseOfColumnMajor(1, 1, new double[] { 1 }));
            Matrix<double> ubr = (branchRow - br).Append(M.SparseOfColumnMajor(1,1, new double[] {1}));

            Tuple<Matrix<double>, double> lc = new Tuple<Matrix<double>, double>(lbr, lb);
            Tuple<Matrix<double>, double> uc = new Tuple<Matrix<double>, double>(ubr, ub);

            SimplexTableua lower = new SimplexTableua((SimplexTableua)this.MemberwiseClone(), "s", lc);
            SimplexTableua upper = new SimplexTableua((SimplexTableua)this.MemberwiseClone(), "e", uc);

            return new Tuple<SimplexTableua, SimplexTableua> (lower, upper);
        }
        public SimplexTableua Cut(string variable)
        {
            return new SimplexTableua(this);
        }

        public void GetTable()
        {
            Matrix<double> lhs = this.A.InsertRow(0, V.DenseOfArray(this.C.ToRowMajorArray()));
            Matrix<double> rhs = this.b.InsertRow(0, V.DenseOfArray(new double[] { z }));
            Matrix<double> ti = lhs.InsertColumn(lhs.ColumnCount, V.DenseOfArray(rhs.ToColumnMajorArray()));

            Console.WriteLine(ti.ToString());
        }
        public void SetFromPrevious(SimplexTableua previous)
        {
            this.previous = previous;
            this.X = new List<string>(this.previous.X);
            this.Xbv = new List<string>(this.previous.Xbv);
            this.C = this.previous.C.Clone();
            this.A = this.previous.A.Clone();
            this.b = this.previous.b.Clone();
            this.objective = this.previous.objective;
            this.z = this.previous.z;
            this.Xint = new List<string>(this.previous.Xint);
            this.pivotColumnIndex = this.previous.pivotColumnIndex;
            this.pivotRowIndex = this.previous.pivotRowIndex;
            this.Xbv = GetBasicVariables(this.Xbv, this.pivotRowIndex, this.pivotColumnIndex, this.X);
        }
    }
}