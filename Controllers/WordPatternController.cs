using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Controllers.GenericClasses;
using System.Linq.Dynamic.Core;

namespace Sammlerplattform.Controllers
{
    public class WordPatternController(IWebHostEnvironment env) : Controller
    {
        private readonly IWebHostEnvironment _env = env;

        public ActionResult PrüfeOrt()
        {
            string germanWordlist = Path.Combine(Path.Combine(_env.WebRootPath, "dic"), "Orte_WhiteList.txt");
            List<string> Orte = [.. System.IO.File.ReadAllLines(germanWordlist)];

            List<string> similaritiesInCitiesEnd = [.. WordPattern.WordSimilartyEndOfWord(Orte, 2).Keys];
            Dictionary<string, int> similaritiesInCitiesEnd2 = WordPattern.WordSimilartyEndOfWord(similaritiesInCitiesEnd, 2);
            StreamWriter stream2 = new(Path.Combine(Path.Combine(_env.WebRootPath, "dic"), "Orte_Ende_result.txt"), true);
            foreach (KeyValuePair<string, int> sic in similaritiesInCitiesEnd2)
            {
                stream2.WriteLine(sic.Key);
            }
            stream2.Close();

            List<string> similaritiesInCitiesBeginning = [.. WordPattern.WordSimilartyBeginningOfWord(Orte, 2).Keys];
            Dictionary<string, int> similaritiesInCitiesBeginning2 = WordPattern.WordSimilartyBeginningOfWord(similaritiesInCitiesBeginning, 2);
            StreamWriter stream3 = new(Path.Combine(Path.Combine(_env.WebRootPath, "dic"), "Orte_Beginn_result.txt"), true);
            foreach (KeyValuePair<string, int> sic in similaritiesInCitiesBeginning2)
            {
                stream3.WriteLine(sic.Key);
            }
            stream3.Close();

            return RedirectToAction("Frontpage", "Home");
        }

        public ActionResult PrüfeNachname()
        {
            string germanWordlist = Path.Combine(Path.Combine(_env.WebRootPath, "dic"), "Nachnamen_WhiteList.txt");
            List<string> Nachnamen = [.. System.IO.File.ReadAllLines(germanWordlist)];

            List<string> similaritiesInNamesEnd = [.. WordPattern.WordSimilartyEndOfWord(Nachnamen, 3).Keys];
            Dictionary<string, int> similaritiesInNamesEnd2 = WordPattern.WordSimilartyEndOfWord(similaritiesInNamesEnd, 3);
            StreamWriter stream2 = new(Path.Combine(Path.Combine(_env.WebRootPath, "dic"), "Nachnamen_Ende_result.txt"), true);
            foreach (KeyValuePair<string, int> sin in similaritiesInNamesEnd2.Where(x => x.Value > 2))
            {
                stream2.WriteLine(sin.Key);
            }
            stream2.Close();

            List<string> similaritiesInNamesBeginning = [.. WordPattern.WordSimilartyBeginningOfWord(Nachnamen, 3).Keys];
            Dictionary<string, int> similaritiesInNamesBeginning2 = WordPattern.WordSimilartyBeginningOfWord(similaritiesInNamesBeginning, 3);
            StreamWriter stream3 = new(Path.Combine(Path.Combine(_env.WebRootPath, "dic"), "Nachnamen_Beginn_result.txt"), true);
            foreach (KeyValuePair<string, int> sin in similaritiesInNamesBeginning2)
            {
                stream3.WriteLine(sin.Key);
            }
            stream3.Close();

            return RedirectToAction("Frontpage", "Home");
        }

        public ActionResult PrüfeVorname()
        {
            string germanWordlist = Path.Combine(Path.Combine(_env.WebRootPath, "dic"), "Vornamen_WhiteList.txt");
            List<string> Vornamen = [.. System.IO.File.ReadAllLines(germanWordlist)];

            Dictionary<string, int> similaritiesInNamesEnd = WordPattern.WordSimilartyEndOfWord(Vornamen, 2);
            StreamWriter stream2 = new(Path.Combine(Path.Combine(_env.WebRootPath, "dic"), "Vornamen_Ende_result.txt"), true);
            foreach (KeyValuePair<string, int> sin in similaritiesInNamesEnd.Where(x => x.Value > 2))
            {
                stream2.WriteLine(sin.Key);
            }
            stream2.Close();

            Dictionary<string, int> similaritiesInNamesBeginning = WordPattern.WordSimilartyBeginningOfWord(Vornamen, 2);
            StreamWriter stream3 = new(Path.Combine(Path.Combine(_env.WebRootPath, "dic"), "Vornamen_Beginn_result.txt"), true);
            foreach (KeyValuePair<string, int> sin in similaritiesInNamesBeginning)
            {
                stream3.WriteLine(sin.Key);
            }
            stream3.Close();

            return RedirectToAction("Frontpage", "Home");
        }

        public ActionResult PrüfeGebäude()
        {
            string gebäudeWordlist = Path.Combine(Path.Combine(_env.WebRootPath, "dic"), "Gebäude_WhiteList.txt");
            List<string> gebäude = [.. System.IO.File.ReadAllLines(gebäudeWordlist)];

            List<string> similaritiesInNamesEnd = [.. WordPattern.WordSimilartyEndOfWord(gebäude, 3).Keys];
            Dictionary<string, int> similaritiesInNamesEnd2 = WordPattern.WordSimilartyEndOfWord(similaritiesInNamesEnd, 3);
            StreamWriter stream2 = new(Path.Combine(Path.Combine(_env.WebRootPath, "dic"), "Gebäude_Ende_result.txt"), true);
            foreach (KeyValuePair<string, int> sin in similaritiesInNamesEnd2)
            {
                stream2.WriteLine(sin.Key);
            }
            stream2.Close();

            List<string> similaritiesInNamesBeginning = [.. WordPattern.WordSimilartyBeginningOfWord(gebäude, 3).Keys];
            Dictionary<string, int> similaritiesInNamesBeginning2 = WordPattern.WordSimilartyBeginningOfWord(similaritiesInNamesBeginning, 3);
            StreamWriter stream3 = new(Path.Combine(Path.Combine(_env.WebRootPath, "dic"), "Gebäude_Beginn_result.txt"), true);
            foreach (KeyValuePair<string, int> sin in similaritiesInNamesBeginning2)
            {
                stream3.WriteLine(sin.Key);
            }
            stream3.Close();

            return RedirectToAction("Frontpage", "Home");
        }
    }
}
