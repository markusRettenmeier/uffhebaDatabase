using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.ManufactoryDatabase;
using Sammlerplattform.Models.PersonDatabase;
using Sammlerplattform.Models.ProductDatabase;
using Sammlerplattform.Models.ProductPictureDatabase;
using Sammlerplattform.Services.GenericClasses;
using Sammlerplattform.Services.UnitOfWork;
using System.Text.Json;
using System.Transactions;

namespace Sammlerplattform.Services.Processes
{
    public interface IProcessBrick
    {
        List<BrickOperationParameterModel> GetWithPredicates(BrickSearchParameterModel model);
        (BrickEntity brickEntity, int statuscode, string message) Create(BrickOperationParameterModel model);
        (BrickEntity brickEntity, int statuscode, string message) Edit(BrickOperationParameterModel model);
        (BrickEntity brickEntity, int statuscode, string message) Delete(BrickOperationParameterModel model);
        BrickSearchParameterModel ParametersOperationToSearch(BrickOperationParameterModel model);
    }

    public class BrickProcessor(IUnitOfWork unitOfWork, IProcessProductPicture processProductPicture) : IProcessBrick
    {
        public BrickSearchParameterModel ParametersOperationToSearch(BrickOperationParameterModel operationParameterModel)
        {
            BrickSearchParameterModel searchParameterModel = new();
            searchParameterModel.BrickEntityID.Add(operationParameterModel.BrickEntity.BrickEntityID);
            if (!string.IsNullOrEmpty(operationParameterModel.Brickname.Name))
            {
                searchParameterModel.BrickPotential_BricknameSynonymList_Name.Add(operationParameterModel.Brickname.Name);
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

                BrickEntity newBrickEntity = unitOfWork.BrickEntityRepository.Insert(model.BrickEntity);
                unitOfWork.Save();

                //ConnectPotentialToEntity(model.BrickPotential.BrickPotentialID, newBrickEntity);
                foreach (BrickEntityNManufactoryNCity entityNManufactoryNCity in model.BrickEntityNManufactoryNCityList)
                {
                        ConnectManufactoryToBrickEntity(newBrickEntity, entityNManufactoryNCity.ManufactoryID, entityNManufactoryNCity.CityID);
                }
                foreach (BrickEntityNPerson entityNPerson in model.BrickEntityNPersonList)
                {
                        ConnectPersonToBrickEntity(newBrickEntity, entityNPerson.PersonID, entityNPerson.Relationship);
                }
                foreach (var brickEntityNCity in model.BrickEntityNCityList)
                {
                        ConnectCityToBrickEntity(newBrickEntity, brickEntityNCity.CityID, brickEntityNCity.Relationship);
                }
                foreach (var productNColorVariant in model.ProductNColorVariantList)
                {
                        ConnectColorToBrickEntity(newBrickEntity, productNColorVariant);
                }
                //ConnectManufacturingDateToBrickEntity(model.ManufacturingDate, newBrickEntity);
                foreach (ProductPicture productPicture in model.ProductPictureList)
                {
                    //ConnectPictureToBrickEntity(newBrickEntity, productPicture);
                    //if (newBrickEntity.UsingIdentityUser != null)
                    //{
                    processProductPicture.Create(productPicture, newBrickEntity);
                    //}
                    //if (statuscode == 302)
                    //{
                    //    return (brickEntity, statuscode, message);
                    //}
                }
                scope.Complete();
                return (newBrickEntity, 201, "Ziegel wurde erstellt.");
            }
            catch (Exception ex)
            {
                return (new() { UsingIdentityUsersID = string.Empty }, 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }

        public (BrickEntity brickEntity, int statuscode, string message) Edit(BrickOperationParameterModel model)
        {
            BrickSearchParameterModel brickSearchParameterModel = new();
            brickSearchParameterModel.BrickEntityID.Add(model.BrickEntity.BrickEntityID);
            BrickEntity? existingBrickEntity = GetWithPredicates(brickSearchParameterModel).First().BrickEntity;

            if (existingBrickEntity == null)
            {
                return (new() { UsingIdentityUsersID = string.Empty }, 204, "Ziegel nicht gefunden.");
            }

            try
            {
                using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                existingBrickEntity.BrickPotentialID = model.BrickPotential.BrickPotentialID;
                existingBrickEntity.Charge = model.BrickEntity.Charge;
                existingBrickEntity.Comment = model.BrickEntity.Comment;
                existingBrickEntity.Width = model.BrickEntity.Width;
                existingBrickEntity.Height = model.BrickEntity.Height;
                existingBrickEntity.Length = model.BrickEntity.Length;
                existingBrickEntity.ConditionInt = model.BrickEntity.ConditionInt;
                existingBrickEntity.Fake = model.BrickEntity.Fake;
                existingBrickEntity.FilingLocation = model.BrickEntity.FilingLocation;
                existingBrickEntity.KeywordInt = model.BrickEntity.KeywordInt;
                existingBrickEntity.MaterialInt = model.BrickEntity.MaterialInt;
                existingBrickEntity.Price = model.BrickEntity.Price;
                existingBrickEntity.ProductionSize = model.BrickEntity.ProductionSize;
                existingBrickEntity.ReliefInt = model.BrickEntity.ReliefInt;
                existingBrickEntity.TransferFromOwner = model.BrickEntity.TransferFromOwner;
                existingBrickEntity.StartYear = model.BrickEntity.StartYear;
                existingBrickEntity.EndYear = model.BrickEntity.EndYear;
                existingBrickEntity.ExactYear = model.BrickEntity.ExactYear;
                existingBrickEntity.IsApproximate = model.BrickEntity.IsApproximate;

                unitOfWork.Save();

                SyncManufactoryCityConnections(existingBrickEntity, model.BrickEntityNManufactoryNCityList);
                SyncPersonConnections(existingBrickEntity, model.BrickEntityNPersonList);
                SyncCityConnections(existingBrickEntity, model.BrickEntityNCityList);
                SyncColorConnections(existingBrickEntity, model.ProductNColorVariantList);
                SyncPictureConnections(existingBrickEntity, model.ProductPictureList);
                scope.Complete();
                return (existingBrickEntity, 200, "Ziegel wurde aktualisiert.");
            }
            catch (Exception ex)
            {
                return (existingBrickEntity, 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }

        public (BrickEntity brickEntity, int statuscode, string message) Delete(BrickOperationParameterModel model)
        {
            BrickOperationParameterModel? operationParameterModel = GetWithPredicates(ParametersOperationToSearch(model)).FirstOrDefault();
            //BrickEntity? existingBrickEntity = unitOfWork.BrickEntityRepository.GetByID(model.BrickEntity.BrickEntityID);
            if (operationParameterModel == null)
            {
                return (new() { UsingIdentityUsersID = string.Empty}, 204, "Ziegel nicht gefunden");
            }

            try
            {
                using TransactionScope scope = new();

                for(int i = operationParameterModel.BrickEntityNManufactoryNCityList.Count; i > 0; i--)
                {
                    int index = i - 1;
                        DisconnectManufactoryConnection(operationParameterModel.BrickEntity, operationParameterModel.BrickEntityNManufactoryNCityList[index].ManufactoryID, operationParameterModel.BrickEntityNManufactoryNCityList[index].CityID);
                }
                for(int i = operationParameterModel.BrickEntityNPersonList.Count; i > 0; i--)
                {
                    int index = i - 1;
                    DisconnectPersonConnection(operationParameterModel.BrickEntity, operationParameterModel.BrickEntityNPersonList[index].PersonID);
                }
                for(int i = operationParameterModel.BrickEntityNCityList.Count; i > 0; i--)
                {
                    int index = i - 1;
                    DisconnectCityConnection(operationParameterModel.BrickEntity, operationParameterModel.BrickEntityNCityList[index].CityID);
                }
                for(int i = operationParameterModel.ProductNColorVariantList.Count; i > 0; i--)
                {
                    int index = i - 1;
                    DisconnectColorConnection(operationParameterModel.BrickEntity, operationParameterModel.ProductNColorVariantList[index].ColorID);
                }
                //DisconnectManufacturingDateConnection(operationParameterModel.ManufacturingDate);
                for(int i = operationParameterModel.ProductPictureList.Count; i > 0; i--)
                {
                    int index = i - 1;
                    (ProductPicture _, int _, string _) = processProductPicture.Delete(operationParameterModel.ProductPictureList[index]);
                }

                unitOfWork.BrickEntityRepository.Delete(operationParameterModel.BrickEntity);
                unitOfWork.Save();

                scope.Complete();
                return (operationParameterModel.BrickEntity, 200, "Ziegel wurde entfernt.");
            }
            catch (Exception ex)
            {
                return (new() { UsingIdentityUsersID = string.Empty }, 500, "Es ist ein Fehler aufgetreten: " + ex.Message + " " + ex.InnerException);
            }
        }

        public List<BrickOperationParameterModel> GetWithPredicates(BrickSearchParameterModel model)
        {
            IEnumerable<BrickEntity> brickIEnumberable = unitOfWork.BrickEntityRepository.Get(
                filter: SearchPredicateBuilder.BuildPredicate<BrickEntity>(model),
                includeProperties: "BrickPotential.BricknameSynonymList," +
                "ProductPictureList," +
                "UsingIdentityUser," +
                "BrickEntityNManufactoryNCityList.Manufactory," +
                "BrickEntityNManufactoryNCityList.City.CityNOeconymList.Oeconym," +
                "BrickEntityNManufactoryNCityList.Manufactory.CityList.CityNOeconymList.Oeconym," +
                "BrickEntityNManufactoryNCityList.City.Geography," +
                "BrickEntityNPersonList.Person," +
                "BrickEntityNCityList.City," +
                "ProcessOfManufacture," +
                "BrickEntityNCityList.City.CityNOeconymList.Oeconym," +
                "BrickEntityNCityList.City.Geography," +
                "BrickEntityNCityList.City.PostalcodeList," +
                "ProductNColorVariantList.Color");

            return [..from b in brickIEnumberable
                  select new BrickOperationParameterModel
                  {
                      BrickEntity = b,
                      BrickPotential = b.BrickPotential ?? new(),
                      ManufactoryTupleList = [
                            ..from connection in b.BrickEntityNManufactoryNCityList
                            let manufactory = connection.Manufactory ?? unitOfWork.ManufactoryRepository.Get(
                                filter: m => m.ManufactoryID == connection.ManufactoryID,
                                includeProperties: "CityList,CityList.CityNOeconymList.Oeconym,ProductionFacility"
                            ).FirstOrDefault() ?? new() { ManufactoryName = string.Empty }
                            select (manufactory, connection.CityID, manufactory.CityList)
                        ],
                      ProductPictureList = [.. b.ProductPictureList],
                      Brickname = b.BrickPotential?.BricknameSynonymList.FirstOrDefault() ?? new() {Name = string.Empty},
                      BrickEntityNPersonList = b.BrickEntityNPersonList,
                      BrickEntityNManufactoryNCityList = b.BrickEntityNManufactoryNCityList,
                      ManufactoryNCityList = [.. from bemc in b.BrickEntityNManufactoryNCityList
                                                 let manufactory = bemc.Manufactory ?? unitOfWork.ManufactoryRepository.Get(
                                filter: m => m.ManufactoryID == bemc.ManufactoryID,
                                includeProperties: "CityList.CityNOeconymList.Oeconym"
                            ).FirstOrDefault() ?? new() { ManufactoryName = string.Empty }
                            select new ManufactoryCityView
                            {
                                ManufactoryName = manufactory.ManufactoryName,
                                City = bemc.City?.CityNOeconymList.FirstOrDefault(x => x.CurrentName)?.Oeconym.OeconymName
                            }],
                      BrickEntityNCityList = b.BrickEntityNCityList,
                      ProcessOfManufacture = b.ProcessOfManufacture ?? new() { Mainprocess = string.Empty, ProcessOfManufactureName = string.Empty },
                      ProductNColorVariantList = b.ProductNColorVariantList,
                      ColorList = [.. unitOfWork.ColorRepository.Get()],
                  }];
        }

        //private BrickPotential? ConnectPotentialToEntity(int brickPotential_ID, BrickEntity brickEntity)
        //{
        //    BrickPotential? brickPotential = unitOfWork.BrickPotentialRepository.GetByID(brickPotential_ID);

        //    if (brickPotential == null)
        //    {
        //        return null;
        //    }

        //    unitOfWork.BrickEntityRepository.SetForeignKey(brickEntity, b => b.BrickPotentialID, brickPotential.BrickPotentialID);
        //    unitOfWork.BrickPotentialRepository.AddMemberToCollection(brickPotential, bp => bp.BrickEntityList, brickEntity);
        //    unitOfWork.Save();

        //    return brickPotential;
        //}
        //private void UpdatePotentialConnection(BrickEntity brickEntity, int newBrickPotentialId)
        //{
        //    if (brickEntity.BrickPotentialID != newBrickPotentialId)
        //    {
        //        DisconnectPotentialConnection(brickEntity);
        //        ConnectPotentialToEntity(newBrickPotentialId, brickEntity);
        //    }
        //}
        //private void DisconnectPotentialConnection(BrickEntity brickEntity)
        //{
        //    if (brickEntity.BrickPotentialID.HasValue)
        //    {
        //        BrickPotential? oldPotential = unitOfWork.BrickPotentialRepository.GetByID(brickEntity.BrickPotentialID.Value);
        //        if (oldPotential != null)
        //        {
        //            unitOfWork.BrickPotentialRepository.RemoveMemberFromCollection(oldPotential, bp => bp.BrickEntityList, brickEntity);
        //        }
        //    }
        //}
        private void ConnectManufactoryToBrickEntity(BrickEntity brickEntity, int manufactoryID, int? cityID)
        {
            if (manufactoryID <= 0) return;

            Manufactory? manufactory = unitOfWork.ManufactoryRepository.GetByID(manufactoryID);
            //City? city = cityID > 0 ? unitOfWork.CityRepository.GetByID(cityID) : null;

            if (manufactory == null) return;

            var entityNManufactoryNCity = new BrickEntityNManufactoryNCity
            {
                BrickEntityID = brickEntity.BrickEntityID,
                ManufactoryID = manufactory.ManufactoryID,
                CityID = cityID
                //BrickEntity = brickEntity,
                //Manufactory = manufactory,
                //City = city
            };

            unitOfWork.BrickEntityNManufactoryNCityRepository.Insert(entityNManufactoryNCity);
            unitOfWork.Save();
        }

        private void SyncManufactoryCityConnections(BrickEntity existingBrickEntity, List<BrickEntityNManufactoryNCity> newConnections)
        {
            var currentConnections = existingBrickEntity.BrickEntityNManufactoryNCityList.ToList();

            foreach (var current in currentConnections)
            {
                var updatedConnection = newConnections.FirstOrDefault(x => x.ManufactoryID == current.ManufactoryID);
                if (updatedConnection == null)
                {
                    DisconnectManufactoryConnection(existingBrickEntity, current.ManufactoryID, current.CityID);
                }
                else if (updatedConnection != null && updatedConnection.CityID != current.CityID)
                {
                    UpdateBrickEntityNManufactoryNCity(existingBrickEntity, current, updatedConnection.CityID);
                }
                // else: Beziehung ist gleich, keine Änderung notwendig
            }

            foreach (var newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.ManufactoryID == newItem.ManufactoryID && (x.CityID == newItem.CityID || x.CityID == newItem.City?.CityID));
                if (!exists)
                {
                    ConnectManufactoryToBrickEntity(existingBrickEntity, newItem.ManufactoryID, newItem.CityID);
                }
            }
        }
        private void UpdateBrickEntityNManufactoryNCity(BrickEntity existingBrickEntity, BrickEntityNManufactoryNCity currentConnection, int? cityID)
        {
            BrickEntityNManufactoryNCity? entityNManufactoryNCity = (from emc in unitOfWork.BrickEntityNManufactoryNCityRepository.Get()
                                                                     where emc.BrickEntity == existingBrickEntity && emc.ManufactoryID == currentConnection.ManufactoryID && emc.CityID == currentConnection.CityID
                                                                     select emc).FirstOrDefault();
            if (entityNManufactoryNCity != null)
            {
                unitOfWork.BrickEntityNManufactoryNCityRepository.SetForeignKey(entityNManufactoryNCity, x => x.CityID, cityID);
                unitOfWork.Save();
            }
        }
        private void DisconnectManufactoryConnection(BrickEntity brickEntity, int manufactoryID, int? cityID)
        {
            if (brickEntity.BrickEntityID > 0 && manufactoryID > 0)
            {
                BrickEntityNManufactoryNCity? entityNManufactoryNCity = (from emc in unitOfWork.BrickEntityNManufactoryNCityRepository.Get(includeProperties: "Manufactory,City")
                                                                        where emc.BrickEntity == brickEntity && emc.ManufactoryID == manufactoryID && emc.CityID == cityID
                                                                        select emc).FirstOrDefault();

                if (entityNManufactoryNCity != null)
                {
                    //unitOfWork.BrickEntityRepository.RemoveMemberFromCollection(brickEntity, be => be.BrickEntityNManufactoryNCityList, entityNManufactoryNCity);
                    //if (entityNManufactoryNCity.Manufactory is not null)
                    //{
                    //    unitOfWork.ManufactoryRepository.RemoveMemberFromCollection(entityNManufactoryNCity.Manufactory, p => p.BrickEntityNManufactoryNCityList, entityNManufactoryNCity);
                    //}
                    //if (cityID is not null && entityNManufactoryNCity.City != null)
                    //{
                    //    unitOfWork.CityRepository.RemoveMemberFromCollection(entityNManufactoryNCity.City, p => p.BrickEntityNManufactoryNCityList, entityNManufactoryNCity);
                    //}
                    //unitOfWork.Save();
                    unitOfWork.BrickEntityNManufactoryNCityRepository.Delete(entityNManufactoryNCity);
                    unitOfWork.Save();
                }
            }
        }

        private void ConnectPersonToBrickEntity(BrickEntity brickEntity, int personID, string? relationship)
        {
            if (personID <= 0) return;

            Person? person = unitOfWork.PersonRepository.GetByID(personID);
            if (person is null) return;

            BrickEntityNPerson brickEntityNPerson = new()
            {
                BrickEntityID = brickEntity.BrickEntityID,
                PersonID = personID,

                //BrickEntity = brickEntity,
                //Person = person,
                Relationship = relationship
            };
            unitOfWork.BrickEntityNPersonRepository.Insert(brickEntityNPerson);
            unitOfWork.Save();
        }
        private void SyncPersonConnections(BrickEntity existingBrickEntity, List<BrickEntityNPerson> newConnections)
        {
            var currentConnections = existingBrickEntity.BrickEntityNPersonList.ToList();

            foreach (var current in currentConnections)
            {
                var updated = newConnections.FirstOrDefault(x => x.PersonID == current.PersonID);

                if (updated == null)
                {
                    DisconnectPersonConnection(existingBrickEntity, current.PersonID);
                }
                else if (updated is not null && updated.Relationship != current.Relationship)
                {
                    UpdateBrickEntityNPerson(existingBrickEntity, updated);
                }
                // else: Beziehung ist gleich, keine Änderung notwendig
            }

            foreach (var newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.PersonID == newItem.PersonID);
                if (!exists)
                {
                    ConnectPersonToBrickEntity(existingBrickEntity, newItem.PersonID, newItem.Relationship);
                }
            }
        }
        private void UpdateBrickEntityNPerson(BrickEntity existingBrickEntity, BrickEntityNPerson updated)
        {
            BrickEntityNPerson? brickEntityNPerson = (from bep in unitOfWork.BrickEntityNPersonRepository.Get(includeProperties: "Person")
                                                      where bep.PersonID == updated.PersonID && bep.BrickEntity == existingBrickEntity
                                                      select bep).FirstOrDefault();
            if (brickEntityNPerson != null)
            {
                brickEntityNPerson.Relationship = updated.Relationship;
                //unitOfWork.BrickEntityNPersonRepository.Update(brickEntityNPerson);
                unitOfWork.Save();
            }
        }
        private void DisconnectPersonConnection(BrickEntity brickEntity, int personID)
        {
            if (brickEntity.BrickEntityID > 0 && personID > 0)
            {
                BrickEntityNPerson? brickEntityNPerson = (from bep in unitOfWork.BrickEntityNPersonRepository.Get(includeProperties: "Person")
                                                         where bep.PersonID == personID && bep.BrickEntity == brickEntity
                                                         select bep).FirstOrDefault();

                if (brickEntityNPerson != null)
                {
                    //unitOfWork.BrickEntityRepository.RemoveMemberFromCollection(brickEntity, be => be.BrickEntityNPersonList, brickEntityNPerson);
                    //if (brickEntityNPerson.Person != null)
                    //{
                    //    unitOfWork.PersonRepository.RemoveMemberFromCollection(brickEntityNPerson.Person, p => p.BrickEntityNPersonList, brickEntityNPerson);
                    //}
                    //unitOfWork.Save();
                    unitOfWork.BrickEntityNPersonRepository.Delete(brickEntityNPerson);
                    unitOfWork.Save();
                }
            }
        }

        private void ConnectCityToBrickEntity(BrickEntity brickEntity, int cityID, string? relationship)
        {
            if (cityID <= 0) return;

            City? city = unitOfWork.CityRepository.GetByID(cityID);
            if (city is null) return;

            BrickEntityNCity brickEntityNCity = new()
            {
                BrickEntityID = brickEntity.BrickEntityID,
                CityID = cityID,
                Relationship = relationship
            };
            unitOfWork.BrickEntityNCityRepository.Insert(brickEntityNCity);
            unitOfWork.Save();
        }
        private void SyncCityConnections(BrickEntity existingBrickEntity, List<BrickEntityNCity> newConnections)
        {
            var currentConnections = existingBrickEntity.BrickEntityNCityList.ToList();

            foreach (var current in currentConnections)
            {
                var updated = newConnections.FirstOrDefault(x => x.CityID == current.CityID);

                if (updated == null)
                {
                    DisconnectCityConnection(existingBrickEntity, current.CityID);
                }
                else if (updated is not null && updated.Relationship != current.Relationship)
                {
                    UpdateBrickEntityNCity(existingBrickEntity, updated);
                }
            }

            foreach (var newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.CityID == newItem.CityID);
                if (!exists)
                {
                    ConnectCityToBrickEntity(existingBrickEntity, newItem.CityID, newItem.Relationship);
                }
            }
        }
        private void UpdateBrickEntityNCity(BrickEntity existingBrickEntity, BrickEntityNCity updated)
        {
            BrickEntityNCity? brickEntityNCity = (from bec in unitOfWork.BrickEntityNCityRepository.Get(includeProperties: "City")
                                                      where bec.CityID == updated.CityID && bec.BrickEntity == existingBrickEntity
                                                      select bec).FirstOrDefault();
            if (brickEntityNCity != null)
            {
                brickEntityNCity.Relationship = updated.Relationship;
                unitOfWork.Save();
            }
        }
        private void DisconnectCityConnection(BrickEntity brickEntity, int cityID)
        {
            if (brickEntity.BrickEntityID > 0 && cityID > 0)
            {
                BrickEntityNCity? brickEntityNCity = (from bec in unitOfWork.BrickEntityNCityRepository.Get(includeProperties: "City")
                                                         where bec.CityID == cityID && bec.BrickEntity == brickEntity
                                                         select bec).FirstOrDefault();

                if (brickEntityNCity != null)
                {
                    unitOfWork.BrickEntityNCityRepository.Delete(brickEntityNCity);
                    unitOfWork.Save();
                }
            }
        }

        private void ConnectColorToBrickEntity(BrickEntity brickEntity, ProductNColorVariant productNColorVariant)
        {
            if (productNColorVariant.ColorID <= 0) return;
            Color? color = unitOfWork.ColorRepository.GetByID(productNColorVariant.ColorID);
            if (color is null) return;

            productNColorVariant.BrickEntityID = brickEntity.BrickEntityID;

            unitOfWork.ProductNColorVariantRepository.Insert(productNColorVariant);
            unitOfWork.Save();
        }
        private void SyncColorConnections(BrickEntity existingBrickEntity, List<ProductNColorVariant> newConnections)
        {
            var currentConnections = existingBrickEntity.ProductNColorVariantList.ToList();

            foreach (var current in currentConnections)
            {
                var updated = newConnections.FirstOrDefault(x => x.ColorID == current.ColorID);

                if (updated == null)
                {
                    DisconnectColorConnection(existingBrickEntity, current.ColorID);
                }
                else if (updated is not null && (updated.IsPrimaryColor != current.IsPrimaryColor || updated.Note != current.Note))
                {
                    UpdateProductNColorVariant(existingBrickEntity, updated);
                }
            }

            foreach (var newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.ColorID == newItem.ColorID);
                if (!exists)
                {
                    ConnectColorToBrickEntity(existingBrickEntity, newItem);
                }
            }
        }
        private void UpdateProductNColorVariant(BrickEntity existingBrickEntity, ProductNColorVariant updated)
        {
            ProductNColorVariant? productNColorVariant = (from pnc in unitOfWork.ProductNColorVariantRepository.Get(includeProperties: "Color")
                                                              where pnc.ColorID == updated.ColorID && pnc.BrickEntity == existingBrickEntity
                                                              select pnc).FirstOrDefault();

            if (productNColorVariant != null)
            {
                productNColorVariant.IsPrimaryColor = updated.IsPrimaryColor;
                productNColorVariant.Note = updated.Note;
                unitOfWork.Save();
            }
        }
        private void DisconnectColorConnection(BrickEntity brickEntity, int colorID)
        {
            if (brickEntity.BrickEntityID > 0 && colorID > 0)
            {
                ProductNColorVariant? productNColorVariant = (from pnc in unitOfWork.ProductNColorVariantRepository.Get(includeProperties: "Color")
                                                            where pnc.ColorID == colorID && pnc.BrickEntity == brickEntity
                                                            select pnc).FirstOrDefault();

                if (productNColorVariant != null)
                {
                    unitOfWork.ProductNColorVariantRepository.Delete(productNColorVariant);
                    unitOfWork.Save();
                }
            }
        }

