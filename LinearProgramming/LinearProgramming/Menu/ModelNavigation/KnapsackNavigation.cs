using LinearProgramming.LinearProgramming;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinearProgramming.Menu.ModelNavigation
{
    internal class KnapsackNavigation
    {
        KnapsackModel model;
        KnapsackTableua current;
        public KnapsackNavigation(string fileString)
        {
            this.model = new KnapsackModel(new ProblemParser(fileString, false));
            InitiateNavigation();
        }

        public void InitiateNavigation()
        {
            Console.Clear();
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
                string option =Console.ReadLine();
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
            }
            catch { }
        }

        public void NavigateTableua()
        {
            Console.Clear();
            if (this.current.leaveT == null && this.current.takeT == null)
            {
                if (this.current.infeasible)
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

            if (this.current.leaveT != null && this.current.takeT != null && !this.current.infeasible)
            {
                Console.WriteLine("l: Leave Item Branch.");
                Console.WriteLine("t: Take Item Branch.");
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
                    case "l":
                        if (this.current.leaveT != null && !this.current.infeasible) LeaveBranch();
                        else NavigateTableua();
                        break;
                    case "t":
                        if (this.current.takeT != null && !this.current.infeasible) TakeBranch();
                        else NavigateTableua();
                        break;
                    case "p":
                        if (this.current.previous != null) PreviousT();
                        else NavigateTableua();
                        break;
                    case "b":
                        new KnapsackNavigation(MainMenu.fileString);
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

        public void LeaveBranch()
        {
            Console.Clear();
            this.current = this.current.leaveT;
            NavigateTableua();
        }

        public void TakeBranch()
        {
            Console.Clear();
            this.current = this.current.takeT;
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

        public void ViewCandidates()
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
            new KnapsackNavigation(MainMenu.fileString);
        }
    }
}
