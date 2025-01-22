using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sammlerplattform.Controllers.DAL;
using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.ManufactoryDatabase;
using Sammlerplattform.Models.PersonDatabase;
using Sammlerplattform.Models.ProductPictureDatabase;
using Sammlerplattform.Models.UserSettings;
using Sammlerplattform.Services;
using System.Security.Claims;
using System.Text.Json;
using System.Transactions;

namespace Sammlerplattform.Controllers
{
    [Authorize]
    public class BrickDatabaseController(IProcessBrick processBrick) : Controller
    {
        public ActionResult AdministerCollectionBrick(string statusMessage, BrickSearchParameterModel model)
        {
            List<BrickOperationParameterModel> brickSelectAll = (from b in processBrick.GetWithPredicates(model)
                                                                 select b).ToList();

            ViewData["StatusMessage"] = statusMessage;

            return View(brickSelectAll);
        }
        public ActionResult CreateBrick(string statusMessage)
        {
            ViewData["StatusMessage"] = statusMessage;

            return View();
        }
        public IActionResult CreateBrickSubmit(BrickOperationParameterModel model)
        {
            (BrickEntity _, int _, string statusMessage) = processBrick.Create(model);

            return RedirectToAction(nameof(CreateBrick), new { statusMessage });
        }

        public ActionResult EditManufactory(string statusMessage, int id)
        {
            BrickSearchParameterModel brickSearch = new();
            brickSearch.SearchBrickEntity_ID.Add(id);
            BrickOperationParameterModel brickSelect = (from b in processBrick.GetWithPredicates(brickSearch)
                                                        select b).First();

            ViewData["StatusMessage"] = statusMessage;
            return View(brickSelect);
        }
        //public ActionResult EditManufactorySubmit(ManufactoryOperationParameterModel manufactoryParameterModel)
        //{
        //    return RedirectToAction(nameof(EditManufactory), new { statusMessage, id = manufactory.Manufactory_ID });
        //}
    }

    public interface IProcessBrick
    {
        IEnumerable<BrickOperationParameterModel> GetWithPredicates(BrickSearchParameterModel model);
        (BrickEntity brickEntity, int statuscode, string message) Create(BrickOperationParameterModel model);
        (BrickEntity brickEntity, int statuscode, string message) Edit(BrickOperationParameterModel model);
        (BrickEntity brickEntity, int statuscode, string message) Delete(BrickOperationParameterModel model);
        BrickSearchParameterModel ParametersOperationToSearch(BrickOperationParameterModel model);
    }

