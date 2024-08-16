using LinearProgramming.LinearProgramming.Alogrithms;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinearProgramming.LinearProgramming
{
    internal class KnapsackModel
    {
        public List<double> C, A;
        public double b;
        public bool max, st;
        public List<KnapsackTableua> candidates = new List<KnapsackTableua>();
        public KnapsackTableua bestCandidate;

        public KnapsackModel(List<double> C, List<double> A, double b, bool max, bool st) 
        {
            this.C = C;
            this.b = b;
            this.A = A;

            List<KnapsackItem> items = new List<KnapsackItem>();

            for (int i = 0; i < C.Count; i++) items.Add(new KnapsackItem(this.C[i], this.A[i], $"x{i + 1}"));
            items = items.OrderBy(x => (x.C / x.A) * (max ? -1 : 1)).ToList();

            KnapsackTableua t_1 = new KnapsackTableua(new List<KnapsackItem>(items), this);

            PreorderTraversal(t_1);

            for (int i = 0; i < candidates.Count; i++)
            {
                candidates[i].candidate = i + 1;
                candidates[i].GetTable();
            }

            this.bestCandidate = candidates.Find(c => c.z == candidates.Max(candidate => candidate.z));
            Console.WriteLine("BEST CANDIDATE:\n");
            this.bestCandidate.GetTable();
        }

        public void PreorderTraversal(KnapsackTableua node)
        {
            if (node == null)
            {
                return;
            }

            if (node.takeT == null && node.leaveT == null && !node.infeasible)
            {
                candidates.Add(node);
            }

            PreorderTraversal(node.takeT);
            PreorderTraversal(node.leaveT);
        }


    } 
};