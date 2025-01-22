using Google.Cloud.Vision.V1;
using ImageMagick;
using Microsoft.IdentityModel.Tokens;
using Sammlerplattform.Models;
using System.Text.RegularExpressions;

namespace Sammlerplattform.Controllers.PictureAnaylsis
{
    public partial class PicturePreprocess
    {
        public static void AnalyzeImage(IFormFile formFile, IWebHostEnvironment _hostEnvironment, WordCategorizationModel wordCategorizationModel)
        {
            MemoryStream ms = new();
            string dir = Path.Combine(_hostEnvironment.WebRootPath, "AnalyzedPics");
            if (!Directory.Exists(dir))
            {
                _ = Directory.CreateDirectory(dir);
            }

            if (formFile != null)
            {
                string fileName = Path.GetFileName(formFile.FileName);
                string pathFile = Path.Combine(Path.Combine(_hostEnvironment.WebRootPath, "DetectedText"), fileName);
                string pathFileImage = Path.Combine(Path.Combine(_hostEnvironment.WebRootPath, "FormattedImages"), fileName);

                string txt = string.Empty;

                //Image Preprocess
                formFile.CopyTo(ms);
                ms.Position = 0;
                MagickImage image = new(ms);

                //google API
                string credPath = Path.Combine(_hostEnvironment.ContentRootPath, "google_application_default_credentials.json");
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credPath);
                ImageAnnotatorClient client = ImageAnnotatorClient.Create();
                Google.Cloud.Vision.V1.Image newImage = Google.Cloud.Vision.V1.Image.FromStream(ms);

                if (wordCategorizationModel.Frontside)
                {
                    try
                    {
                        IReadOnlyList<LocalizedObjectAnnotation> objectAnnotations = client.DetectLocalizedObjects(newImage);
                        int countBuilding = 0;
                        foreach (LocalizedObjectAnnotation annotation in objectAnnotations)
                        {
                            if (annotation.Name is "House" or "Building")
                            {
                                List<int> xPositions = [];
                                List<int> yPositions = [];
                                countBuilding++;
                                foreach (NormalizedVertex? poly in annotation.BoundingPoly.NormalizedVertices)
                                {
                                    xPositions.Add((int)(poly.X * wordCategorizationModel.ImageWidth));
                                    yPositions.Add((int)(poly.Y * wordCategorizationModel.ImageHeight));
                                }
                                wordCategorizationModel.BuildingsWithXYPositionTupleList.Add((countBuilding, xPositions, yPositions, annotation.Score));
                            }
                            wordCategorizationModel.Countobjects++;
                        }
                    }
                    catch (AnnotateImageException e)
                    {
                        AnnotateImageResponse response = e.Response;
                        txt += "AnnotationError: " + response.Error;
                        wordCategorizationModel.CountErrors++;
                    }
                }
                try
                {
                    IReadOnlyList<EntityAnnotation> textAnnotations = client.DetectText(newImage);
                    if (!textAnnotations.IsNullOrEmpty())
                    {
                        for (int tAIndex = 1; tAIndex < textAnnotations.Count; tAIndex++)
                        {
                            Google.Protobuf.Collections.RepeatedField<Vertex> vertices = textAnnotations[tAIndex].BoundingPoly.Vertices;
                            List<int> xPositions = vertices.Select(x => x.X).ToList();
                            List<int> yPositions = vertices.Select(x => x.Y).ToList();
                            wordCategorizationModel.WordsWithXYPositions.Add((textAnnotations[tAIndex].Description, xPositions, yPositions, wordCategorizationModel.Frontside));
                        }
                        if (wordCategorizationModel.Frontside)
                        {
                            WordCategorization.CheckWordPosComparedToCenterOfImg(wordCategorizationModel);
                        }
                    }
                }
                catch (AnnotateImageException e)
                {
                    AnnotateImageResponse response = e.Response;
                    txt += "AnnotationError: " + response.Error;
                    wordCategorizationModel.CountErrors++;
                }

                if (wordCategorizationModel.WordsWithXYPositions.Count > 0)
                {
                    if (!wordCategorizationModel.Frontside)
                    {
                        WordCategorization.CheckWordSize(wordCategorizationModel);
                        CheckBlocks(wordCategorizationModel);
                        WordCategorization.CategorizeWord(wordCategorizationModel, _hostEnvironment);
                    }

                    string file = Path.Combine(dir, fileName + ".txt");
                    StreamWriter stream = new(file, true);
                    if (wordCategorizationModel.Frontside)
                    {
                        stream.WriteLine("Vorderseite");
                    }
                    else
                    {
                        stream.WriteLine("Rückseite");
                    }

                    if (!string.IsNullOrEmpty(txt))
                    {
                        stream.WriteLine("---------------------------------------------------------");
                        stream.WriteLine("Fehler");
                        stream.WriteLine("---------------------------------------------------------");
                        stream.WriteLine(txt);
                    }
                    stream.WriteLine("---------------------------------------------------------");
                    stream.WriteLine("Blocks");
                    stream.WriteLine("---------------------------------------------------------");
                    foreach ((string content, string category, double prop, bool Frontside) block in wordCategorizationModel.Blocks)
                    {
                        stream.WriteLine(block);
                    }
                    stream.WriteLine("---------------------------------------------------------");
                    stream.WriteLine("Zugeordnet");
                    stream.WriteLine("---------------------------------------------------------");
                    foreach ((string Word, int Block, int Position, List<(double Weight, string CategoryName, string? CategorizedTo, string CategorizedWhere)> Category, bool Frontside) wbc in wordCategorizationModel.WordBlockCategorization)
                    {
                        stream.WriteLine(wbc);
                        foreach ((double Weight, string CategoryName, string? CategorizedTo, string CategorizedWhere) category in wbc.Category)
                        {
                            stream.WriteLine("  - " + category);
                        }
                    }

                    CreateBlocks(wordCategorizationModel);
                    stream.WriteLine("---------------------------------------------------------");
                    stream.WriteLine("Blocks kategorisiert");
                    stream.WriteLine("---------------------------------------------------------");
                    foreach ((string content, string category, double prop, bool Frontside) block in wordCategorizationModel.Blocks)
                    {
                        stream.WriteLine(block);
                    }
                    stream.Close();
                }
            }
        }