    public class BrickProcessor(IUnitOfWork unitOfWork, ILogger<BrickProcessor> logger, IProcessManufactory processManufactory, UserAccessor userAccessor) : IProcessBrick
    {
        public BrickSearchParameterModel ParametersOperationToSearch(BrickOperationParameterModel operationParameterModel)
        {
            BrickSearchParameterModel searchParameterModel = new();
            searchParameterModel.SearchBrickEntity_ID.Add(operationParameterModel.BrickEntity.BrickEntity_ID);
            if (operationParameterModel.Brickname != null)
            {
                searchParameterModel.SearchBrickname.Add(operationParameterModel.Brickname.Name);
            }

            return searchParameterModel;
        }
        public (BrickEntity brickEntity, int statuscode, string message) Create(BrickOperationParameterModel model)
        {
            BrickOperationParameterModel? brickOperationParameterModel = GetWithPredicates(ParametersOperationToSearch(model)).FirstOrDefault();

            if (brickOperationParameterModel != null)
            {
                return (brickOperationParameterModel.BrickEntity, 302, "Eintrag existiert bereits.");
            }

            try 
            { 
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                model.BrickEntity.UsingIdentityUsers_ID = userAccessor.GetUserId();

                BrickEntity newBrickEntity = unitOfWork.BrickEntityRepository.Insert(model.BrickEntity);
                unitOfWork.Save();

                BrickPotential? brickPotential = ConnectPotentialToEntity(model.BrickPotential.BrickPotential_ID, newBrickEntity);
                ConnectManufactoryToBrickEntity(model.BrickworksTuple, newBrickEntity);
                ConnectManufacturerToBrickEntity(model.Manufacturer, newBrickEntity);
                ConnectPictureToBrickEntity(model.ProductPicture, model.TextPositionString, newBrickEntity);

                scope.Complete();
                return (newBrickEntity, 201, "Ziegel wurde erstellt.");
            }
            catch (Exception ex)
            {
                logger.LogError("Fehler beim Hinzufügen: {ex.InnerException}", ex.InnerException);
                return (new(), 500, "Es ist ein Fehler aufgetreten: " + ex.InnerException);
            }
        }
        public (BrickEntity brickEntity, int statuscode, string message) Delete(BrickOperationParameterModel model)
        {
            throw new NotImplementedException();
        }
        public (BrickEntity brickEntity, int statuscode, string message) Edit(BrickOperationParameterModel model)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<BrickOperationParameterModel> GetWithPredicates(BrickSearchParameterModel model)
        {
            ExpressionStarter<BrickOperationParameterModel> predicate = PredicateBuilder.New<BrickOperationParameterModel>();
            IEnumerable<BrickOperationParameterModel> brickIEnumberable = from be in unitOfWork.BrickEntityRepository.Get(includeProperties: "BrickPotential,BrickPotential.BricknameSynonymICollection,ManufacturingDate,Brickworks,CityOfBrickworks")
                                                                              //where be.Manufactory?.ProductionFacility != null && be.Manufactory.ProductionFacility.ProductionFacilityName.Equals("Ziegelei")
                                                                          //where be.BrickPotential != null && be.BrickPotential.BricknameSynonymICollection.Any(x => x.Name.Equals(model.SearchBrickname))
                                                                          select new BrickOperationParameterModel
                                                                          {
                                                                              BrickEntity = be
                                                                           ,
                                                                              BrickPotential = be.BrickPotential ?? new()
                                                                           ,
                                                                              BrickworksTuple = new(be.Brickworks ?? new() { ManufactoryName = string.Empty }, be.CityOfBrickworks?.City_ID, (from c in unitOfWork.ManufactoryRepository.Get(includeProperties: "CityList") where c.Manufactory_ID == be.Brickworks_ID select c.CityICollection.ToList()).First())
                                                                           //,
                                                                           //   ManufacturingDate = be.ManufacturingDate ?? new()
                                                                              //,
                                                                              //Manufacturer = subBmm.Manufacturer ?? new()
                                                                            ,
                                                                              Brickname = be.BrickPotential!.BricknameSynonymICollection.First()
                                                                          };

            predicate = predicate.And(GenericClasses.GenericLambdas.CreateLambdaSpanIntJoin<BrickOperationParameterModel>("BrickPotential", "BrickPotential_ID", model.SearchBrickEntity_ID));
            predicate = predicate.And(GenericClasses.GenericLambdas.CreateLambdaStringEqualsJoin<BrickOperationParameterModel>("Brickname", "Name", model.SearchBrickname));
            predicate = predicate.And(GenericClasses.GenericLambdas.CreateLambdaStringEqualsJoin<BrickOperationParameterModel>("BrickPotential", "Relief", model.SearchRelief));
            predicate = predicate.And(GenericClasses.GenericLambdas.CreateLambdaStringEqualsJoin<BrickOperationParameterModel>("BrickPotential", "Usage", model.SearchUsage));
            predicate = predicate.And(GenericClasses.GenericLambdas.CreateLambdaStringEqualsJoin<BrickOperationParameterModel>("BrickEntity", "SerialNumber", model.SearchSerialnumber));
            predicate = predicate.And(GenericClasses.GenericLambdas.CreateLambdaSpanIntJoin<BrickOperationParameterModel>("BrickEntity", "BrickEntity_ID", model.SearchBrickEntity_ID));
            predicate = predicate.And(GenericClasses.GenericLambdas.CreateLambdaStringEqualsJoin<BrickOperationParameterModel>("BrickEntity", "FilingLocation", model.SearchFilingLocation));
            predicate = predicate.And(GenericClasses.GenericLambdas.CreateLambdaSpanDecimalJoin<BrickOperationParameterModel>("BrickEntity", "Price", model.SearchPrice));
            if (model.SearchFake == "on")
            {
                predicate = predicate.And(x => x.BrickEntity.Fake);
            }
            predicate = predicate.And(GenericClasses.GenericLambdas.CreateLambdaStringEqualsJoin<BrickOperationParameterModel>("BrickEntity", "Material", model.SearchMaterial));
            predicate = predicate.And(GenericClasses.GenericLambdas.CreateLambdaStringEqualsJoin<BrickOperationParameterModel>("BrickEntity", "ConditionInt", model.SearchCondition));
            //Width, Height, Length is missing, because it should seen together
            predicate = predicate.And(GenericClasses.GenericLambdas.QueryManufacturingDate<BrickOperationParameterModel>(model.SearchYear));
            predicate = predicate.And(GenericClasses.GenericLambdas.CreateLambdaStringContainsJoin<BrickOperationParameterModel>("Manufacturer", "Name", model.SearchName));
            predicate = predicate.And(GenericClasses.GenericLambdas.CreateLambdaStringEqualsJoin<BrickOperationParameterModel>("Manufacturer", "PersonSignature", model.SearchSignature));

            if (predicate.IsStarted)
            {
                _ = brickIEnumberable.Where(predicate);
            }

            return brickIEnumberable;
        }

