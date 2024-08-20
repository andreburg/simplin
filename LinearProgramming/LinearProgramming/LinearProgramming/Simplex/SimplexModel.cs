using LinearProgramming.LinearProgramming;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace LinearProgramming.LinearProgramming
{
    internal class SimplexModel
    {
        public List<SimplexTableua> candidates = new List<SimplexTableua>();
        public SimplexTableua bestCandidate;

        public SimplexTableua rootNode;

        public List<string> X;
        public List<string> Xbv;
        List<string> Xint = new string[] { }.ToList();
        public bool branch;

        public List<Constraint> constraints;

        public Matrix<double> A, b, C = Matrix<double>.Build.Dense(1,1);
        public int objective = 1;
        public SimplexModel(ProblemParser problem, bool branch = true)
        {
            this.branch = branch;

            this.objective = problem.objective;
            this.Xbv = problem.Xbv;
            this.X = problem.X;
            this.A = problem.A;
            this.b = problem.b;
            this.C = problem.C;
            this.Xint = problem.Xint;

            this.rootNode = new SimplexTableua(X, Xbv, C, A, b, objective, this.Xint, branch);

            for(int i = 0; i < candidates.Count; i++)
            {
                candidates[i].candidate = i;
            }

            this.bestCandidate = candidates.Find(c => c.z == candidates.Max(candidate => candidate.z));
        }

        public void Export(SimplexTableua node, StreamWriter writer)
        {
            if (node == null)
            {
                return;
            }

            if(node.next == null && node.branchL == null && node.branchR == null)
            {
                candidates.Add(node);
            }
            node.GetTable(writer);
            Export(node.next, writer);
            Export(node.branchL, writer);
            Export(node.branchR, writer);
        }
    }
}