        //private (BrickEntity brickEntity, int statuscode, string message) ConnectPictureToBrickEntity(BrickEntity brickEntity, ProductPicture productPicture)
        //{
        //    if (brickEntity.UsingIdentityUser is null)
        //    {
        //        return (brickEntity, 302, "User fehlt.");
        //    }
        //    (ProductPicture newProductPicture, int _, string _) = processProductPicture.Create(productPicture, brickEntity.UsingIdentityUser.UserName);

        //    //unitOfWork.BrickEntityRepository.AddMemberToCollection(brickEntity, b => b.ProductPictureList, newProductPicture);
        //    unitOfWork.ProductPictureRepository.SetForeignKey(newProductPicture, p => p.BrickEntityID, brickEntity.BrickEntityID);
        //    //unitOfWork.Save();

        //    return (brickEntity, 201, "Bilder erstellt.");
        //}
        private void SyncPictureConnections(BrickEntity existingBrickEntity, List<ProductPicture> newConnections)
        {
            var currentConnections = existingBrickEntity.ProductPictureList.ToList();

            foreach (var current in currentConnections)
            {
                var updated = newConnections.FirstOrDefault(x => x.ProductPictureID == current.ProductPictureID);

                if (updated == null)
                {
                    (ProductPicture _, int _, string _) = processProductPicture.Delete(current);
                }
                else if (updated != null)
                {
                    (ProductPicture _, int _, string _) = processProductPicture.Edit(updated, existingBrickEntity);
                }
            }

            foreach (var newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.ProductPictureID == newItem.ProductPictureID);
                if (!exists)
                {
                    //ConnectPictureToBrickEntity(existingBrickEntity, newItem);
                    (ProductPicture _, int _, string _) = processProductPicture.Create(newItem, existingBrickEntity);
                }
            }
        }
        //private void DisconnectPictureConnection(ProductPicture? productPicture)
        //{
        //    //productPicture = unitOfWork.ProductPictureRepository.GetByID(productPicture.ProductPictureID);
        //    if (productPicture != null)
        //    {
        //        //unitOfWork.BrickEntityRepository.RemoveMemberFromCollection(brickEntity, be => be.ProductPictureList, productPicture);
        //        //unitOfWork.Save();
        //        unitOfWork.ProductPictureRepository.Delete(productPicture);
        //        unitOfWork.Save();
        //    }
        //}