        public static string SaveFileForAnalysis(IFormFile fileToAnalyze, IWebHostEnvironment hostEnvironment)
        {
            Random random;
            string pathTemp, fileName, pathFile;
            MemoryStream ms;
            MagickImage image;
            random = new Random();
            pathTemp = Path.Combine(hostEnvironment.WebRootPath, "images/Zwischenablage");
            fileName = random.Next(1, 1000) + DateTime.Now.ToString("_ddMMyyhhmmss");
            //int frontsideNr = random.Next(1, 1000) + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond + DateTime.Now.Microsecond;
            pathFile = Path.Combine(pathTemp, fileName + ".png");

            ms = new MemoryStream();
            fileToAnalyze.CopyTo(ms);
            ms.Position = 0;
            image = new MagickImage(ms)
            {
                Format = MagickFormat.Png
            };
            image.Write(pathFile);
            File.SetLastAccessTime(pathFile, DateTime.Now);

            return pathFile;
        }

        public static void CheckBlocks(WordCategorizationModel model)
        {
            int currentBlock = 0;
            int positionInBlock = 0;
            bool blockHorizontal = false;
            int countWordsInBlock = 1;
            double blockAngle = 0.0;
            bool curveText = false;
            int buildingNo = 0;
            List<(int index, int x, int y)> adressList = [];

            int indexPrp = 0;
            Dictionary<int, string> itemToChange = [];
            //Preprocess
            foreach ((string word, List<int> X, List<int> Y, bool Frontside) in model.WordsWithXYPositions)
            {
                if (RegexBlockWithoutBlank().IsMatch(word) && !RegexNumber().IsMatch(word))
                {
                    // z.B. Warengeschäft.B.Stützel => Warengeschäft. B. Stützel
                    itemToChange.Add(indexPrp, RegexSpecialCharacterBeforeLetter().Replace(word, @"$1 $2"));
                }
                else if (RegexTwoWordsPutTogether().IsMatch(word))
                {
                    // z.B. WarengeschäftB. => Warengeschäft B.
                    itemToChange.Add(indexPrp, RegexTwoWordsPutTogether().Replace(word, @"$1 $2"));
                }

                indexPrp++;
            }
            foreach (KeyValuePair<int, string> item in itemToChange)
            {
                string[] splitWord = item.Value.Split(" ");
                for (int i = splitWord.Length - 1; i > -1; i--)
                {
                    (string word, List<int> X, List<int> Y, bool front) wwp = model.WordsWithXYPositions[item.Key];
                    wwp.word = splitWord[i];
                    if (i == splitWord.Length - 1)
                    {
                        model.WordsWithXYPositions[item.Key] = wwp;
                    }
                    else
                    {
                        model.WordsWithXYPositions.Insert(item.Key, wwp);
                    }
                }
            }

            for (int index = 0; index < model.WordsWithXYPositions.Count; index++)
            {
                int letterHeightHorizontal = Math.Abs(model.WordsWithXYPositions[index].Y[3] - model.WordsWithXYPositions[index].Y[0]);
                int letterWidthHorizontal = Math.Abs(model.WordsWithXYPositions[index].X[1] - model.WordsWithXYPositions[index].X[0]);
                int letterHeightVertical = Math.Abs(model.WordsWithXYPositions[index].X[0] - model.WordsWithXYPositions[index].X[3]);
                int letterWidthVertical = Math.Abs(model.WordsWithXYPositions[index].Y[2] - model.WordsWithXYPositions[index].Y[3]);
                bool currentHorizontal = false;
                bool isBuilding = false;
                bool stillSameBuilding = false;
                double buildingScore = 0;
                bool blockChanged = false;

                double wordAngle = 0.0;
                if (model.WordsWithXYPositions[index].X[0] - model.WordsWithXYPositions[index].X[1] != 0)
                {
                    int hypothenuse = model.WordsWithXYPositions[index].Y[1] - model.WordsWithXYPositions[index].Y[0];
                    int leg = model.WordsWithXYPositions[index].X[1] - model.WordsWithXYPositions[index].X[0];
                    wordAngle = Math.Atan2(hypothenuse, leg) * 180.0 / Math.PI;
                }

                if (wordAngle is > 15 and < 75)
                {
                    curveText = true;
                }

                if (letterHeightHorizontal > letterHeightVertical && letterWidthHorizontal > letterWidthVertical)
                {
                    currentHorizontal = true;
                }

                if (model.WordsWithXYPositions[index].Frontside)
                {
                    foreach ((int building, List<int> X, List<int> Y, double Score) building in model.BuildingsWithXYPositionTupleList)
                    {
                        if (model.WordsWithXYPositions[index].X.Max() > building.X.Min() && model.WordsWithXYPositions[index].X.Min() < building.X.Max()
                            && model.WordsWithXYPositions[index].Y.Max() > building.Y.Min() && model.WordsWithXYPositions[index].Y.Min() < building.Y.Max()
                            && model.WordsWithXYPositions[index].X.Average() < building.X.Max() && model.WordsWithXYPositions[index].X.Average() > building.X.Min())
                        {
                            isBuilding = true;
                            buildingScore = building.Score; //Erscheint unnötig, aber es kann sein, dass vorheriger, bzw. nachfolgender, nicht gleiches Gebäude ist
                            if (buildingNo == building.building)
                            {
                                stillSameBuilding = true;
                            }
                            else
                            {
                                buildingNo = building.building;
                            }

                            break;
                        }
                    }
                }

                if (index == 0)
                {
                    model.WordBlockCategorization.Add((model.WordsWithXYPositions[index].word, currentBlock, ++positionInBlock, [], model.WordsWithXYPositions[index].Frontside));
                    blockAngle = wordAngle;
                    blockHorizontal = currentHorizontal;
                }
                else if (stillSameBuilding)
                {
                    model.WordBlockCategorization.Add((model.WordsWithXYPositions[index].word, currentBlock, ++positionInBlock, [], model.WordsWithXYPositions[index].Frontside));
                    blockAngle = wordAngle;
                    blockHorizontal = currentHorizontal;
                }
                else
                {
                    if (model.WordsWithXYPositions[index].word.Length == 1)
                    {
                        model.WordBlockCategorization.Add((model.WordsWithXYPositions[index].word, currentBlock, ++positionInBlock, [], model.WordsWithXYPositions[index].Frontside));

                        countWordsInBlock++;
                    }
                    else
                    {
                        if (currentHorizontal)
                        {
                            if (!curveText && !blockHorizontal)
                            {
                                //if it was not horizontal before
                                currentBlock++;
                                blockChanged = true;
                                model.MaxWordsInBlock.Add(positionInBlock);
                                model.WordBlockCategorization.Add((model.WordsWithXYPositions[index].word, currentBlock, positionInBlock = 0, [], model.WordsWithXYPositions[index].Frontside));
                                blockAngle = wordAngle;
                                countWordsInBlock = 1;
                            }
                            else
                            {
                                double letterHeight = letterHeightHorizontal * 1.4;
                                int wordDistance = Math.Abs(model.WordsWithXYPositions[index - 1].X[1] - model.WordsWithXYPositions[index].X[0]);
                                double lineOfWord = (model.WordsWithXYPositions[index - 1].Y[1] + Math.Tan(blockAngle)) * 1.02;
                                if (letterHeight > wordDistance && lineOfWord >= model.WordsWithXYPositions[index].Y[0])
                                {
                                    model.WordBlockCategorization.Add((model.WordsWithXYPositions[index].word, currentBlock, ++positionInBlock, [], model.WordsWithXYPositions[index].Frontside));
                                    countWordsInBlock++;
                                }
                                else if (model.WordsWithXYPositions[index].Y.Min() > model.WordsWithXYPositions[index - 1].Y.Min()
                                    && letterHeightHorizontal > Math.Abs(model.WordsWithXYPositions[index - countWordsInBlock].Y[3] - model.WordsWithXYPositions[index].Y[0])
                                    && !(blockAngle - 2 > wordAngle) && !(blockAngle + 2 < wordAngle)
                                    && model.WordsWithXYPositions[index - countWordsInBlock].X[1] > model.WordsWithXYPositions[index].X[1])
                                {
                                    //Auch untere Zeile einschließen, wenn nicht zu schräg, relativ zu aktuellem Block & untere Zeile nicht zu weit enfernt von oberer Zeile
                                    model.WordBlockCategorization.Add((model.WordsWithXYPositions[index].word, currentBlock, ++positionInBlock, [], model.WordsWithXYPositions[index].Frontside));
                                    countWordsInBlock++;
                                }
                                else
                                {
                                    currentBlock++;
                                    blockChanged = true;
                                    model.MaxWordsInBlock.Add(positionInBlock);
                                    model.WordBlockCategorization.Add((model.WordsWithXYPositions[index].word, currentBlock, positionInBlock = 0, [], model.WordsWithXYPositions[index].Frontside));
                                    blockAngle = wordAngle;
                                    countWordsInBlock = 1;
                                }
                            }
                            blockHorizontal = currentHorizontal;
                        }
                        else if (!currentHorizontal)
                        {
                            //vertical
                            if (!curveText && blockHorizontal)
                            {
                                currentBlock++;
                                blockChanged = true;
                                model.MaxWordsInBlock.Add(positionInBlock);
                                model.WordBlockCategorization.Add((model.WordsWithXYPositions[index].word, currentBlock, positionInBlock = 0, [], model.WordsWithXYPositions[index].Frontside));
                                blockAngle = wordAngle;
                                countWordsInBlock = 1;
                            }
                            else
                            {
                                int widthBottonOfVerticalWord = Math.Abs(model.WordsWithXYPositions[index].Y[0] - model.WordsWithXYPositions[index - 1].Y[1]);
                                int positionBeginningOfWord = model.WordsWithXYPositions[index].X[0] + 1;
                                int positionEndOfFollowingWord = model.WordsWithXYPositions[index - 1].X[1];
                                int heightBeginOfVerticalWord = Math.Abs(model.WordsWithXYPositions[index - countWordsInBlock].X[3] - model.WordsWithXYPositions[index].X[0]);
                                if (letterHeightVertical * 1.4 > widthBottonOfVerticalWord && positionBeginningOfWord >= positionEndOfFollowingWord)
                                {
                                    //The distance between words is equal or smaler than the Height
                                    model.MaxWordsInBlock.Add(positionInBlock);
                                    model.WordBlockCategorization.Add((model.WordsWithXYPositions[index].word, currentBlock, positionInBlock = 0, [], model.WordsWithXYPositions[index].Frontside));
                                    model.WordBlockCategorization[index].Category.Add((0.3, "AuthorArtist", string.Empty, "CheckBlocks"));
                                    model.WordBlockCategorization[index].Category.Add((0.3, "Publisher", string.Empty, "CheckBlocks"));
                                    countWordsInBlock++;
                                }
                                else if (letterWidthVertical > heightBeginOfVerticalWord && !(blockAngle - 2 > wordAngle) && !(blockAngle + 2 < wordAngle))
                                {
                                    model.MaxWordsInBlock.Add(positionInBlock);
                                    // If Word is uneven
                                    model.WordBlockCategorization.Add((model.WordsWithXYPositions[index].word, currentBlock, positionInBlock = 0, [], model.WordsWithXYPositions[index].Frontside));
                                    countWordsInBlock++;
                                }
                                else
                                {
                                    currentBlock++;
                                    blockChanged = true;
                                    model.MaxWordsInBlock.Add(positionInBlock);
                                    model.WordBlockCategorization.Add((model.WordsWithXYPositions[index].word, currentBlock, positionInBlock = 0, [], model.WordsWithXYPositions[index].Frontside));
                                    model.WordBlockCategorization[index].Category.Add((0.3, "AuthorArtist", string.Empty, "CheckBlocks"));
                                    model.WordBlockCategorization[index].Category.Add((0.3, "Publisher", string.Empty, "CheckBlocks"));
                                    blockAngle = wordAngle;
                                    countWordsInBlock = 1;
                                }
                            }

                            blockHorizontal = currentHorizontal;
                        }
                    }
                }

                if (isBuilding)
                {
                    if (buildingScore == 0)
                    {
                        buildingScore = 1;
                    }

                    model.WordBlockCategorization[index].Category.Add((buildingScore, "Building", buildingNo.ToString(), "CheckBlocks"));
                }

                if (!model.WordsWithXYPositions[index].Frontside)
                {
                    if (model.WordsWithXYPositions[index].X.Max() < model.ImageWidth / 2)
                    {
                        if (blockChanged)
                        {
                            model.WordBlockCategorization[index].Category.Add((0.3, "Text", string.Empty, "CheckBlocks"));
                            if (!currentHorizontal)
                            {
                                model.WordBlockCategorization[index].Category.Add((0.3, "Publisher", string.Empty, "CheckBlocks"));
                            }

                            model.WordBlockCategorization[index].Category.Add((0.3, "City", string.Empty, "CheckBlocks"));
                        }
                    }
                    else
                    {
                        if (model.WordsWithXYPositions[index].Y.Max() > model.ImageHeight * 0.4)
                        {
                            model.WordBlockCategorization[index].Category.Add((0.6, "Address", string.Empty, "CheckBlocks"));
                            adressList.Add((index, model.WordsWithXYPositions[index].X[0], model.WordsWithXYPositions[index].Y[0]));
                        }
                        else
                        {
                            if (model.WordsWithXYPositions[index].X.Max() >= model.ImageWidth * 0.75)
                            {
                                model.WordBlockCategorization[index].Category.Add((0.6, "Postmark", string.Empty, "CheckBlocks"));
                                model.WordBlockCategorization[index].Category.Add((0.6, "Stamp", string.Empty, "CheckBlocks"));
                            }
                            else
                            {
                                model.WordBlockCategorization[index].Category.Add((0.6, "Postmark", string.Empty, "CheckBlocks"));
                            }
                        }
                    }

                }

            }
            model.MaxWordsInBlock.Add(positionInBlock);

            adressList = [.. adressList.OrderBy(i => i.x).OrderBy(i => i.y)];
            int lowerX = adressList.FirstOrDefault().x + 5;
            int row = 1;
            int col = 1;
            foreach ((int index, int x, int y) in adressList)
            {
                if (x > lowerX)
                {
                    lowerX = x + 5;
                    row++;
                }

                switch (row)
                {
                    case 1:
                        //Frau, Herrn, Fräulein
                        break;
                    case 2:
                        if (col == 1)
                        {
                            model.WordBlockCategorization[index].Category.Add((0.4, "ForeName", string.Empty, "CheckBlocks"));
                        }
                        else
                        {
                            model.WordBlockCategorization[index].Category.Add((0.1, "Forename", string.Empty, "CheckBlocks"));
                            model.WordBlockCategorization[index].Category.Add((0.4, "Surname", string.Empty, "CheckBlocks"));
                        }
                        break;
                    case 3:
                        model.WordBlockCategorization[index].Category.Add((0.6, "Street", string.Empty, "CheckBlocks"));
                        break;
                    case 4:
                        model.WordBlockCategorization[index].Category.Add((0.6, "City", string.Empty, "CheckBlocks"));
                        break;
                    default:
                        break;

                }
                col++;
            }

            return;
        }

