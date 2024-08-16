using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinearProgramming.LinearProgramming.Simplex
{
    internal class CuttingPlaneSimplex : SimplexTableua
    {
        public CuttingPlaneSimplex(List<string> X, List<string> Xbv, Matrix<double> C, Matrix<double> A, Matrix<double> b, int objective, List<string> Xint) : base(X, Xbv, C, A, b, objective, Xint)
        {
        }
    }
}