        private BrickPotential? ConnectPotentialToEntity(int brickPotential_ID, BrickEntity brickEntity)
        {
            BrickPotential? brickPotential = unitOfWork.BrickPotentialRepository.GetByID(brickPotential_ID);

            if (brickPotential != null)
            {
                unitOfWork.BrickEntityRepository.SetForeignKey(brickEntity, b => b.BrickPotential_ID, brickPotential.BrickPotential_ID);
                unitOfWork.BrickPotentialRepository.AddMemberToCollection(brickPotential, bp => bp.BrickEntityICollection, brickEntity);
                unitOfWork.Save();
            }            

            return brickPotential;
        }
        private void ConnectManufactoryToBrickEntity((Manufactory Name, int? Location, List<City> CityList) manufactoryTuple, BrickEntity brickEntity)
        {
            if (manufactoryTuple.Name != null || manufactoryTuple.Location != null)
            {
                ManufactoryOperationParameterModel manufactoryOperationParameter = new()
                {
                    Manufactory = manufactoryTuple.Name,
                    City = unitOfWork.CityRepository.Get().ToList().Where(x => x.City_ID == manufactoryTuple.Location).First()
                };
                Manufactory manufactory = processManufactory.CreateManufactory(manufactoryOperationParameter).manufactory;

                unitOfWork.BrickEntityRepository.SetForeignKey(brickEntity, b => b.Brickworks_ID, manufactory.Manufactory_ID);
                unitOfWork.ManufactoryRepository.AddMemberToCollection(manufactory, m => m.BrickEntityICollection, brickEntity);
                unitOfWork.Save();
            }
        }
        private void ConnectManufacturerToBrickEntity(Person manufacturer, BrickEntity brickEntity)
        {
            Person? person = (from p in unitOfWork.PersonRepository.Get()
                              where p.Person_ID == manufacturer.Person_ID
                              select p).FirstOrDefault();
            if (person != null)
            {
                unitOfWork.PersonRepository.AddMemberToCollection(person, p => p.BrickEntityBrickmakerICollection, brickEntity);
                unitOfWork.BrickEntityRepository.SetForeignKey(brickEntity, b => b.Brickmaker_ID, manufacturer.Person_ID);
                unitOfWork.Save();
            }
            else if(manufacturer.Person_ID > 0)
            {
                logger.LogWarning("Eingegebene Person {manufacturer.Person_ID} nicht verfügbar.", manufacturer.Person_ID);
            }
        }
        private void ConnectPictureToBrickEntity(ProductPicture productPicture, List<string> textPositionListString, BrickEntity brickEntity)
        {
            CreateAddTextPositionJson(productPicture, textPositionListString);

            ProductPicture newProductPicture = unitOfWork.ProductPictureRepository.Insert(productPicture);
            unitOfWork.Save();

            unitOfWork.BrickEntityRepository.AddMemberToCollection(brickEntity, b => b.ProductPictureICollection, productPicture);
            unitOfWork.ProductPictureRepository.SetForeignKey(productPicture, p => p.BrickEntity_ID, brickEntity.BrickEntity_ID);
            unitOfWork.Save();
        }

        private static void CreateAddTextPositionJson(ProductPicture productPicture, List<string> textPositionListString)
        {
            List<TextPosition> textPositionList = [];
            foreach (string textPositionConjugated in textPositionListString)
            {
                string[] textPositionMembers = textPositionConjugated.Split("§§");

                TextPosition textPosition = new()
                {
                    Text = textPositionMembers[0]
                };

                if (!string.IsNullOrEmpty(textPositionMembers[1]))
                {
                    textPosition.Height = Int32.Parse(textPositionMembers[1]);
                }
                if (!string.IsNullOrEmpty(textPositionMembers[2]))
                {
                    textPosition.XPosition = Int32.Parse(textPositionMembers[2]);
                }
                if (!string.IsNullOrEmpty(textPositionMembers[3]))
                {
                    textPosition.YPosition = Int32.Parse(textPositionMembers[3]);
                }

                textPositionList.Add(textPosition);
            };
            string jsonSerializer = JsonSerializer.Serialize<List<TextPosition>>(textPositionList);
            productPicture.TextPositionJson = jsonSerializer;
        }
    }
}