        public static void CreateBlocks(WordCategorizationModel model)
        {
            model.Blocks = [];
            string blockText = string.Empty;
            int currentBlock = 0;
            int maxOfwbc = model.WordBlockCategorization.Count;
            int idx = 0;
            Dictionary<string, double> totalPropability = new()
            {
                { "Building", 0 },
                { "City", 0 },
                { "AuthorArtist", 0 },
                { "Publisher", 0 },
                { "Forename", 0 },
                { "Surname", 0 },
                { "Address", 0 },
                { "Stamp", 0 },
                { "Postmark", 0 },
                { "Number", 0 },
                { "Year", 0 },
                { "Date", 0 },
                { "Geography", 0 },
                { "Occasion", 0 },
                { "Street", 0 },
                { "Text", 0 }
            };
            Dictionary<int, string> wordsToChange = [];

            foreach ((string Word, int Block, int Position, List<(double Weight, string CategoryName, string? CategorizedTo, string CategorizedWhere)> Category, bool Frontside) wbc in model.WordBlockCategorization)
            {
                bool textWasCountedIn = false;
                string? wordToUse = string.Empty;
                if (currentBlock == wbc.Block)
                {
                    AddBlockToCategory(model, idx, totalPropability, wbc, ref textWasCountedIn, ref wordToUse, ref wordsToChange);
                    if (string.IsNullOrEmpty(wordToUse))
                    {
                        //Wenn Sonderzeichen am Beginn des Wortes oder wenn blocktext mit z.b. Schriftsteller' endet
                        if (RegexSpecialCharacterAtBeginningOfWord().IsMatch(wbc.Word) || RegexWordEndsWithLineOrApostrophe().IsMatch(blockText))
                        {
                            blockText += wbc.Word;
                        }
                        else
                        {
                            blockText += " " + wbc.Word;
                        }
                    }
                    else
                    {
                        if (RegexSpecialCharacter().IsMatch(wbc.Word))
                        {
                            blockText += wordToUse;
                        }
                        else
                        {
                            blockText += " " + wordToUse;
                        }
                    }
                }
                else
                {
                    SetMaxOfTotalPropability(model, blockText, idx, totalPropability);
                    totalPropability = new()
                    {
                        { "Building", 0 },
                        { "City", 0 },
                        { "AuthorArtist", 0 },
                        { "Publisher", 0 },
                        { "Surname", 0 },
                        { "Forename", 0 },
                        { "Address", 0 },
                        { "Stamp", 0 },
                        { "Postmark", 0 },
                        { "Number", 0 },
                        { "Year", 0 },
                        { "Date", 0 },
                        { "Geography", 0 },
                        { "Occasion", 0 },
                        { "Street", 0 },
                        { "Text", 0 }
                    };
                    //(double Propability, string CategoryName, string? CategorizedTo, string CategorizedWhere) maxCategory = new();
                    AddBlockToCategory(model, idx, totalPropability, wbc, ref textWasCountedIn, ref wordToUse, ref wordsToChange);
                    blockText = string.IsNullOrEmpty(wordToUse) ? wbc.Word : wordToUse;
                    currentBlock = wbc.Block;
                }

                if (maxOfwbc == idx)
                {
                    SetMaxOfTotalPropability(model, blockText, idx, totalPropability);
                }

                idx++;
            }

            //Neural Network for AuthorArtist and Publisher
            //var curNeuralNetwork = new NeuralNetWork(1, 3);
            //// Hier Output

            //var trainingInputs = new double[,] { { 0, 0, 1 }, { 1, 1, 1 }, { 1, 0, 1 }, { 0, 1, 1 } };
            //var trainingOutputs = NeuralNetWork.MatrixTranspose(new double[,] { { 0, 1, 1, 0 } });

            //curNeuralNetwork.Train(trainingInputs, trainingOutputs, 10000);

            //Console.WriteLine("\nSynaptic weights after training:");
            //PrintMatrix(curNeuralNetwork.SynapsesMatrix);

            //// testing neural networks against a new problem 
            //var output = curNeuralNetwork.Think(new double[,] { { 1, 0, 0 } });
            //// Hier Output

            foreach (KeyValuePair<int, string> wordToChange in wordsToChange)
            {
                (string Word, int Block, int Position, List<(double Propability, string CategoryName, string? CategorizedTo, string CategorizedWhere)> Category, bool Frontside) wbc = model.WordBlockCategorization[wordToChange.Key];
                wbc.Word = wordToChange.Value;
                model.WordBlockCategorization[wordToChange.Key] = wbc;
            }

            return;
        }

