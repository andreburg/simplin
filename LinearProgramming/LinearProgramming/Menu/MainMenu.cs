using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LinearProgramming.LinearProgramming;

namespace LinearProgramming.Menu
{
    internal class MainMenu
    {
        public static string problemPath;
        public static string fileString;
        public MainMenu()
        {
            Home();
        }

        public static void Home()
        {
            Console.Clear();
            Console.WriteLine($"WELCOME TO SIMPLIN \nYour Linear Programming Solver.\n\n");

            if (problemPath != null)
            {
                Console.WriteLine($"Path :: '${problemPath}'");
                SelectModel();
            }
            else
            {
                SelectProblem();
            }
        }

        public static void SelectProblem()
        {
            Console.Clear();
            Console.WriteLine($"WELCOME TO SIMPLIN \nYour Linear Programming Solver.\n\n");
            Console.WriteLine($"Please specify the path to your problem:");
            try
            {
                problemPath = Console.ReadLine().Trim();
                fileString = FileHandler.ReadFile(problemPath);
                ProblemParser problem = new ProblemParser(fileString, true);
                Home();
            }
            catch
            {
                problemPath = null;
                fileString = null;
                Console.Clear();
                Console.WriteLine("Please select a valid file with the correct format: \nInput Text File Criteria\r\nThe first line contains the following, seperated by spaces:\r\n• The word max or min, to indicate whether it is a maximization or a minimization problem.\r\n• For each decision variable, a operator to represent wheter the objective function coefficient is a\r\nnegative or positive.\r\n• For each decision variable, a number to represent its objective function coefficient.\r\nA line for each constraint:\r\n• The operator of the technological coefficients for the decision variables, in the same order as in\r\nthe specification of the objective function in line 1, that represents whether the technological\r\ncoefficient is negative or positive.\r\n• The technological coefficients for the decision variables, in the same order as in the specification\r\nof the objective function in line 1.\r\n• The relation used in the constraint, with =,<=, or >=, to indicate respectively, an inequality to\r\nconstraint the constaint right-hand-side.\r\n• The right-hand-side of the constraint.\r\nSign Restrictions\r\n• Sign restriction to be below all the constraints, seperated by a space, +, -, urs, int, bin, in the\r\nsame order as in the specification of the objective function in line 1.");
                Console.ReadKey();
                Home();
            }
        }

        public static void SelectModel()
        {
            Console.Clear();
            Console.WriteLine($"WELCOME TO SIMPLIN \nYour Linear Programming Solver.\n\n");
            Console.WriteLine("Please Select A Model:\n");
            Console.WriteLine("1. Simplex.");
            Console.WriteLine("2. Knapsack.");

            Console.WriteLine("\nc. Select different problem.");
            Console.WriteLine("q. Quit.");


            try
            {
                string option = Console.ReadLine();
                switch (option)
                {
                    case "1":
                        new SimplexMenu(fileString);
                        break;
                    case "2":
                        new KnapsackMenu(fileString);
                        break;
                    case "c":
                        SelectProblem();
                        break;
                    case "q":
                        Console.Clear();
                        Environment.Exit(0);
                        break;
                    default:
                        SelectModel();
                        break;
                }
            }
            catch
            {
                Home();
            }
        }
    }
}
