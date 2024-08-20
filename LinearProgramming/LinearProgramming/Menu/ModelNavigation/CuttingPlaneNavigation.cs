using LinearProgramming.LinearProgramming;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinearProgramming.Menu.ModelNavigation
{
    internal class CuttingPlaneNavigation
    {
        SimplexModel model;
        SimplexTableua current;
        public CuttingPlaneNavigation(string fileString) 
        {
            this.model = new SimplexModel(new ProblemParser(fileString, true), false);
            InitiateNavigation();
        }

        public void InitiateNavigation()
        {
            Console.Clear();
            Console.WriteLine($"WELCOME TO SIMPLIN \nYour Linear Programming Solver.\n\n");

            Console.WriteLine($"::SIMPLEX SOLVER::\n\t|Cutting Plane|");
            this.current = this.model.rootNode;
            this.current.GetTable();
            Console.WriteLine("1. Navigate Tableua.");
            Console.WriteLine("2. View Optimal.");
            Console.WriteLine("\ne. Export Model.");

            Console.WriteLine("\nb. Go Back.");
            Console.WriteLine("m. To Main Menu.");

            try
            {
                string option = Console.ReadLine();
                switch (option)
                {
                    case "1":
                        NavigateTableua();
                        break;
                    case "2":
                        ViewOptimal();
                        break;
                    case "e":
                        ExportModel();
                        break;
                    case "b":
                        new SimplexMenu(MainMenu.fileString);
                        break;
                    case "m":
                        new MainMenu();
                        break;
                }
            }
            catch { }
        }

        public void NavigateTableua()
        {
            Console.Clear();
            if (this.current.next == null && this.current.branchL == null && this.current.branchR == null)
            {
                if (this.current.isInfeasible)
                {
                    Console.WriteLine("INFEASIBLE!\n");
                }
                else
                {
                    Console.WriteLine("OPTIMAL!\n");
                }
            }

            this.current.GetTable();
            Console.WriteLine("\n");

            if (this.current.next != null)
            {
                Console.WriteLine("n: Next.");
            }

            if (this.current.previous != null)
            {
                Console.WriteLine("p: Previous.");
            }

            Console.WriteLine("\nb. Go Back.");
            Console.WriteLine("m. To Main Menu.");

            try
            {
                string option = Console.ReadLine();
                switch (option)
                {
                    case "n":
                        if (this.current.next != null) NextT();
                        else NavigateTableua();
                        break;
                    case "p":
                        if (this.current.previous != null) PreviousT();
                        else NavigateTableua();
                        break;
                    case "b":
                        new CuttingPlaneNavigation(MainMenu.fileString);
                        break;
                    case "m":
                        new MainMenu();
                        break;
                    default:
                        NavigateTableua();
                        break;
                }
            }
            catch
            {
                NavigateTableua();
            }
        }

        public void NextT()
        {
            Console.Clear();
            this.current = this.current.next;
            NavigateTableua();
        }

        public void PreviousT()
        {
            Console.Clear();
            this.current = this.current.previous;
            NavigateTableua();
        }

        public void ViewOptimal()
        {
            Console.Clear();
            this.current = this.model.bestCandidate;
            NavigateTableua();
        }

        public void ExportModel()
        {
            Console.Clear();
            Console.WriteLine("Please specify the output location:");
            try
            {
                string outputDir = Console.ReadLine();
                using (StreamWriter writer = new StreamWriter(outputDir))
                {
                    this.model.Export(this.model.rootNode, writer);
                }
                Console.Clear();
                Console.WriteLine("Model Exported!");
                Console.ReadKey();
            }
            catch
            {
                Console.WriteLine("Error");
                Console.ReadKey();
            }
            new CuttingPlaneNavigation(MainMenu.fileString);
        }
    }
}
