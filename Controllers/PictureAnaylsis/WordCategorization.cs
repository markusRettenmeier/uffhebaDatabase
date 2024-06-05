using Catalyst;
using Mosaik.Core;
using Sammlerplattform.Models;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Sammlerplattform.Controllers.PictureAnaylsis
{
    public partial class WordCategorization
    {
        public static void CategorizeWord(WordCategorizationModel wordCategorizationModel, IWebHostEnvironment _hostEnvironment)
        {
            string germanWordlist = Path.Combine(Path.Combine(_hostEnvironment.WebRootPath, "dic"), "wordlist-german.txt");
            List<string> germanWords = [.. File.ReadAllLines(germanWordlist)];
            string buildingWordlist = Path.Combine(Path.Combine(_hostEnvironment.WebRootPath, "dic"), "Gebäude_WhiteList.txt");
            List<string> buildingsList = [.. File.ReadAllLines(buildingWordlist)];
            wordCategorizationModel.Buildings = buildingsList;
            string occasionWordlist = Path.Combine(Path.Combine(_hostEnvironment.WebRootPath, "dic"), "Anlässe_WhiteList.txt");
            List<string> occasion = [.. File.ReadAllLines(occasionWordlist)];
            wordCategorizationModel.OccasionKeywordList = occasion;
            string surnameWordlist = Path.Combine(Path.Combine(_hostEnvironment.WebRootPath, "dic"), "Nachnamen_WhiteList.txt");
            List<string> surnameList = [.. File.ReadAllLines(surnameWordlist)];
            string forenameWordlist = Path.Combine(Path.Combine(_hostEnvironment.WebRootPath, "dic"), "Vornamen_WhiteList.txt");
            List<string> forenameList = [.. File.ReadAllLines(forenameWordlist)];
            string cityWordlist = Path.Combine(Path.Combine(_hostEnvironment.WebRootPath, "dic"), "Orte_WhiteList.txt");
            List<string> cityList = [.. File.ReadAllLines(cityWordlist)];
            List<string> wordsToCheck = germanWords.Union(buildingsList).Union(surnameList).Union(cityList).ToList();
            wordCategorizationModel.CultureInfo = new CultureInfo("de-DE");
            string? currentBuilding = string.Empty;
            List<string> wordInBuildingList = [];
            List<int> entryToDeleteList = [];
            List<int> newBlockList = [];

            for (int idx = 0; idx < wordCategorizationModel.WordBlockCategorization.Count; idx++)
            {
                (string Word, int Block, int Position, List<(double Propability, string CategoryName, string? CategorizedTo, string CategorizedWhere)> Category, bool Frontside) wbc = wordCategorizationModel.WordBlockCategorization[idx];
                string currentWord = wbc.Word;
                int wordLength = currentWord.Length;
                double weightForename = 0;
                double weightSurname = 0;

                if (wbc.Category.Exists(x => x.CategoryName.Equals("Building")))
                {
                    (double Propability, string CategoryName, string? CategorizedTo, string CategorizedWhere) = wbc.Category.Where(x => x.CategoryName.Equals("Building")).First();
                    if (currentBuilding != CategorizedTo)
                    {
                        wordInBuildingList =
                        [
                            wbc.Word
                        ];
                        currentBuilding = CategorizedTo;
                    }
                    else
                    {
                        bool wordIsEqual = false;
                        // following Test is necessary, because "Metzerei Kühnle" can appear twice
                        if (wordInBuildingList.Contains(wbc.Word))
                        {
                            entryToDeleteList.Add(idx);
                        }
                        else
                        {
                            foreach (string word in wordInBuildingList)
                            {
                                double tet = LevenshteinDistance(word, wbc.Word);
                                double test = LevenshteinDistance(word, wbc.Word) / (double)wbc.Word.Length;
                                if (LevenshteinDistance(word, wbc.Word) / (double)wbc.Word.Length <= 0.5)
                                {
                                    //Wörter sollten nicht zu ähnlich sein
                                    wordIsEqual = true;
                                    break;
                                }
                            }

                            if (wordIsEqual)
                            {
                                entryToDeleteList.Add(idx);
                            }
                            else
                            {
                                wordInBuildingList.Add(wbc.Word);
                            }
                        }
                    }

                    double prop = GuessForSurname(wbc.Word, idx, _hostEnvironment, wordCategorizationModel, 1);
                    if (prop > 0)
                    {
                        wordCategorizationModel.WordBlockCategorization[idx].Category.Add((Propability * prop, "Surname", string.Empty, "CategorizeWord"));
                    }

                    prop = GuessForForename(wbc.Word, idx, _hostEnvironment, wordCategorizationModel, 1);
                    if (prop > 0)
                    {
                        wordCategorizationModel.WordBlockCategorization[idx].Category.Add((Propability * prop, "Forename", string.Empty, "CategorizeWord"));
                    }
                }
                else if (RegexNumber().IsMatch(currentWord))
                {
                    MatchCollection dateCollection = RegexDate().Matches(currentWord);
                    foreach (object date in dateCollection)
                    {
                        wordCategorizationModel.WordBlockCategorization[idx].Category.Add((1, "Date", date.ToString(), "CategorizeWord"));
                        newBlockList.Add(idx);
                    }
                    MatchCollection yearCollection = RegexYear().Matches(currentWord);
                    foreach (object year in yearCollection)
                    {
                        wordCategorizationModel.WordBlockCategorization[idx].Category.Add((1, "Year", year.ToString(), "CategorizeWord"));
                    }

                    if (yearCollection.Count == 0 && dateCollection.Count == 0)
                    {
                        wordCategorizationModel.WordBlockCategorization[idx].Category.Add((1, "Number", string.Empty, "CategorizeWord"));
                        if (idx > 0 && wordCategorizationModel.WordBlockCategorization[idx - 1].Category.Exists(x => x.CategoryName.Equals("Straßenzug")))
                        {
                            wordCategorizationModel.WordBlockCategorization[idx].Category.Add((1, "Address", string.Empty, "CategorizeWord"));
                        }
                    }

                    string? isCurrentBlockPostmark = (from x in wordCategorizationModel.WordBlockCategorization
                                                      where x.Block.Equals(wordCategorizationModel.WordBlockCategorization[idx].Block)
                                                      && x.Category.Exists(x => x.CategoryName.Equals("Postmark"))
                                                      select x.Word).FirstOrDefault();
                    if (isCurrentBlockPostmark != null)
                    {
                        wordCategorizationModel.WordBlockCategorization[idx].Category.Add((0.3, "Postmark", string.Empty, "CategorizeWord"));
                    }

                    string? isCurrentBlockStamp = (from x in wordCategorizationModel.WordBlockCategorization
                                                   where x.Block.Equals(wordCategorizationModel.WordBlockCategorization[idx].Block)
                                                   && x.Category.Exists(x => x.CategoryName.Equals("Stamp"))
                                                   select x.Word).FirstOrDefault();
                    if (isCurrentBlockStamp != null)
                    {
                        wordCategorizationModel.WordBlockCategorization[idx].Category.Add((0.3, "Stamp", string.Empty, "CategorizeWord"));
                    }
                }
                else
                {
                    if (wordLength == 1 && !RegexSpecialCharacter().IsMatch(currentWord))
                    {
                        // FIXIT too specific
                        if (idx - 1 >= 0 && wordCategorizationModel.WordBlockCategorization[idx - 1].Category.Exists(x => x.CategoryName.Equals("Building")) && currentWord.Equals("z"))
                        {
                            wordCategorizationModel.WordBlockCategorization[idx - 1].Category.Add((1.5, "Building", currentWord, "CategorizeWord"));
                        }
                        string? isCurrentBlockBuilding = (from x in wordCategorizationModel.WordBlockCategorization
                                                          where x.Block.Equals(wordCategorizationModel.WordBlockCategorization[idx].Block)
                                                          && x.Category.Exists(x => x.CategoryName.Equals("Building"))
                                                          select x.Word).FirstOrDefault();
                        if (isCurrentBlockBuilding != null && currentWord.Equals("v"))
                        {
                            wordCategorizationModel.WordBlockCategorization[idx].Category.Add((1, "Building", currentWord, "CategorizeWord"));
                        }
                    }
                    else if (wordLength > 1)
                    {
                        currentWord = RemoveAllSpecialCharacters(currentWord);
                        if (string.IsNullOrEmpty(currentWord))
                        {
                            continue;
                        }

                        currentWord = ChangeCapitalLetters(currentWord);

                        if (CheckIfWordIsCategorized(currentWord, idx, wordCategorizationModel, 1))
                        {
                            continue;
                        }

                        if (CheckCapitalLetterAtBeginning(currentWord))
                        {
                            //await führt immer zu Sprüngen
                            _ = CheckWikipedia(currentWord, idx, wordCategorizationModel, 1);
                            Console.WriteLine("Wartet");

                            weightForename = GuessForForename(currentWord, idx, _hostEnvironment, wordCategorizationModel, 1);
                            weightSurname = GuessForSurname(currentWord, idx, _hostEnvironment, wordCategorizationModel, 1);
                            _ = GuessForAddress(currentWord, idx, wordCategorizationModel, 1);
                        }

                        _ = GuessForCity(currentWord, idx, wordCategorizationModel, _hostEnvironment, 1);
                        //GuessForGeography();
                        _ = GuessForPublisher(currentWord, idx, wordCategorizationModel, 1, weightForename + weightSurname);
                        _ = GuessForAnlass(currentWord, idx, wordCategorizationModel, 1);

                        if (wbc.Frontside)
                        {
                            _ = weightForename + weightSurname > 0.5
                                ? GuessForBuilding(currentWord, idx, wordCategorizationModel, weightForename + weightSurname)
                                : GuessForBuilding(currentWord, idx, wordCategorizationModel, 1);
                        }

                        _ = GuessForAuthorArtist(currentWord, idx, wordCategorizationModel, 1, weightForename + weightSurname);

                        if (wbc.Category.Count == 0 || wbc.Category.MaxBy(x => x.Propability).Propability < 1)
                        {
                            // Falls noch nichts passendes gefunden wurde, dann Prüfung auf Rechtschreibfehler
                            if (wordsToCheck.Where(a => a.Equals(currentWord, StringComparison.CurrentCultureIgnoreCase)).Any())
                            {
                                wordCategorizationModel.WordBlockCategorization[idx].Category.Add((0.3 + (0.1 * wordCategorizationModel.MaxWordsInBlock[wbc.Block]), "Text", string.Empty, "CategorizeWord"));
                            }
                            else
                            {
                                List<(string rightWord, double probability)> resultSpellCheck = ListSpellChecker.ListSpellChecker.WordProbabilityTupleList(currentWord, wordsToCheck, wordCategorizationModel.CultureInfo);

                                if (resultSpellCheck.Count > 0)
                                {
                                    double highestPropability = resultSpellCheck[0].probability;
                                    foreach ((string rightWord, double probability) in resultSpellCheck)
                                    {
                                        currentWord = rightWord;
                                        if (probability < highestPropability)
                                        {
                                            break;
                                        }

                                        double weightWordIsText = (0.2 * probability) + (0.1 * wordCategorizationModel.MaxWordsInBlock[wbc.Block]);
                                        wordCategorizationModel.WordBlockCategorization[idx].Category.Add((weightWordIsText, "Text", currentWord, "CategorizeWord"));

                                        if (CheckIfWordIsCategorized(currentWord, idx, wordCategorizationModel, probability))
                                        {
                                            break;
                                        }

                                        List<double> listOfProp = [];

                                        if (CheckCapitalLetterAtBeginning(currentWord))
                                        {
                                            _ = CheckWikipedia(currentWord, idx, wordCategorizationModel, probability);
                                            Console.WriteLine("Wartet");

                                            listOfProp.Add(GuessForForename(currentWord, idx, _hostEnvironment, wordCategorizationModel, probability));
                                            listOfProp.Add(GuessForSurname(currentWord, idx, _hostEnvironment, wordCategorizationModel, probability));
                                            listOfProp.Add(GuessForAddress(currentWord, idx, wordCategorizationModel, probability));
                                        }

                                        listOfProp.Add(GuessForCity(currentWord, idx, wordCategorizationModel, _hostEnvironment, probability));
                                        //listOfProp.Add(GuessForGeography(currentWord, idx, wordCategorizationModel, 1));
                                        listOfProp.Add(GuessForAnlass(currentWord, idx, wordCategorizationModel, 1));
                                        if (wbc.Frontside)
                                        {
                                            listOfProp.Add(GuessForBuilding(currentWord, idx, wordCategorizationModel, probability * (weightForename + weightSurname)));
                                        }

                                        listOfProp.Add(GuessForAuthorArtist(currentWord, idx, wordCategorizationModel, probability, weightForename + weightSurname));

                                        if (listOfProp.Max() > 0.7)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            for (int i = entryToDeleteList.Count - 1; i >= 0; i--)
            {
                wordCategorizationModel.WordBlockCategorization.RemoveAt(entryToDeleteList[i]);
            }

            foreach (int index in newBlockList)
            {
                (string Word, int Block, int Position, List<(double Weight, string CategoryName, string? CategorizedTo, string CategorizedWhere)> Category, bool Frontside) currentword = wordCategorizationModel.WordBlockCategorization[index];
                currentword.Block = wordCategorizationModel.WordBlockCategorization.MaxBy(x => x.Block).Block + 1;
                wordCategorizationModel.WordBlockCategorization[index] = currentword;
            }

            return;
        }

        public static bool CheckIfWordIsCategorized(string word, int idx, WordCategorizationModel model, double propability)
        {
            bool isCategorized = false;

            if (model.Cities.Contains(word))
            {
                model.WordBlockCategorization[idx].Category.Add((1.5 * propability, "City", model.Cities.Where(x => x.Contains(word)).First(), "CheckIfWordIsCategorized"));
                model.WordBlockCategorization[idx].Category.Add((0.4 * propability, "Publisher", model.Cities.Where(x => x.Contains(word)).First(), "CheckIfWordIsCategorized"));
                model.WordBlockCategorization[idx].Category.Add((0.4 * propability, "AuthorArtist", model.Cities.Where(x => x.Contains(word)).First(), "CheckIfWordIsCategorized"));
                isCategorized = true;
            }
            else if (model.Publishers.Contains(word))
            {
                model.WordBlockCategorization[idx].Category
                    .Add((1.5 * propability, "Publisher", model.Publishers.Where(x => x.Contains(word)).First(), "CheckIfWordIsCategorized"));
                isCategorized = true;
            }
            else if (model.AuthorArtists.Contains(word))
            {
                model.WordBlockCategorization[idx].Category
                    .Add((1.5 * propability, "AuthorArtist", model.AuthorArtists.Where(x => x.Contains(word)).First(), "CheckIfWordIsCategorized"));
                isCategorized = true;
            }
            else if (PostcardCategorizationByOCR(word, idx, model, propability))
            {
                isCategorized = true;
            }

            return isCategorized;
        }

        public static async Task<double> CheckWikipedia(string searchstring, int idx, WordCategorizationModel model, double outsideWeight)
        {
            double weight = 0;

            HttpClient httpClient = new();
            HttpResponseMessage response = new();
            string url1 = "https://de.wikipedia.org/w/api.php?action=query&generator=search&format=json&prop=categories&gsrlimit=1&gsrsearch=%27%22" + searchstring + "%22%27";
            string url2 = "https://de.wikipedia.org/w/api.php?action=query&generator=search&format=json&prop=categories&cllimit=40&gsrlimit=10&gsrsearch='" + searchstring + "'";

            response = await httpClient.GetAsync(url1);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            Rootobject? jsonWiki = JsonSerializer.Deserialize<Rootobject>(jsonResponse);
            if (jsonWiki != null && jsonWiki.query != null)
            {
                foreach (KeyValuePair<string, pageval> item in jsonWiki.query.pages)
                {
                    string currentKey = item.Key;
                    string[] splittedTitle = jsonWiki.query.pages[currentKey].title.Split(" ");
                    string ersterTeil = splittedTitle[0];
                    if (jsonWiki.query.pages[currentKey].title.Contains(searchstring) && ersterTeil.Length <= searchstring.Length * 1.2)
                    {
                        Category[]? categories = jsonWiki.query.pages[currentKey].categories;
                        if (categories != null)
                        {
                            foreach (Category category in categories)
                            {
                                if (model.Cities.Contains(jsonWiki.query.pages[currentKey].title))
                                {
                                    model.WordBlockCategorization[idx].Category.Add((1, "City", model.Cities.Where(x => x.Contains(jsonWiki.query.pages[currentKey].title)).First(), "CheckWikipedia"));
                                }
                                else if (category.title.Contains("Bauwerk", StringComparison.OrdinalIgnoreCase) || category.title.Contains("Garten", StringComparison.OrdinalIgnoreCase)
                                    || category.title.Contains("Denkmal", StringComparison.OrdinalIgnoreCase) || category.title.Contains("Museum", StringComparison.OrdinalIgnoreCase)
                                    || category.title.Contains("Kulturgut", StringComparison.OrdinalIgnoreCase))
                                {
                                    model.WordBlockCategorization[idx].Category.Add((0.8 * outsideWeight, "Building", string.Empty, "CheckWikipedia"));
                                    weight = 0.8 * outsideWeight;
                                    break;
                                }
                                else if (category.title.Contains("Flurname") || category.title.Contains("Berg", StringComparison.OrdinalIgnoreCase))
                                {
                                    model.WordBlockCategorization[idx].Category.Add((0.8 * outsideWeight, "Geography", string.Empty, "CheckWikipedia"));
                                    weight = 0.8 * outsideWeight;
                                    break;
                                }
                                if (category.title.Contains("Maler") || category.title.Contains("Grafiker"))
                                {
                                    model.WordBlockCategorization[idx].Category.Add((0.8 * outsideWeight, "AuthorArtist", string.Empty, "CheckWikipedia"));
                                    weight = 0.8 * outsideWeight;
                                    break;
                                }
                                if (category.title.Contains("Verlag", StringComparison.OrdinalIgnoreCase))
                                {
                                    model.WordBlockCategorization[idx].Category.Add((0.8 * outsideWeight, "Publisher", string.Empty, "CheckWikipedia"));
                                    weight = 0.8 * outsideWeight;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return weight;
        }

        public static async Task WordSemantic(IWebHostEnvironment _hostEnvironment)
        {
            //Hier nicht nützlich, vielleicht Rückseite
            Catalyst.Models.German.Register();

            Storage.Current = new DiskStorage("catalyst-models");
            Pipeline nlp = await Pipeline.ForAsync(Language.German);
            Document doc = new("M. Raible Inh. Josef Raible, Ellwangen-Jagst am Schönenberg", Language.German);
            _ = nlp.ProcessSingle(doc);

            string dir = Path.Combine(_hostEnvironment.WebRootPath, "DetectedTexts_GoogleVision");
            string file = Path.Combine(dir, "wordSemantic.txt");
            StreamWriter stream = new(file, true);
            stream.Write(doc.ToJson());
            stream.Close();

            return;
        }

        public static double GuessForCity(string word, int idx, WordCategorizationModel model, IWebHostEnvironment _hostEnvironment, double outsideWeight)
        {
            double weight = 0.0;

            string OrteEndelist = Path.Combine(Path.Combine(_hostEnvironment.WebRootPath, "dic"), "Orte_Ende_result.txt");
            List<string> stichworteOrtEnde = [.. File.ReadAllLines(OrteEndelist)];
            string OrteAnfangList = Path.Combine(Path.Combine(_hostEnvironment.WebRootPath, "dic"), "Orte_Beginn_result.txt");
            List<string> stichworteOrtBeginn = [.. File.ReadAllLines(OrteAnfangList)];

            if (word.Equals("Gruß") || word.Equals("Gruss"))
            {
                //'Nicht das Wort "aus" sollte so stark gewichtet werden, sondern der Block
                if (idx + 2 <= model.WordBlockCategorization.Count)
                {
                    model.WordBlockCategorization[idx + 2].Category.Add((0.5 * outsideWeight, "City", word, "GuessForCity"));
                }
            }
            else if (word.Equals("aus", StringComparison.OrdinalIgnoreCase))
            {
                //'Nicht das Wort "aus" sollte so stark gewichtet werden, sondern der Block
                if (idx + 1 <= model.WordBlockCategorization.Count)
                {
                    model.WordBlockCategorization[idx + 1].Category.Add((0.5 * outsideWeight, "City", word, "GuessForCity"));
                }
            }
            else if (CheckCapitalLetterAtBeginning(word))
            {
                string Beginn = string.Empty;
                string Ende = string.Empty;
                foreach (string wortEnde in stichworteOrtEnde)
                {
                    string escapedSymbol = Regex.Escape(wortEnde);
                    if (Regex.IsMatch(word, @"(" + escapedSymbol + @")\b"))
                    {
                        if (wortEnde.Length != word.Length)
                        {
                            Ende = wortEnde;
                            weight += wortEnde.Length / (double)word.Length;
                        }
                        break;
                    }
                }
                string last = stichworteOrtBeginn.Last();
                foreach (string wortTeil in stichworteOrtBeginn)
                {
                    if (word.Contains(wortTeil))
                    {
                        if (wortTeil.Length != word.Length)
                        {
                            Beginn = wortTeil;
                            weight += wortTeil.Length / (double)word.Length / 2;
                        }
                        break;
                    }
                }

                if (weight > 0)
                {
                    string wordWithoutCapital = RemoveAllSpecialCharacters(word);
                    double wordHeightRank = model.WordHeight.FindIndex(x => x.word.Equals(wordWithoutCapital)) + 1;
                    if (wordHeightRank > 0)
                    {
                        weight += 1 / wordHeightRank * 0.2;
                    }

                    if (model.WordBlockCategorization[idx].Frontside)
                    {
                        double wordPositionRank = model.WordPosComparedToCenterOfImg.FindIndex(x => x.word.Equals(wordWithoutCapital)) + 1;
                        if (wordPositionRank > 0)
                        {
                            weight += 1 / wordPositionRank * 0.2;
                        }
                    }

                    if (CheckCapitalLetterAtBeginning(word))
                    {
                        weight += 0.15;
                    }

                    if (RegexMale().IsMatch(word) || RegexNeuter().IsMatch(word) || RegexFemale().IsMatch(word))
                    {
                        weight -= 0.1;
                    }

                    string? isCurrentBlockAA = (from x in model.WordBlockCategorization
                                                where x.Block.Equals(model.WordBlockCategorization[idx].Block)
                                                && x.Category.Exists(x => x.CategoryName.Equals("AuthorArtist"))
                                                select x.Word).FirstOrDefault();
                    if (isCurrentBlockAA != null)
                    {
                        model.WordBlockCategorization[idx].Category.Add((0.3 * outsideWeight, "AuthorArtist", word, "GuessForCity"));
                    }
                    string? isCurrentBlockPublisher = (from x in model.WordBlockCategorization
                                                       where x.Block.Equals(model.WordBlockCategorization[idx].Block)
                                                       && x.Category.Exists(x => x.CategoryName.Equals("Publisher"))
                                                       select x.Word).FirstOrDefault();
                    if (isCurrentBlockPublisher != null)
                    {
                        model.WordBlockCategorization[idx].Category.Add((0.3 * outsideWeight, "Publisher", word, "GuessForCity"));
                    }
                }
                if (weight >= 1)
                {
                    weight = 0.95;
                }

                if (weight * outsideWeight > 0.2)
                {
                    model.WordBlockCategorization[idx].Category.Add((weight * outsideWeight, "City", word, "GuessForCity"));
                }
            }

            return weight;
        }

        public static double GuessForBuilding(string word, int idx, WordCategorizationModel model, double outsideWeight)
        {
            double weight = 0;

            if (model.Buildings.Contains(word, StringComparer.OrdinalIgnoreCase) || model.Buildings.Any(word.Contains))
            {
                model.WordBlockCategorization[idx].Category
                    .Add((1 * outsideWeight, "Building", word, "GuessForBuilding"));
            }

            if (idx - 1 >= 0 && model.WordBlockCategorization[idx - 1].Category.Exists(x => x.CategoryName.Equals("Building")) && (word.Contains("z.") || word.Contains("zu")))
            {
                model.WordBlockCategorization[idx - 1].Category.Add((1.5 * outsideWeight, "Building", word, "GuessForBuilding"));
                weight = 1.5;
            }

            string? isCurrentBlockBuilding = (from x in model.WordBlockCategorization
                                              where x.Block.Equals(model.WordBlockCategorization[idx].Block)
                                              && x.Category.Exists(x => x.CategoryName.Equals("Building"))
                                              select x.Word).FirstOrDefault();
            if (isCurrentBlockBuilding != null && (word.Contains("v.") || word.Contains("von")))
            {
                model.WordBlockCategorization[idx].Category.Add((1, "Building", word, "GuessForBuilding"));
                weight = 1.5;
            }

            return weight;
        }

        public static double GuessForPublisher(string word, int idx, WordCategorizationModel model, double outsideWeight, double propName)
        {
            double weight = (1.5 * outsideWeight) + (propName * 0.5);

            if (word.Contains("Verlag", StringComparison.OrdinalIgnoreCase))
            {
                model.WordBlockCategorization[idx].Category.Add((weight, "Publisher", word, "GuessForPublisher"));
            }

            return weight;
        }

        public static double GuessForAddress(string word, int idx, WordCategorizationModel model, double outsideWeight)
        {
            double weight = outsideWeight;

            if (word.Equals("Herr") || word.Equals("Frau") || word.Equals("Fräulein"))
            {
                model.WordBlockCategorization[idx].Category.Add((1.5 * weight, "Address", word, "GuessForAddress"));
            }

            return weight;
        }

        public static double GuessForSurname(string surNameWeight, int idx, IWebHostEnvironment _hostEnvironment, WordCategorizationModel model, double outsideWeight)
        {
            double weight = 0.0;

            string surnameList = Path.Combine(Path.Combine(_hostEnvironment.WebRootPath, "dic"), "Nachnamen_WhiteList.txt");
            List<string> nachnamen = [.. File.ReadAllLines(surnameList)];
            string surnameEndingList = Path.Combine(Path.Combine(_hostEnvironment.WebRootPath, "dic"), "Nachnamen_Ende_result.txt");
            List<string> keywordsSurnameEnding = [.. File.ReadAllLines(surnameEndingList)];
            string surnameBeginnList = Path.Combine(Path.Combine(_hostEnvironment.WebRootPath, "dic"), "Nachnamen_Beginn_result.txt");
            List<string> keywordSurnameBegin = [.. File.ReadAllLines(surnameBeginnList)];

            if (nachnamen.Any(x => x.Equals(surNameWeight)))
            {
                model.WordBlockCategorization[idx].Category.Add((1 * outsideWeight, "Surname", surNameWeight, "GuessForSurname"));
                weight = 1;

                string? isCurrentBlockAddress = (from x in model.WordBlockCategorization
                                                 where x.Block.Equals(model.WordBlockCategorization[idx].Block)
                                                 && x.Category.Exists(x => x.CategoryName.Equals("Address"))
                                                     && !x.Frontside
                                                 select x.Word).FirstOrDefault();
                if (isCurrentBlockAddress != null)
                {
                    model.WordBlockCategorization[idx].Category.Add((0.5, "Address", surNameWeight, "GuessForSurname"));
                }

            }
            else
            {
                foreach (string keyword in keywordsSurnameEnding)
                {
                    string escapedSymbol = Regex.Escape(keyword);
                    if (Regex.IsMatch(surNameWeight, @"(" + escapedSymbol + @")\b"))
                    {
                        if (keyword.Length != surNameWeight.Length)
                        {
                            weight += keyword.Length / (double)surNameWeight.Length;
                        }

                        break;
                    }
                }
                foreach (string stichwort in keywordSurnameBegin)
                {
                    if (surNameWeight.Contains(stichwort))
                    {
                        if (stichwort.Length != surNameWeight.Length)
                        {
                            weight += stichwort.Length / (double)surNameWeight.Length / 2;
                        }

                        break;
                    }
                }

                if (weight > 0)
                {
                    model.WordBlockCategorization[idx].Category.Add((weight * outsideWeight, "Surname", surNameWeight, "GuessForSurname"));

                    string? isCurrentBlockAddress = (from x in model.WordBlockCategorization
                                                     where x.Block.Equals(model.WordBlockCategorization[idx].Block)
                                                     && x.Category.Exists(x => x.CategoryName.Equals("Address"))
                                                     && !x.Frontside
                                                     select x.Word).FirstOrDefault();
                    if (isCurrentBlockAddress != null)
                    {
                        model.WordBlockCategorization[idx].Category.Add((0.5, "Address", surNameWeight, "GuessForSurname"));
                    }
                }
                if (weight >= 1)
                {
                    weight = 0.95;
                }
            }
            return weight;
        }
        public static double GuessForForename(string probForeName, int idx, IWebHostEnvironment _hostEnvironment, WordCategorizationModel model, double outsidePropability)
        {
            double propability = 0.0;

            string vornamenList = Path.Combine(Path.Combine(_hostEnvironment.WebRootPath, "dic"), "Vornamen_WhiteList.txt");
            List<string> vornamen = [.. File.ReadAllLines(vornamenList)];
            string vornamenEndelist = Path.Combine(Path.Combine(_hostEnvironment.WebRootPath, "dic"), "Vornamen_Ende_result.txt");
            List<string> stichworteVornameEnde = [.. File.ReadAllLines(vornamenEndelist)];
            string vornamenBeginnList = Path.Combine(Path.Combine(_hostEnvironment.WebRootPath, "dic"), "Vornamen_Beginn_result.txt");
            List<string> stichworteVornameBeginn = [.. File.ReadAllLines(vornamenBeginnList)];

            if (vornamen.Contains(probForeName))
            {
                model.WordBlockCategorization[idx].Category.Add((1 * outsidePropability, "Forename", probForeName, "GuessForForename"));
            }
            else
            {
                foreach (string stichwort in stichworteVornameEnde)
                {
                    string escapedSymbol = Regex.Escape(stichwort);
                    if (Regex.IsMatch(probForeName, @"(" + escapedSymbol + @")\b"))
                    {
                        if (stichwort.Length != probForeName.Length)
                        {
                            propability += stichwort.Length / (double)probForeName.Length;
                        }

                        break;
                    }
                }
                foreach (string stichwort in stichworteVornameBeginn)
                {
                    if (probForeName.Contains(stichwort))
                    {
                        if (stichwort.Length != probForeName.Length)
                        {
                            propability += stichwort.Length / (double)probForeName.Length / 2;
                        }

                        break;
                    }
                }

                if (propability > 0)
                {
                    model.WordBlockCategorization[idx].Category.Add((propability * outsidePropability, "Forename", probForeName, "GuessForForename"));
                }

                if (propability >= 1)
                {
                    propability = 0.95;
                }
            }
            return propability;
        }

        public static double GuessForGeography()
        {
            double probability = 0;

            //if (model.LandscapeStichworte.Contains(word) || model.LandscapeStichworte.Any(word.Contains))
            //{
            //    probability = 1 + outsideProbability;
            //    model.WordBlockCategorization[idx].Category.Add((probability, "Landscape", word, "GuessForLandscape"));

            //    var isCurrentBlockPublisher = (from x in model.WordBlockCategorization
            //                                   where x.Block.Equals(model.WordBlockCategorization[idx].Block)
            //                                   && x.Category.Exists(x => x.CategoryName.Equals("City"))
            //                                   select x.Word).FirstOrDefault();
            //    if (isCurrentBlockPublisher != null)
            //    {
            //        model.WordBlockCategorization[idx].Category.Add((0.3 * outsideProbability, "City", word, "GuessForLandscape"));
            //    }
            //}

            return probability;
        }

        public static double GuessForAuthorArtist(string word, int idx, WordCategorizationModel model, double outsidePropability, double propName)
        {
            double propability = propName * 0.5; //Zwar ´hat ein Künstler ienen Nachnamen, aber nicht jeder NAchname ist ein Künstler

            if (propability > 0)
            {
                string wordWOSpecial = RemoveAllSpecialCharacters(word);
                double wordHeightRank = model.WordHeight.FindIndex(x => x.word.Equals(wordWOSpecial)) + 1;
                if (wordHeightRank > 0)
                {
                    propability += (1 - (1 / wordHeightRank)) * 0.2;
                }

                if (CheckCapitalLetterAtBeginning(word))
                {
                    propability += 0.15;
                }

                if (!model.Frontside)
                {
                    propability -= 0.2;
                }

                string? isCurrentBlockCity = (from x in model.WordBlockCategorization
                                              where x.Block.Equals(model.WordBlockCategorization[idx].Block)
                                              && x.Category.Exists(x => x.CategoryName.Equals("City"))
                                              select x.Word).FirstOrDefault();
                if (isCurrentBlockCity != null)
                {
                    propability += 0.5;
                }

                if (propability * outsidePropability > 0.17)
                {
                    model.WordBlockCategorization[idx].Category.Add((propability * outsidePropability, "AuthorArtist", word, "GuessForAuthorArtist"));
                }
            }
            if (propability >= 1)
            {
                propability = 0.95;
            }

            if (model.AuthorArtistKeywordList.Contains(word) || model.AuthorArtistKeywordList.Any(word.Contains))
            {
                model.WordBlockCategorization[idx].Category.Add(((0.8 * outsidePropability) + propability, "AuthorArtist", word, "GuessForAuthorArtist"));
            }

            return propability;
        }

        public static double GuessForAnlass(string word, int idx, WordCategorizationModel model, double outsidePropability)
        {
            double propability = 0;

            if (model.OccasionKeywordList.Contains(word) || model.OccasionKeywordList.Any(word.Contains))
            {
                model.WordBlockCategorization[idx].Category.Add((1.5 * outsidePropability, "Occasion", word, "GuessForAnlass"));
                propability = 0.8 * outsidePropability;
            }

            return propability;
        }

        public static string RemoveAllSpecialCharacters(string word)
        {
            //Sonderzeichen entfernen, außer Punkt
            word = RegexSpecialCharacter().Replace(word, string.Empty);

            return word;
        }

        public static bool CheckCapitalLetterAtBeginning(string word)
        {
            bool isCapital = false;

            if (RegexChapitalLetterAtBegining().IsMatch(word))
            {
                isCapital = true;
            }

            return isCapital;
        }

        public static void CheckWordSize(WordCategorizationModel model)
        {
            List<(string word, int height)> value = [];
            List<(string word, int height)> resultSizeCheck = value;

            foreach ((string word, List<int> X, List<int> Y, bool Frontside) in model.WordsWithXYPositions)
            {
                resultSizeCheck.Add((word, (Y[3] - Y[0] + Y[2] - Y[1]) / 2));
            }

            model.WordHeight = [.. resultSizeCheck.OrderByDescending(x => x.height)];

            return;
        }

        public static void CheckWordPosComparedToCenterOfImg(WordCategorizationModel model)
        {
            List<(string word, int distanceToCenterX, int distanceToCenterY)> resultSizeCheck = [];
            int centerX = model.ImageWidth / 2, centerY = model.ImageHeight / 2;

            foreach ((string word, List<int> X, List<int> Y, bool Frontside) in model.WordsWithXYPositions)
            {
                int posX = (X[0] + X[1] + X[2] + X[3]) / 4;
                int posY = (Y[0] + Y[1] + Y[2] + Y[3]) / 4;
                resultSizeCheck.Add((word, Math.Abs(centerX - posX), Math.Abs(centerY - posY)));
            }

            model.WordPosComparedToCenterOfImg = [.. resultSizeCheck.OrderBy(x => x.distanceToCenterY).ThenBy(y => y.distanceToCenterX)];

            return;
        }

        public static string ChangeCapitalLetters(string word)
        {
            List<int> posUpperChars = (from ch in word.ToArray()
                                       where char.IsUpper(ch)
                                       select word.IndexOf(ch)).ToList();
            if (posUpperChars.Count == word.Length)
            {
                //Falls alles groß geschrieben, dann nur erster Buchstabe so lassen
                word = word[0] + word.ToLower()[1..];
            }

            return word;
        }

        public static int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.IsNullOrEmpty(t) ? 0 : t.Length;
            }

            if (string.IsNullOrEmpty(t))
            {
                return s.Length;
            }

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // initialize the top and right of the table to 0, 1, 2, ...
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
                ;
            }

            for (int j = 1; j <= m; d[0, j] = j++)
            {
                ;
            }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = t[j - 1] == s[i - 1] ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            return d[n, m];
        }

        public static bool PostcardCategorizationByOCR(string word, int idx, WordCategorizationModel model, double propability)
        {
            bool isCategorized = false;

            if (word.Contains("Absolvia", StringComparison.OrdinalIgnoreCase))
            {
                model.PostcardCategories.Add(word); model.WordBlockCategorization[idx].Category.Add((1 * propability, "Absolvia", word, "PostcardCategorizationByOCR"));
                isCategorized = true;
            }

            if (word.Equals("Vogelschau") || word.Equals("Umgebung"))
            {
                model.PostcardCategories.Add(word); model.WordBlockCategorization[idx].Category.Add((1 * propability, "Luftbild", word, "PostcardCategorizationByOCR"));
                isCategorized = true;
            }

            if (word.Contains("strasse", StringComparison.OrdinalIgnoreCase) || word.Contains("Straße", StringComparison.OrdinalIgnoreCase) || word.Contains("Str", StringComparison.OrdinalIgnoreCase))
            {
                model.PostcardCategories.Add(word); model.WordBlockCategorization[idx].Category.Add((1 * propability, "Straßenzug", word, "PostcardCategorizationByOCR"));
                model.PostcardCategories.Add(word); model.WordBlockCategorization[idx].Category.Add((0.8 * propability, "Address", word, "PostcardCategorizationByOCR"));
            }

            return isCategorized;
        }

        [GeneratedRegex(@"\d")]
        private static partial Regex RegexNumber();
        [GeneratedRegex(@"\b(\d{1,2})\.(\d{1,2})\.(\d{2,4})\b")]
        private static partial Regex RegexDate();
        [GeneratedRegex(@"[A-z]es\b")]
        private static partial Regex RegexNeuter();
        [GeneratedRegex(@"\b(?:16|17|18|19|20)\d{2}\b")]
        private static partial Regex RegexYear();
        [GeneratedRegex(@"[A-z]er\b")]
        private static partial Regex RegexMale();
        [GeneratedRegex(@"[A-z]e\b")]
        private static partial Regex RegexFemale();
        [GeneratedRegex(@"[<>'/;`%+{}\[\]‘\\°_*,?!():=.-]")]
        private static partial Regex RegexSpecialCharacter();
        [GeneratedRegex(@"\b[A-ZÄÖÜ][a-zäöü]+")]
        private static partial Regex RegexChapitalLetterAtBegining();
    }
}
