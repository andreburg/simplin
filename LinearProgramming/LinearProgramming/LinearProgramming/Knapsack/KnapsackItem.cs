using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinearProgramming.LinearProgramming
{
    internal class KnapsackItem : IComparable<KnapsackItem>
    {
        public double A;
        public double C;
        public double coeficient = 1;
        public bool chosen = false;
        public string name;
        public KnapsackItem(double C, double A, string name)
        { 
            this.A = A;
            this.C = C;
            this.name = name;
        }

        public KnapsackItem(KnapsackItem origin)
        {
            this.C = (double)origin.C;
            this.A = (double)origin.A;
            this.name = (string)origin.name;
            this.coeficient = (double)origin.coeficient;
            this.chosen = (bool)origin.chosen;
        }
        public KnapsackItem()
        {

        }

        public int CompareTo(KnapsackItem item)
        {
            return (int)((this.C / this.A) - (item.C / item.A));
        }
    }
}
