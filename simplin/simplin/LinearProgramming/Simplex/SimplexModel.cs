using LinearProgramming.LinearProgramming;
using LinearProgramming.LinearProgramming.Simplex;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace LinearProgramming.LinearProgramming
{
    internal class SimplexModel
    {
        public List<SimplexTableua> candidates = new List<SimplexTableua>();
        public SimplexTableua bestCandidate;

        public List<string> X;
        public List<string> Xbv;
        List<string> Xint = new string[] { }.ToList();
        public bool branch;

        public double z;
        public Matrix<double> A, b, C = Matrix<double>.Build.Dense(1,1);
        public int objective = 1;
        public SimplexModel(string input, bool branch = true)
        {
            this.branch = branch;
            List<string> splitInput = input.Split('\n').ToList();
            string objectiveFunction = splitInput[0];
            List<string> constraintStrings = splitInput.ToList().GetRange(1, splitInput.Count - 1);

            GroupCollection objectiveFunctionGroups = Regex.Match(objectiveFunction, @"(max|min)(.*)$").Groups;
            double[] oCoeff = objectiveFunctionGroups[2].Value.Trim().Split(' ')
                .Select((coeficient) => double.Parse(coeficient)).ToArray();
            this.objective = objectiveFunctionGroups[1].Value.Trim() == "max" ? 1 : -1;
            List<Constraint> constraints = GetConstraints(constraintStrings);
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

            GetSignRestrictions(constraintStrings[constraintStrings.Count - 1]);
            SimplexTableua ti = new SimplexTableua(X, Xbv, C, A, b, objective, this.Xint, branch ? "branch" : "cut");

            PreorderTraversal(ti);

            for(int i = 0; i < candidates.Count; i++)
            {
                candidates[i].candidate = i;
                candidates[i].GetTable();
            }

            this.bestCandidate = candidates.Find(c => c.z == candidates.Max(candidate => candidate.z));

            SimplexSensitivityAnalysis se = new SimplexSensitivityAnalysis(constraints, X.GetRange(0, oCoeff.Length).ToList(), b, C.SubMatrix(0,1,0, oCoeff.Length)*-1);
            se.SolveWithMicrosoftSolver();
        }

        public void PreorderTraversal(SimplexTableua node)
        {
            if (node == null)
            {
                return;
            }

            if(node.next == null && node.branchL == null && node.branchR == null)
            {
                candidates.Add(node);
            }

            node.GetTable();
            PreorderTraversal(node.next);
            PreorderTraversal(node.branchL);
            PreorderTraversal(node.branchR);
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
            for(int i = 0; i < restrictions.Count; i++)
            {
                switch (restrictions[i]) 
                { 
                    case "urs":
                        break;
                    case "bin":
                        this.Xint.Add(X[i]);
                        break;
                    case "int":
                        this.Xint.Add(X[i]);
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
