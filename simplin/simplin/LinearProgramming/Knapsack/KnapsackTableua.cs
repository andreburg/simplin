using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinearProgramming.LinearProgramming
{
    internal class KnapsackTableua
    {
        public bool infeasible = false;
        public KnapsackTableua takeT;
        public KnapsackTableua leaveT;
        public List<KnapsackItem> items;

        public double z;
        public int candidate;

        public KnapsackTableua(List<KnapsackItem> items, KnapsackModel model)
        {
            this.items = items.Select(i => new KnapsackItem(i)).ToList();
            double available = model.b;
            List<KnapsackItem> chosen = new List<KnapsackItem>(this.items.Where(item => item.chosen).ToList());
            KnapsackItem choose = new KnapsackItem();

            foreach (KnapsackItem item in this.items)
            {
                if (!choose.chosen)
                {
                    if (available - (item.A * (item.chosen ? item.coeficient : 1)) < 0)
                    {
                        if (item.chosen)
                        {
                            this.infeasible = true;
                        }
                        else
                        {
                            item.coeficient = available / item.A;
                            choose = new KnapsackItem(item);
                            choose.chosen = true;
                        }
                    }
                    else
                    {
                        this.z += item.C * (item.chosen ? item.coeficient : 1);
                        available -= (item.A * (item.chosen ? item.coeficient : 1));
                    }
                }
                else
                {
                    item.coeficient = 0;
                }
            }
            List<KnapsackItem> toBeChosen = new List<KnapsackItem>(this.items.Where(item => !item.chosen && !(item.name == choose.name)).ToList());

            KnapsackItem leaveItem = new KnapsackItem(choose);
            KnapsackItem takeItem = new KnapsackItem(choose);

            leaveItem.coeficient = 0;
            takeItem.coeficient = 1;

            if (choose.chosen)
            {
                this.z = 0;
                this.leaveT = new KnapsackTableua(chosen.Append(leaveItem).Concat(new List<KnapsackItem>(toBeChosen)).ToList(), model);
                this.takeT = new KnapsackTableua(chosen.Append(takeItem).Concat(new List<KnapsackItem>(toBeChosen)).ToList(), model);
            }
        }

        public void GetTable()
        {
            string zS = (this.takeT == null && this.leaveT == null && !this.infeasible) ? Convert.ToString(this.z) : this.infeasible ? "Infeasible" : "n/a";
            Console.WriteLine($"ITEM \t\tCOEFICIENT \t\t Z::{zS}");
            foreach (KnapsackItem item in this.items)
            {
                Console.WriteLine($"{item.name} \t\t{item.coeficient}");
            }
            string cS = this.candidate != 0 ? Convert.ToString(this.candidate) : "n/a";
            Console.WriteLine($"Candidate::{cS}");
            Console.WriteLine($"\n");
        }
    }
}
