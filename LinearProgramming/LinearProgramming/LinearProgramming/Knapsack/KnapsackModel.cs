using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinearProgramming.LinearProgramming
{
    internal class KnapsackModel
    {
        public List<double> C, A;
        public double b;
        public List<KnapsackTableua> candidates = new List<KnapsackTableua>();
        public KnapsackTableua bestCandidate;
        public KnapsackTableua rootNode;

        public KnapsackModel(ProblemParser problem) 
        {
            this.C = (problem.C * -1).ToColumnMajorArray().ToList();
            this.b = problem.b.ToColumnMajorArray().ToList()[0];
            this.A = problem.A.ToColumnMajorArray().ToList();

            List<KnapsackItem> items = new List<KnapsackItem>();

            for (int i = 0; i < C.Count; i++) items.Add(new KnapsackItem(this.C[i], this.A[i], $"x{i + 1}"));
            items = items.OrderBy(x => (x.C / x.A) * problem.objective).ToList();

            this.rootNode = new KnapsackTableua(new List<KnapsackItem>(items), this);

            Iter(rootNode);

            for (int i = 0; i < candidates.Count; i++)
            {
                candidates[i].candidate = i + 1;
                candidates[i].GetTable();
            }

            this.bestCandidate = candidates.Find(c => c.z == candidates.Max(candidate => candidate.z));
            Console.WriteLine("BEST CANDIDATE:\n");
            this.bestCandidate.GetTable();
        }

        public void Iter(KnapsackTableua node)
        {
            if (node == null) return;
            if (node.takeT == null && node.leaveT == null && !node.infeasible) candidates.Add(node);
            Iter(node.takeT);
            Iter(node.leaveT);
        }


        public void Export(KnapsackTableua node, StreamWriter writer)
        {
            if (node == null) return;
            node.GetTable(writer);
            Export(node.takeT, writer);
            Export(node.leaveT, writer);
        }


    } 
};