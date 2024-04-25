using System.Text.RegularExpressions;

namespace Sammlerplattform.Controllers.GenericClasses
{
    public static class WordPattern
    {
        public static Dictionary<string, int> WordSimilartyEndOfWord(List<string> wordsTocheck, int minimum)
        {
            DateTime timer1 = new();
            DateTime timer2 = new();
            List<TimeSpan> timeDifference = [];
            int index = 1;
            Dictionary<string, int> wordSimilarities = [];

            foreach (string word in wordsTocheck)
            {
                index++;
                string[] teile = word.Split(" ");
                //foreach (var teil in teile)
                //{
                string teil = teile[0];
                timer1 = DateTime.Now;
                char[] wordSplits = teil.ToCharArray();
                List<string> lastWordParts = [];
                if (teil.Length <= minimum)
                {
                    lastWordParts.Add(teil);
                }
                else
                {
                    for (int j = wordSplits.Length - minimum; j > -1; j--)
                    {
                        if (lastWordParts.Count > 0)
                        {
                            lastWordParts.Add(wordSplits[j] + lastWordParts.Last());
                        }
                        else
                        {
                            string start = string.Empty;
                            for (int k = 0; k < minimum; k++)
                            {
                                start += wordSplits[j + k].ToString();
                            }
                            lastWordParts.Add(start);
                        }
                    }
                }

                for (int i = index; i < wordsTocheck.Count; i++)
                {
                    string currentWord = wordsTocheck[i];
                    string fittingPart = string.Empty;
                    foreach (string part in lastWordParts)
                    {
                        string escapedSymbol = Regex.Escape(part);
                        if (Regex.IsMatch(currentWord, @"(" + escapedSymbol + @")\b"))
                        {
                            fittingPart = part;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (fittingPart == teil)
                    {
                        fittingPart = string.Empty;
                    }

                    if (fittingPart.Length > minimum)
                    {
                        if (wordSimilarities.TryGetValue(fittingPart, out int value))
                        {
                            wordSimilarities[fittingPart] = ++value;
                        }
                        else
                        {
                            wordSimilarities.Add(fittingPart, 1);
                        }
                    }
                }
                timer2 = DateTime.Now;
                timeDifference.Add(timer2 - timer1);
                //}
            }

            //double durchschnittslänge = wordSimilarities.Average(x => x.Key.Length);
            //var groupedList = wordSimilarities.GroupBy(x => x.Key.Length).ToDictionary(x => x.Key, x => x.Count());
            //var meisteLänge = groupedList.OrderByDescending(x => x.Value).First();
            //var median = GetMedian(wordSimilarities.Select(x => x.Key.Length).Order().ToList());

            //foreach (var endung1 in wordSimilarities)
            //{
            //    foreach (var endung2 in wordSimilarities)
            //    {
            //        if (endung1.Key != endung2.Key)
            //        {
            //            var escapedSymbol = Regex.Escape(endung1.Key);
            //            if (Regex.IsMatch(endung2.Key, @"(" + escapedSymbol + @")\b"))
            //            {
            //                wordSimilarities[endung1.Key] -= endung2.Value;

            //                int abstand1 = Math.Abs(meisteLänge.Key - endung1.Key.Length);
            //                int abstand2 = Math.Abs(meisteLänge.Key - endung2.Key.Length);
            //                if (abstand1 > abstand2)
            //                    wordSimilarities[endung1.Key] -= endung2.Value;
            //                else if(abstand2 > abstand1)
            //                    wordSimilarities[endung2.Key] -= endung1.Value;
            //                else
            //                {
            //                    if(endung1.Key.Length < endung2.Key.Length)
            //                        wordSimilarities[endung2.Key] -= endung1.Value;
            //                    else
            //                        wordSimilarities[endung1.Key] -= endung2.Value;
            //                }
            //            }                            
            //        }
            //    }
            //    if (wordSimilarities[endung1.Key] <= 0)
            //        wordSimilarities.Remove(endung1.Key);
            //}

            KeyValuePair<int, int> kumulierteSeltensteHäufigkeit = wordSimilarities.GroupBy(x => x.Value).ToDictionary(x => x.Key, x => x.Count()).Where(x => x.Key > 1).OrderByDescending(x => x.Value).First();
            double Duchschnittszeit = timeDifference.Average(timeSpan => timeSpan.TotalSeconds);

            return wordSimilarities.Where(x => x.Value > kumulierteSeltensteHäufigkeit.Key).OrderByDescending(x => x.Key.Length).ToDictionary(x => x.Key, x => x.Value);
        }

        public static Dictionary<string, int> WordSimilartyBeginningOfWord(List<string> wordsTocheck, int minimum)
        {
            DateTime timer1 = new();
            DateTime timer2 = new();
            List<TimeSpan> timeDifference = [];
            int index = 0;
            Dictionary<string, int> wordSimilarities = [];

            foreach (string word in wordsTocheck)
            {
                index++;
                string[] teile = word.Split(" ");
                //foreach (var teil in teile)
                //{

                string teil = teile[0];
                timer1 = DateTime.Now;
                char[] wordSplits = teil.ToCharArray();
                List<string> lastWordParts = [];
                if (teil.Length <= minimum)
                {
                    lastWordParts.Add(teil);
                }
                else
                {
                    for (int j = minimum; j < wordSplits.Length; j++)
                    {
                        if (lastWordParts.Count > 0)
                        {
                            lastWordParts.Add(lastWordParts.Last() + wordSplits[j]);
                        }
                        else
                        {
                            string start = string.Empty;
                            for (int k = 0; k < minimum; k++)
                            {
                                start += wordSplits[k].ToString();
                            }
                            lastWordParts.Add(start);
                        }
                    }
                }

                for (int i = index; i < wordsTocheck.Count; i++)
                {
                    string currentWord = wordsTocheck[i];
                    string fittingPart = string.Empty;
                    foreach (string part in lastWordParts)
                    {
                        string escapedSymbol = Regex.Escape(part);
                        if (Regex.IsMatch(currentWord, @"\b(" + escapedSymbol + @")"))
                        {
                            fittingPart = part;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (fittingPart == teil)
                    {
                        fittingPart = string.Empty;
                    }

                    if (fittingPart.Length > minimum)
                    {
                        if (wordSimilarities.TryGetValue(fittingPart, out int value))
                        {
                            wordSimilarities[fittingPart] = ++value;
                        }
                        else
                        {
                            wordSimilarities.Add(fittingPart, 1);
                        }
                    }
                }

                timer2 = DateTime.Now;
                timeDifference.Add(timer2 - timer1);
                //}
            }
            KeyValuePair<int, int> kumulierteSeltensteHäufigkeit = new();
            if (wordSimilarities.Count > 0)
            {
                kumulierteSeltensteHäufigkeit = wordSimilarities.GroupBy(x => x.Value).ToDictionary(x => x.Key, x => x.Count()).Where(x => x.Key > 1).OrderByDescending(x => x.Value).First();
            }

            double Duchschnittszeit = timeDifference.Average(timeSpan => timeSpan.TotalSeconds);

            return wordSimilarities.Where(x => x.Value > kumulierteSeltensteHäufigkeit.Key).OrderByDescending(x => x.Key.Length).ToDictionary(x => x.Key, x => x.Value);
        }

        public static Dictionary<string, int> NewWordSimilarity(List<string> wordsTocheck)
        {
            List<string> wordSimilarities = [];
            foreach (string word in wordsTocheck)
            {
                string[] teile = word.Split(" ");
                foreach (string teil in teile)
                {
                    wordSimilarities.AddRange(GetAllSubstrings(word).ToList());
                }
            }

            Dictionary<string, int> wordSimilaritiesDic = wordSimilarities.Where(w => w.Length > 1).GroupBy(w => w).ToDictionary(w => w.Key, w => w.Count());

            return wordSimilaritiesDic.Where(w => w.Value > 1).OrderByDescending(x => x.Value).ThenBy(x => x.Key.Length).ToDictionary(x => x.Key, x => x.Value);
        }

        public static IEnumerable<string> GetAllSubstrings(this string word)
        {
            return from charIndex1 in Enumerable.Range(0, word.Length)
                   from charIndex2 in Enumerable.Range(0, word.Length - charIndex1 + 1)
                   where charIndex2 > 0
                   select word.Substring(charIndex1, charIndex2);
        }
    }
}
