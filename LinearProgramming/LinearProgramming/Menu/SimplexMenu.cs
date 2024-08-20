using LinearProgramming.LinearProgramming;
using LinearProgramming.Menu.ModelNavigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinearProgramming.Menu
{
    internal class SimplexMenu
    {
        public string fileString;
        public SimplexMenu(string fileString) 
        {
            this.fileString = fileString;
            InitiateSimplexMenu();
        }

        public void InitiateSimplexMenu()
        {

            ProblemParser p = new ProblemParser(fileString, true);

            if (p.Xint.Count > 0)
            {
                Console.Clear();
                Console.WriteLine($"WELCOME TO SIMPLIN \nYour Linear Programming Solver.\n\n");

                Console.WriteLine($"::SIMPLEX SOLVER::");

                Console.WriteLine("How would you like to handle integer constraints?\n");

                Console.WriteLine("1. Branch and bound.");
                Console.WriteLine("2. Cutting plane.");

                Console.WriteLine("\nm. To Main Menu.");

                try
                {
                    string option = Console.ReadLine();
                    switch (option)
                    {
                        case "1":
                            new BranchAndBoundNavigation(fileString);
                            break;
                        case "2":
                            new CuttingPlaneNavigation(fileString);
                            break;
                        case "m":
                            MainMenu.Home();
                            break;
                        default:
                            InitiateSimplexMenu();
                            break;
                    }
                }
                catch
                {
                    InitiateSimplexMenu();
                }
            }
            else
            {
                new CuttingPlaneNavigation(fileString);
            }
        }
    }
}
