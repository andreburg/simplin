using LinearProgramming.Menu.ModelNavigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinearProgramming.Menu
{
    internal class KnapsackMenu
    {
        public KnapsackMenu(string fileString) 
        {
            new KnapsackNavigation(fileString);
        }
    }
}