        private static void AddBlockToCategory(WordCategorizationModel model, int idx, Dictionary<string, double> totalPropability, (string Word, int Block, int Position, List<(double Propability, string CategoryName, string? CategorizedTo, string CategorizedWhere)> Category, bool Frontside) wbc, ref bool textWasCountedIn, ref string? wordToUse, ref Dictionary<int, string> wordsToChange)
        {
            if (model.WordBlockCategorization[idx].Category.Count > 0)
            {
                foreach ((double Propability, string CategoryName, string? CategorizedTo, string CategorizedWhere) in wbc.Category)
                {
                    switch (CategoryName)
                    {
                        case "Building":
                            totalPropability["Building"] += Propability;
                            break;
                        case "Publisher":
                            totalPropability["Publisher"] += Propability;
                            break;
                        case "City":
                            totalPropability["City"] += Propability;
                            break;
                        case "AuthorArtist":
                            totalPropability["AuthorArtist"] += Propability;
                            break;
                        case "Surname":
                            totalPropability["Surname"] += Propability;
                            break;
                        case "Forename":
                            totalPropability["Forename"] += Propability;
                            break;
                        case "Address":
                            totalPropability["Address"] += Propability;
                            break;
                        case "Stamp":
                            totalPropability["Stamp"] += Propability;
                            break;
                        case "Postmark":
                            totalPropability["Postmark"] += Propability;
                            break;
                        case "Year":
                            totalPropability["Year"] += Propability;
                            break;
                        case "Date":
                            totalPropability["Date"] += Propability;
                            break;
                        case "Number":
                            totalPropability["Number"] += Propability;
                            break;
                        case "Geography":
                            totalPropability["Geography"] += Propability;
                            break;
                        case "Occasion":
                            totalPropability["Occasion"] += Propability;
                            break;
                        case "Street":
                            totalPropability["Street"] += Propability;
                            break;
                        default:
                            //if (!textWasCountedIn)
                            totalPropability["Text"] += Propability;
                            textWasCountedIn = true;
                            break;
                    }
                }
                (double Weight, string CategoryName, string? CategorizedTo, string CategorizedWhere) maxCategory = model.WordBlockCategorization[idx].Category.MaxBy(x => x.Weight);
                if (!string.IsNullOrEmpty(maxCategory.CategorizedTo) && maxCategory.CategoryName != "Building" && maxCategory.CategorizedTo != model.WordBlockCategorization[idx].Word)
                {
                    wordToUse = maxCategory.CategorizedTo;
                    wordsToChange.Add(idx, maxCategory.CategorizedTo);
                }
            }
        }

