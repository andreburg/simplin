using LinearProgramming.LinearProgramming;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinearProgramming.Menu.ModelNavigation
{
    internal class BranchAndBoundNavigation
    {
        SimplexModel model;
        SimplexTableua current;
        public BranchAndBoundNavigation(string fileString) 
        {
            this.model = new SimplexModel(new ProblemParser(fileString, true));
            InitiateNavigation();
        }

        public void InitiateNavigation()
        {
            Console.Clear();
            Console.WriteLine($"WELCOME TO SIMPLIN \nYour Linear Programming Solver.\n\n");

            Console.WriteLine($"::SIMPLEX SOLVER::\n\t|B&B|\n\n");

            this.current = this.model.rootNode;
            this.current.GetTable();
            Console.WriteLine("1. Navigate Tableua.");
            Console.WriteLine("2. View Optimal.");
            Console.WriteLine("3. View All Candidates.");
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
                    case "3":
                        ViewCandidates();
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
            } catch 
            {
                InitiateNavigation();
            }
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
            Console.WriteLine("\n\n");

            if (this.current.next != null)
            {
                Console.WriteLine("n: Next.");
            }

            if (this.current.branchL != null || this.current.branchR != null)
            {
                Console.WriteLine("l: Branch Lower.");
                Console.WriteLine("u: Branch Upper.");
            }

            if (this.current.previous != null)
            {
                Console.WriteLine("p: Previous.");
            }

            Console.WriteLine("\nb: Go Back.");
            Console.WriteLine("m: To Main Menu.");

            try
            {
                string option = Console.ReadLine();
                switch (option)
                {
                    case "n":
                        if (this.current.next != null) NextT();
                        else NavigateTableua();
                        break;
                    case "l":
                        if (this.current.branchL != null) LowerBranch();
                        else NavigateTableua();
                        break;
                    case "u":
                        if (this.current.branchR != null) UpperBranch();
                        else NavigateTableua();
                        break;
                    case "p":
                        if (this.current.previous != null) PreviousT();
                        else NavigateTableua();
                        break;
                    case "b":
                        new BranchAndBoundNavigation(MainMenu.fileString);
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

        public void LowerBranch()
        {
            Console.Clear();
            this.current = this.current.branchL;
            NavigateTableua();
        }

        public void UpperBranch()
        {
            Console.Clear();
            this.current = this.current.branchR;
            NavigateTableua();
        }

        public void ViewOptimal()
        {
            Console.Clear();
            this.current = this.model.bestCandidate;
            NavigateTableua();
        }

        public void ViewCandidates()
        {
            Console.Clear();
            foreach (SimplexTableua candidate in model.candidates)
            {
                Console.WriteLine($"Candidate: {candidate.candidate}");
                candidate.GetTable();
                Console.WriteLine("\n");
            }
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
            new BranchAndBoundNavigation(MainMenu.fileString);
        }
    }
}
