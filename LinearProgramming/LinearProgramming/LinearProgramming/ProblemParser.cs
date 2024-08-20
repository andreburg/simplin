using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LinearProgramming.LinearProgramming
{
    internal class ProblemParser
    {
        public List<string> X;
        public List<string> Xbv;
        public List<string> Xint = new string[] { }.ToList();
        public bool branch;

        public List<Constraint> constraints;

        public Matrix<double> A, b, C = Matrix<double>.Build.Dense(1, 1);
        public int objective = 1;
        public ProblemParser(string input, bool includeRestrictions)
        {
            List<string> splitInput = input.Split(new[] { "\r\n" }, StringSplitOptions.None).ToList();
            string objectiveFunction = splitInput[0];
            List<string> constraintStrings = splitInput.ToList().GetRange(1, splitInput.Count - 1);

            GroupCollection objectiveFunctionGroups = Regex.Match(objectiveFunction, @"(max|min)(.*)$").Groups;
            double[] oCoeff = objectiveFunctionGroups[2].Value.Trim().Split(' ')
                .Select((coeficient) => double.Parse(coeficient)).ToArray();
            this.objective = objectiveFunctionGroups[1].Value.Trim() == "max" ? 1 : -1;
            this.constraints = GetConstraints(constraintStrings);

            if (includeRestrictions) GetSignRestrictions(constraintStrings[constraintStrings.Count - 1]);

            this.Xbv = constraints.Select(c => c.variables[c.variables.Count - 1]).ToList();

            this.X = oCoeff.Select((_, i) => $"x{i + 1}")
                .Concat(
                    constraints.Select((constraint) => constraint.variables[constraint.variables.Count - 1])
                )
                .ToList();

            Matrix<double> cMatrix = Matrix<double>.Build.DenseOfRowVectors(constraints.Select((constraint) => constraint.GetCannonical(X)).ToArray());

            this.A = cMatrix.SubMatrix(0, cMatrix.RowCount, 0, cMatrix.ColumnCount - 1);
            this.b = cMatrix.SubMatrix(0, cMatrix.RowCount, cMatrix.ColumnCount - 1, 1);
            this.C = Matrix<double>.Build.DenseOfRowArrays(oCoeff).Append(Matrix<double>.Build.Dense(1, constraints.Count, 0)) * (-1);
        }

        private List<Constraint> GetConstraints(List<string> constraintStrings)
        {
            List<Constraint> constraints = new List<Constraint>();
            List<string> regConstraints = constraintStrings.Where((constraint) => Regex.IsMatch(constraint, @"[<=|>=|=]+")).ToList();

            for (int i = 0; i < regConstraints.Count; i++)
            {
                GroupCollection constraintGroups = Regex.Match(constraintStrings[i], @"^([^<=|>=|=]*)(<=|>=|=)(.*)").Groups;
                double[] coefs = constraintGroups[1].Value.Trim().Split(' ').Select(double.Parse).ToArray();
                string sign = constraintGroups[2].Value.Trim();
                double rhs = double.Parse(constraintGroups[3].Value);

                if (sign == "=")
                {
                    constraints.Add(new Constraint(coefs, ConstraintType.lte, rhs, i + 1));
                    constraints.Add(new Constraint(coefs, ConstraintType.gte, rhs, i + 1));
                }
                else
                    constraints.Add(new Constraint(coefs, sign == "<=" ? ConstraintType.lte : ConstraintType.gte, rhs, i + 1));
            }
            return constraints;
        }

        private List<string> GetSignRestrictions(string signConstraints)
        {
            List<string> restrictions = signConstraints.Split(' ').ToList();
            for (int i = 0; i < restrictions.Count; i++)
            {
                switch (restrictions[i])
                {
                    case "urs":
                        break;
                    case "bin":
                        this.Xint.Add($"x{i + 1}");
                        this.constraints.Add(new Constraint(restrictions.Select((x, j) => j == i ? 1.0 : 0.0).ToArray(), ConstraintType.lte, 1, constraints.Count + 1));
                        break;
                    case "int":
                        this.Xint.Add($"x{i + 1}");
                        break;
                    case "+":
                        break;
                    case "-":
                        break;
                }
            }
            return Xint;
        }
    }
}