        //private (BrickEntity brickEntity, int statuscode, string message) ConnectManufacturingDateToBrickEntity(ManufacturingDate manufacturingDate, BrickEntity brickEntity)
        //{
        //    manufacturingDate = unitOfWork.ManufactoringDateRepository.Insert(manufacturingDate);

        //    unitOfWork.BrickEntityRepository.SetForeignKey(brickEntity, b => b.ManufacturingDate_ID, manufacturingDate.ManufacturingDate_ID);
        //    //unitOfWork.ManufactoringDateRepository.AddMemberToCollection(manufacturingDate, p => p.BrickEntityICollection, brickEntity);
        //    unitOfWork.Save();

        //    return (brickEntity, 201, "Bilder erstellt.");
        //}
        //private void UpdateManufacturingDateConnection(BrickEntity brickEntity, ManufacturingDate manufacturingDate)
        //{
        //    if (brickEntity.ManufacturingDate != null && manufacturingDate.ManufacturingDate_ID > 0 &&
        //        (brickEntity.ManufacturingDate.EndYear != manufacturingDate.EndYear ||
        //        brickEntity.ManufacturingDate.ExactYear != manufacturingDate.ExactYear ||
        //        brickEntity.ManufacturingDate.IsApproximate != manufacturingDate.IsApproximate ||
        //        brickEntity.ManufacturingDate.Note != manufacturingDate.Note||
        //        brickEntity.ManufacturingDate.StartYear != manufacturingDate.StartYear))
        //    {
        //        ManufacturingDate? existingManufactory = unitOfWork.ManufactoringDateRepository.GetByID(manufacturingDate.ManufacturingDate_ID);