        private static void SetMaxOfTotalPropability(WordCategorizationModel model, string blockText, int idx, Dictionary<string, double> totalPropability)
        {
            KeyValuePair<string, double> maxOfTotalPropability = totalPropability.MaxBy(x => x.Value);
            if (maxOfTotalPropability.Value < 0.6)
            {
                if (totalPropability["Text"] > 0.3)
                {
                    model.Blocks.Add((blockText, "Text", 0.0, model.WordBlockCategorization[idx].Frontside));
                }
                else
                {
                    model.Blocks.Add((blockText, string.Empty, 0.0, model.WordBlockCategorization[idx].Frontside));
                }
            }
            else
            //Wenn nur Zahlen, und nicht Year, Date, Number, dann raus
            //if(!Regex.IsMatch(blockText,@"\d") && (maxOfTotalPropability.Key is not "Number" or not "Year")
            {
                if (maxOfTotalPropability.Key is "City")
                {
                    blockText = RegexSpecialCharacter().Replace(blockText, string.Empty);
                }

                model.Blocks.Add((blockText, maxOfTotalPropability.Key, maxOfTotalPropability.Value, model.WordBlockCategorization[idx].Frontside));
                if (maxOfTotalPropability.Key is "Forename" or "Surename" && totalPropability["Address"] < 0.8)
                {
                    model.Blocks.Add((blockText, "Text", 0.0, model.WordBlockCategorization[idx].Frontside));
                }
            }

            return;
        }

