using MathNet.Numerics.LinearAlgebra;
using Microsoft.SolverFoundation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinearProgramming.LinearProgramming.Simplex
{
    internal class SimplexSensitivityAnalysis
    {
        List<Constraint> constraints;
        public List<string> X;
        public Matrix<double> b, C = Matrix<double>.Build.Dense(1, 1);

        public SimplexSensitivityAnalysis(List<Constraint> constraints,  List<string> X, Matrix<double> b, Matrix<double> C) 
        { 
            this.constraints = constraints;
            this.X = X;
            this.b = b;
            this.C = C;
        }

        public void SolveWithMicrosoftSolver()
        {
            SolverContext context = SolverContext.GetContext();
            Model model = context.CreateModel();

            // Create and add decisions
            List<Decision> decisions = new List<Decision>();
            foreach (var variable in X)
            {
                Decision decision = new Decision(Domain.RealNonnegative, variable);
                decisions.Add(decision);
                model.AddDecision(decision);
            }

            // Build and print the objective function
            Term objectiveTerm = 0;
            Console.WriteLine("Objective Function:");
            for (int i = 0; i < C.ColumnCount; i++)
            {
                objectiveTerm += C[0, i] * decisions[i];
                Console.Write($"{C[0, i]} * {decisions[i].Name}");
                if (i < C.ColumnCount - 1) Console.Write(" + ");
            }
            Console.WriteLine();
            model.AddGoal("Goal", GoalKind.Maximize, objectiveTerm);

            // Build, print, and add constraints
            Console.WriteLine("\nConstraints:");
            for (int i = 0; i < constraints.Count; i++)
            {
                Term constraintTerm = 0;

                for (int j = 0; j < X.Count; j++)
                {
                    constraintTerm += constraints[i].coeficients[j] * decisions[j];
                }

                if (constraints[i].pOfE)
                {
                    if (i < constraints.Count - 1 && constraints[i + 1].number == constraints[i].number)
                    {
                        Console.Write($"Constraint {i}: ");
                        Console.WriteLine($"{constraintTerm} == {b[i, 0]}");
                        model.AddConstraint($"Constraint{i}", constraintTerm == b[i, 0]);
                    }
                }
                else
                {
                    if (constraints[i].type == ConstraintType.gte)
                    {
                        Console.Write($"Constraint {i}: ");
                        Console.WriteLine($"{constraintTerm} >= {b[i, 0]}");
                        model.AddConstraint($"Constraint{i}", constraintTerm >= b[i, 0]);
                    }
                    else
                    {
                        Console.Write($"Constraint {i}: ");
                        Console.WriteLine($"{constraintTerm} <= {b[i, 0]}");
                        model.AddConstraint($"Constraint{i}", constraintTerm <= b[i, 0]);
                    }
                }
            }

            // Solve the model
            Solution solution = context.Solve();

            // Get and print the solution report
            Report report = solution.GetReport();
            Console.WriteLine("\nSolution Report:");
            Console.WriteLine(report);

            // Print the dual values
            Console.WriteLine("\nDual Values:");
            foreach (var constraint in model.Constraints)
            {
            }

            // Print the values of decisions
            Console.WriteLine("\nDecision Variable Values:");
            foreach (var decision in decisions)
            {
                Console.WriteLine($"{decision.Name} = {decision.GetDouble()}");
            }
        }

      

    }
}
