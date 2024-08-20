using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinearProgramming.LinearProgramming
{
    public enum ConstraintType
    {
        gte,
        lte
    }

    internal class Constraint
    {
        public double[] coeficients;
        double rhs;
        public ConstraintType type;
        public List<string> variables;
        public bool pOfE;
        public int number;

        public Constraint(double[] coeficients, ConstraintType type, double rhs, int constraintNumber, bool pOfE = false) { 
            this.number = constraintNumber;
            this.coeficients = coeficients.Append(type == ConstraintType.gte ? -1 : 1).ToArray();
            this.rhs = rhs;
            this.type = type;
            this.variables = coeficients.Select((_, i) => $"x{i + 1}").Append(type == ConstraintType.gte ? $"e{constraintNumber}" : $"s{constraintNumber}").ToList();
        }

        public Vector<double> GetCannonical(List<string> modelVariables)
        {
            Vector<double> cannonical = Vector<double>.Build.DenseOfArray(modelVariables.Select((x, i) =>
            {
                int coeficientIndex = this.variables.IndexOf(x);
                return (coeficientIndex != -1 ? this.coeficients[coeficientIndex] : 0);
            }).Append(this.rhs).ToArray());
            return cannonical * (this.type == ConstraintType.lte ? 1 : -1);
        }
    }
}