        public static string CreateCurrentBlock(int idx, WordCategorizationModel model)
        {
            int blockID = model.WordBlockCategorization[idx].Block;
            string blockText = string.Empty;
            foreach ((string Word, int Block, int Position, List<(double Weight, string CategoryName, string? CategorizedTo, string CategorizedWhere)> Category, bool Frontside) block in model.WordBlockCategorization.Where(x => x.Block.Equals(blockID)))
            {
                blockText += block.Word;
            }

            return blockText;
        }

        [GeneratedRegex(@"\d")]
        private static partial Regex RegexNumber();
        [GeneratedRegex(@"[A-z][<>'/;`%+{}\[\]‘\\°_*,?!():.-]+[A-z]")]
        private static partial Regex RegexBlockWithoutBlank();
        [GeneratedRegex(@"([<>';`%+{}\[\]‘\\°_*,?!():.-])(\w)")]
        private static partial Regex RegexSpecialCharacterBeforeLetter();
        [GeneratedRegex(@"[a-zäöü][A-ZÄÖÜ]")]
        private static partial Regex RegexTwoWordsPutTogether();
        [GeneratedRegex(@"\w[-']")]
        private static partial Regex RegexWordEndsWithLineOrApostrophe();
        [GeneratedRegex(@"\B[<>'/;`%+{}\[\]‘\\°_*,?!():=.-]")]
        private static partial Regex RegexSpecialCharacterAtBeginningOfWord();
        [GeneratedRegex(@"[<>'/;`%+{}\[\]‘\\°_*,?!():=.-]")]
        private static partial Regex RegexSpecialCharacter();

        //static void PrintMatrix(double[,] matrix)
        //{
        //    int rowLength = matrix.GetLength(0);
        //    int colLength = matrix.GetLength(1);

        //    for (int i = 0; i < rowLength; i++)
        //    {
        //        for (int j = 0; j < colLength; j++)
        //        {
        //            Console.Write(string.Format("{0} ", matrix[i, j]));
        //        }
        //        Console.Write(Environment.NewLine);
        //    }
        //}

    }
}
