using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.SolverFoundation.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace LinearProgramming.LinearProgramming
{
    internal class SimplexTableua
    {
        public SimplexTableua root;
        public SimplexTableua previous;
        public SimplexTableua next;
        public SimplexTableua branchL;
        public SimplexTableua branchR;

        string handleInt;

        bool branch;

        int objective;

        public int candidate;

        string branchVar;

        List<string> X = new string[] { }.ToList();
        List<string> Xbv = new string[] { }.ToList();
        List<string> Xint = new string[] { }.ToList();

        public bool feasible = false;
        public bool optimal = false;
        public bool signResFulfilled = false;

        public bool isInfeasible = false;
        public bool isUnbounded = false;

        public int pivotRowIndex, pivotColumnIndex = -1;
        private static MatrixBuilder<double> M = Matrix<double>.Build;
        private static VectorBuilder<double> V = Vector<double>.Build;

        Matrix<double> C = M.DenseOfArray(new double[,] { });
        Matrix<double> A = M.DenseOfArray(new double[,] { });
        Matrix<double> b = M.DenseOfArray(new double[,] { });

        Matrix<double> Cbv = M.DenseOfArray(new double[,] { });
        Matrix<double> B = M.DenseOfArray(new double[,] { });

        public double z = 0;

        public SimplexTableua(List<string> X, List<string> Xbv, Matrix<double> C, Matrix<double> A, Matrix<double> b, int objective, List<string> Xint, bool branch = true)
        {
            this.X = X;
            this.Xbv = Xbv;
            this.C = C;
            this.A = A;
            this.b = b;
            this.objective = objective;
            this.Xint = Xint;
            this.branch = branch;
            this.root = this;

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

        public SimplexTableua(SimplexTableua initial, List<string> Xbv ,string constraintName, Tuple<Matrix<double>, double> constraint)
        {
            this.previous = initial;
            this.root = initial.root;
            this.X = new List<string>(this.previous.X);
            this.Xbv = new List<string>(this.previous.Xbv);
            this.handleInt = this.previous.handleInt;
            this.C = this.previous.C.Clone();
            this.A = this.previous.A.Clone();
            this.b = this.previous.b.Clone();
            this.objective = this.previous.objective;
            this.z = this.previous.z;
            this.Xint = new List<string>(this.previous.Xint);
            this.pivotColumnIndex = this.previous.pivotColumnIndex;
            this.pivotRowIndex = this.previous.pivotRowIndex;
            int nextIndex = this.X.Where((name) => !name.Contains('x')).Select(name => int.Parse(name.Substring(1))).Max() + 1;
            this.X.Add(constraintName + nextIndex);
            this.Xbv.Add(constraintName + nextIndex);

            this.C = this.C.InsertColumn(this.C.ColumnCount, V.DenseOfArray(new double[] { 0 }));
            this.A = this.A.InsertColumn(this.A.ColumnCount, V.DenseOfArray((this.A.SubMatrix(0, this.A.RowCount, 0, 1) * 0).ToColumnMajorArray()));
            this.A = this.A.InsertRow(this.A.RowCount, V.DenseOfArray(constraint.Item1.ToRowMajorArray()));
            this.b = b.InsertRow(b.RowCount, V.DenseOfArray(new double[] { constraint.Item2 }));

            this.IteratateSimplex();

            this.previous.next = this;
        }

        private void IteratateSimplex()
        {
            SolveTableua();

            this.feasible = this.b.ForAll(e => Math.Round(e, 10, MidpointRounding.AwayFromZero) >= 0);
            this.optimal = this.C.ForAll(e => (e * this.objective) >= 0);

            if (!this.feasible) PivotDual();
            else if (!this.optimal) PivotSimplex();

            if (isInfeasible) return;

            if (!(this.feasible && this.optimal)) this.next = new SimplexTableua(this);
            else
            {
                List<string> branchXbv = GetBasicVariables(this.Xbv, this.pivotRowIndex, this.pivotColumnIndex, this.X);
                signResFulfilled = Xint.All(x => !branchXbv.Contains(x) || Math.Round(b.ToColumnMajorArray()[branchXbv.ToList().IndexOf(x)], 10) % 1 == 0);

                if (!signResFulfilled)
                {
                    List<double> rhsSelect = b.ToColumnMajorArray().Select((val, i) =>  Math.Abs(0.5 - Math.Round(val % 1, 10)) + (Xint.Contains(branchXbv[i]) ? 0 : 1)).ToList();
                    double minRhs = rhsSelect.Min();
                    string basicInt = Xint.Where((x) => branchXbv.Contains(x)).ToList().Find((x) => Math.Abs(0.5 - Math.Round(b.ToColumnMajorArray()[branchXbv.ToList().IndexOf(x)] % 1, 10)) == minRhs);
                    if (this.branch) Branch(basicInt);
                    else Cut(basicInt);
                }
            }

            if(this.next != null)
            {
                this.next.previous = this;
            }

            if(this.branchL != null && this.branchR != null)
            {
                this.branchL.previous = this;
                this.branchR.previous = this;
            }
        }

        private void SolveTableua()
        {
            this.Cbv = V.DenseOfArray(this.Xbv.ToList().Select((bv) =>  (this.C * -1).ToColumnMajorArray()[this.X.IndexOf(bv)]).ToArray()).ToRowMatrix();
            this.B = M.DenseOfColumnArrays(this.Xbv.ToList().Select((bv) => this.A.ToColumnArrays()[this.X.IndexOf(bv)]).ToArray());
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
            catch
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
            newXbv = Xbv;
            newXbv[pivotRowIndex] = X[pivotColumnIndex];
            return newXbv;
        }

        public void Branch(string variable)
        {
            this.branchVar = variable;
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

            this.branchL = new SimplexTableua((SimplexTableua)this.MemberwiseClone(), "s", lc);
            this.branchR = new SimplexTableua((SimplexTableua)this.MemberwiseClone(), "e", uc);
        }

        public void Cut(string variable)
        {
            double branchValue = this.b.ToColumnMajorArray()[this.Xbv.ToList().IndexOf(variable)];
            double b = -(branchValue % 1);

            Matrix<double> branchRow = this.A.SubMatrix(this.Xbv.ToList().IndexOf(variable), 1, 0, this.A.ColumnCount);
            Matrix<double> br = (-(branchRow.Modulus(1))).Append(M.SparseOfColumnMajor(1, 1, new double[] { 1 }));

            Tuple<Matrix<double>, double> cc = new Tuple<Matrix<double>, double>(br, b);
            this.next = new SimplexTableua((SimplexTableua)this.MemberwiseClone(), "s", cc);
        }

        public void GetXbvRange(string x)
        {
            Matrix<double> Cbv = V.DenseOfArray(this.Xbv.ToList().Select((bv) => (root.C * -1).ToColumnMajorArray()[this.X.IndexOf(bv)]).ToArray()).ToRowMatrix();
            Matrix<double> B = M.DenseOfColumnArrays(this.Xbv.ToList().Select((bv) => root.A.ToColumnArrays()[this.X.IndexOf(bv)]).ToArray());

            Matrix<double> Cbv_l = V.DenseOfArray(this.Xbv.ToList().Select((bv) => (root.C * -1).ToColumnMajorArray()[this.X.IndexOf(bv)]).ToArray()).ToRowMatrix();
            Matrix<double> Cbv_u = V.DenseOfArray(this.Xbv.ToList().Select((bv) => (root.C * -1).ToColumnMajorArray()[this.X.IndexOf(bv)]).ToArray()).ToRowMatrix();

            for (int i = 0; i < Cbv.AsRowMajorArray().Length; i++)
            {
                Matrix<double> Cbv_B_1_l = Cbv_l * B.Inverse();
                Matrix<double> C_l = (Cbv_B_1_l * root.A) - (root.C * -1);

                int iter = 100000;

                while (!C_l.ForAll((v) => objective == 1 ? v <= 0 : v >= 0))
                {
                    Cbv_l = V.DenseOfArray(Cbv.AsRowMajorArray().Select((val, ind) => val + (ind == i ? -iter : 0)).ToArray()).ToRowMatrix();
                    Cbv_B_1_l = Cbv_l * B.Inverse();
                    C_l = (Cbv_B_1_l * root.A) - (root.C * -1);
                    iter -= 1;
                }

                Matrix<double> Cbv_B_1_u = Cbv_u * B.Inverse();
                Matrix<double> C_u = (Cbv_B_1_u * root.A) - (root.C * -1);

                iter = 100000;

                while (!C_l.ForAll((v) => objective == 1 ? v <= 0 : v >= 0))
                {
                    Cbv_u = V.DenseOfArray(Cbv.AsRowMajorArray().Select((val, ind) => val + (ind == i ? iter : 0)).ToArray()).ToRowMatrix();
                    Cbv_B_1_u = Cbv_u * B.Inverse();
                    C_u = (Cbv_B_1_u * root.A) - (root.C * -1);
                    iter -= 1;
                }
            }

        }


        public void AddConstraintAndSolve()
        {
            Console.WriteLine("Enter constraint to be added:");
            Console.WriteLine("e.g. +0 +3 +1 <= 0");
            string constraintInput = Console.ReadLine();

            try
            {
                constraintInput.Split(' ');
            }
            catch
            {

            }
        }

        public void AddActivityAndSolve()
        {
            Console.WriteLine("Enter activity to be added:");
            Console.WriteLine("e.g. +1 +0 +5 +3 int; corresponding to the model");
            string constraintInput = Console.ReadLine();

            try
            {
                constraintInput.Split(' ');
            }
            catch
            {

            }
        }

        public void GetDual()
        {
            Matrix<double> Cbv_B_1 = this.Cbv * this.B.Inverse();
            double duality = (this.Cbv * this.B.Inverse() * root.b).ToColumnMajorArray()[0];
            if(objective == 1)
            {
                Console.WriteLine($"Min w = {duality}");
            }
            else
            {
                Console.WriteLine($"Max w = {duality}");
            }
            Console.WriteLine(duality - this.z == 0 ? $"Strong Duality :: {duality - this.z}" : $"Weak Duality :: {duality - this.z}");
        }

        public void GetShadowPrices()
        {
            Matrix<double> Cbv_B_1 = this.Cbv * this.B.Inverse() * -1;

            List<double> bs = b.AsColumnMajorArray().ToList();
            List<double> shadowPrices = bs.Select((x, i) => ((Cbv_B_1 + V.DenseOfArray(Cbv_B_1.ToRowMajorArray().Select((y, j) => j == i ? 1.0 : 0.0).ToArray()).ToRowMatrix()) * b).ToArray()[0,0]).ToList();

            Console.WriteLine($"Shadow Prices:");
            for (int i = 0; i < shadowPrices.Count; i++)
            {
                Console.WriteLine($"Constraint {i + 1}: {shadowPrices[i]}");
            }
            
            foreach (var item in shadowPrices)
            {
                Console.WriteLine(item);
            }
        }

        public void GetTable()
        {

            Matrix<double> lhs = this.A.InsertRow(0, V.DenseOfArray(this.C.ToRowMajorArray()));
            Matrix<double> rhs = this.b.InsertRow(0, V.DenseOfArray(new double[] { z }));
            Matrix<double> ti = lhs.InsertColumn(lhs.ColumnCount, V.DenseOfArray(rhs.ToColumnMajorArray()));

            List<double[]> rows = ti.ToRowArrays().ToList();
            List<string> header = new List<string>(X);
            header.Insert(X.Count, "rhs");
            int[] maxLengths = header.Select(h => h.Length).ToArray();

            for (int j = 0; j < maxLengths.Length; j++)
            {
                foreach (var row in rows)
                {
                    int currLength = Convert.ToString(row[j]).Length;
                    if (currLength > maxLengths[j]) maxLengths[j] = currLength;
                }
            }

            Console.Write("    | ");
            for (int i = 0; i < header.Count; i++)
            {
                string item = Convert.ToString(header[i]).PadRight(maxLengths[i]);

                if ((i == pivotColumnIndex && next != null) || branchVar == item)
                {
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write(item);
                    if (!(i == pivotRowIndex + 1))
                    {
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.ResetColor();
                    Console.Write(item);
                }
                
                if(i < header.Count - 1) Console.Write(" | ");
            }
            Console.Write("\n");

            for (int i = 0; i < rows.Count; i++)
            {
                if(i > 0)
                {
                    if (i == pivotRowIndex + 1 && next != null)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write($"({i}) | ");
                        if (!(i == pivotRowIndex + 1))
                        {
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        Console.ResetColor();
                        Console.Write($"({i}) | ");
                    }
                }
                else
                {
                    Console.Write("(z) | ");
                }


                for (int j = 0; j < rows[i].Length; j++)
                {
                    string item = Convert.ToString(Math.Round(rows[i][j], 3)).PadRight(maxLengths[j]);

                    if ((i == pivotRowIndex + 1 || j == pivotColumnIndex) && next != null)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write(item);
                        if (!(i == pivotRowIndex + 1))
                        {
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        Console.ResetColor();
                        Console.Write(item);
                    }

                    if (j < rows[i].Length - 1)
                        Console.Write(" | ");
                }
                Console.ResetColor();
                Console.WriteLine();
            }
        }

        public void GetTable(StreamWriter writer)
        {
            // if (previous != null) GetDual();
            // if (previous != null) GetShadowPrices();

            if(branchL == null && branchR == null && next == null && !this.isInfeasible)
            {
                writer.WriteLine("\n");
                writer.WriteLine($"CANDIDATE :: {candidate}");
            }

            if (this.isInfeasible)
            {
                writer.WriteLine("\n");
                writer.WriteLine($"INFEASIBLE !!");
            }

            Matrix<double> lhs = this.A.InsertRow(0, V.DenseOfArray(this.C.ToRowMajorArray()));
            Matrix<double> rhs = this.b.InsertRow(0, V.DenseOfArray(new double[] { z }));
            Matrix<double> ti = lhs.InsertColumn(lhs.ColumnCount, V.DenseOfArray(rhs.ToColumnMajorArray()));

            List<double[]> rows = ti.ToRowArrays().ToList();
            List<string> header = new List<string>(X);
            header.Insert(X.Count, "rhs");
            int[] maxLengths = header.Select(h => h.Length).ToArray();

            for (int j = 0; j < maxLengths.Length; j++)
            {
                foreach (var row in rows)
                {
                    int currLength = Convert.ToString(row[j]).Length;
                    if (currLength > maxLengths[j]) maxLengths[j] = currLength;
                }
            }

            writer.Write("    | ");
            for (int i = 0; i < header.Count; i++)
            {
                string item = Convert.ToString(header[i]).PadRight(maxLengths[i]);
                writer.Write(item);

                if (i < header.Count - 1) writer.Write(" | ");
            }
            writer.Write("\n");

            for (int i = 0; i < rows.Count; i++)
            {
                if (i > 0)
                {
                    writer.Write($"({i}) | ");
                }
                else
                {
                    writer.Write("(z) | ");
                }


                for (int j = 0; j < rows[i].Length; j++)
                {
                    string item = Convert.ToString(Math.Round(rows[i][j], 3)).PadRight(maxLengths[j]);

                    writer.Write(item);

                    if (j < rows[i].Length - 1)
                        writer.Write(" | ");
                }
                writer.Write("\n");
            }

            writer.WriteLine("\n\n");

        }


        public void SetFromPrevious(SimplexTableua previous)
        {
            this.previous = previous;
            this.root = previous.root;
            this.branch = this.previous.branch;
            this.X = new List<string>(this.previous.X);
            this.Xbv = new List<string>(this.previous.Xbv);
            this.handleInt = this.previous.handleInt;
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

        public void AddActivity()
        {

        }

        public void AddConstraint()
        {

        }
    }
}