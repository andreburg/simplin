using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LinearProgramming
{
    class FileHandler
    {
        public static string ReadFile(string path)
        {
			try
			{
				string fileContent = File.ReadAllText(path);
				return fileContent;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				return "";
			}
		}
    }
}