        //        if (existingManufactory != null)
        //        {
        //            existingManufactory.EndYear = manufacturingDate.EndYear;
        //            existingManufactory.ExactYear = manufacturingDate.ExactYear;
        //            existingManufactory.IsApproximate = manufacturingDate.IsApproximate;
        //            existingManufactory.Note = manufacturingDate.Note;
        //            existingManufactory.StartYear = manufacturingDate.StartYear;

        //            unitOfWork.Save();
        //        }
        //    }
        //}
        //private void UpdateManufacturingDate(ManufacturingDate manufacturingDate)
        //{
        //    if (manufacturingDate.ManufacturingDate_ID > 0)
        //    {
        //        unitOfWork.ManufactoringDateRepository.Update(manufacturingDate);
        //        unitOfWork.Save();
        //    }
        //}
        //private void DisconnectManufacturingDateConnection(ManufacturingDate manufacturingDate)
        //{
        //    if (manufacturingDate.ManufacturingDate_ID > 0)
        //    {
        //        //ManufacturingDate? manufacturing = unitOfWork.ManufactoringDateRepository.GetByID(manufacturingDate.ManufacturingDate_ID);
        //        //if (manufacturingDate != null)
        //        //{
        //            //unitOfWork.ManufactoringDateRepository.RemoveMemberFromCollection(manufacturingDate, be => be.BrickEntityICollection, brickEntity);
        //            //unitOfWork.Save();
        //            unitOfWork.ManufactoringDateRepository.Delete(manufacturingDate);
        //            unitOfWork.Save();
        //        //}
        //    }
        //}

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
                    textPosition.Height = int.Parse(textPositionMembers[1]);
                }
                if (!string.IsNullOrEmpty(textPositionMembers[2]))
                {
                    textPosition.XPosition = int.Parse(textPositionMembers[2]);
                }
                if (!string.IsNullOrEmpty(textPositionMembers[3]))
                {
                    textPosition.YPosition = int.Parse(textPositionMembers[3]);
                }

                textPositionList.Add(textPosition);
            };

            string jsonSerializer = JsonSerializer.Serialize<List<TextPosition>>(textPositionList);
            productPicture.TextPositionJson = jsonSerializer;
        }
    }
}
