using Sammlerplattform.Data;
using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.CityDatabase;
using Sammlerplattform.Models.ManufactoryDatabase;
using Sammlerplattform.Models.PartyDatabase;
using Sammlerplattform.Models.PersonDatabase;
using Sammlerplattform.Models.ProductDatabase;
using Sammlerplattform.Models.ProductPictureDatabase;
using System.Transactions;

namespace Sammlerplattform.Services.Processes
{
    public interface IProcessProduct
    {
        List<BrickOperationParameterModel> GetWithPredicates(BrickSearchParameterModel model);
        (BrickEntity brickEntity, int statuscode, string message) Create(BrickOperationParameterModel model);
        (BrickEntity brickEntity, int statuscode, string message) Edit(BrickOperationParameterModel model);
        (BrickEntity brickEntity, int statuscode, string message) Delete(BrickOperationParameterModel model);
        BrickSearchParameterModel ParametersOperationToSearch(BrickOperationParameterModel model);
    }

    public class ProductProcessor(IUnitOfWork unitOfWork, IProcessProductPicture processProductPicture) : IProcessProduct
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

                //foreach (BrickEntityNManufactoryNCity entityNManufactoryNCity in model.BrickEntityNManufactoryNCityList)
                //{
                //    ConnectManufactoryToBrickEntity(newBrickEntity, entityNManufactoryNCity.ManufactoryID, entityNManufactoryNCity.CityID);
                //}
                //foreach (BrickEntityNPerson entityNPerson in model.BrickEntityNPersonList)
                //{
                //    ConnectPersonToBrickEntity(newBrickEntity, entityNPerson.PersonID, entityNPerson.Relationship);
                //}
                //foreach (BrickEntityNCity brickEntityNCity in model.BrickEntityNCityList)
                //{
                //    ConnectCityToBrickEntity(newBrickEntity, brickEntityNCity.CityID, brickEntityNCity.Relationship);
                //}
                foreach (ProductNColorVariant productNColorVariant in model.ProductNColorVariantList)
                {
                    ConnectColorToBrickEntity(newBrickEntity, productNColorVariant);
                }
                foreach (ProductNMaterial productNMaterial in model.ProductNMaterialList)
                {
                    ConnectMaterialToProductEntity(newBrickEntity, productNMaterial);
                }
                foreach (ProductNKeyword productNKeyword in model.ProductNKeywordList)
                {
                    ConnectKeywordToProductEntity(newBrickEntity, productNKeyword);
                }
                foreach (ProductPicture productPicture in model.ProductPictureList)
                {
                    _ = processProductPicture.Create(productPicture, newBrickEntity);
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
                existingBrickEntity.ConditionID = model.BrickEntity.ConditionID;
                existingBrickEntity.Fake = model.BrickEntity.Fake;
                existingBrickEntity.FilingLocation = model.BrickEntity.FilingLocation;
                existingBrickEntity.DeliveryPrice = model.BrickEntity.DeliveryPrice;
                existingBrickEntity.ProductionSize = model.BrickEntity.ProductionSize;
                existingBrickEntity.TransferFromOwner = model.BrickEntity.TransferFromOwner;
                existingBrickEntity.StartYear = model.BrickEntity.StartYear;
                existingBrickEntity.EndYear = model.BrickEntity.EndYear;
                existingBrickEntity.ExactYear = model.BrickEntity.ExactYear;
                existingBrickEntity.IsApproximate = model.BrickEntity.IsApproximate;

                unitOfWork.Save();

                //SyncManufactoryCityConnections(existingBrickEntity, model.BrickEntityNManufactoryNCityList);
                //SyncPersonConnections(existingBrickEntity, model.BrickEntityNPersonList);
                //SyncCityConnections(existingBrickEntity, model.BrickEntityNCityList);
                SyncColorConnections(existingBrickEntity, model.ProductNColorVariantList);
                SyncMaterialConnections(existingBrickEntity, model.ProductNMaterialList);
                SyncKeywordConnections(existingBrickEntity, model.ProductNKeywordList);
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
            if (operationParameterModel == null)
            {
                return (new() { UsingIdentityUsersID = string.Empty }, 204, "Ziegel nicht gefunden");
            }

            try
            {
                using TransactionScope scope = new();

                //for (int i = operationParameterModel.BrickEntityNManufactoryNCityList.Count; i > 0; i--)
                //{
                //    int index = i - 1;
                //    DisconnectManufactoryConnection(operationParameterModel.BrickEntity, operationParameterModel.BrickEntityNManufactoryNCityList[index].ManufactoryID, operationParameterModel.BrickEntityNManufactoryNCityList[index].CityID);
                //}
                //for (int i = operationParameterModel.BrickEntityNPersonList.Count; i > 0; i--)
                //{
                //    int index = i - 1;
                //    DisconnectPersonConnection(operationParameterModel.BrickEntity, operationParameterModel.BrickEntityNPersonList[index].PersonID);
                //}
                //for (int i = operationParameterModel.BrickEntityNCityList.Count; i > 0; i--)
                //{
                //    int index = i - 1;
                //    DisconnectCityConnection(operationParameterModel.BrickEntity, operationParameterModel.BrickEntityNCityList[index].CityID);
                //}
                for (int i = operationParameterModel.ProductNColorVariantList.Count; i > 0; i--)
                {
                    int index = i - 1;
                    DisconnectColorConnection(operationParameterModel.BrickEntity, operationParameterModel.ProductNColorVariantList[index].ColorID);
                }
                for (int i = operationParameterModel.ProductNMaterialList.Count; i > 0; i--)
                {
                    int index = i - 1;
                    DisconnectMaterialConnection(operationParameterModel.BrickEntity, operationParameterModel.ProductNMaterialList[index].MaterialID);
                }
                for (int i = operationParameterModel.ProductNKeywordList.Count; i > 0; i--)
                {
                    int index = i - 1;
                    DisconnectKeywordConnection(operationParameterModel.BrickEntity, operationParameterModel.ProductNKeywordList[index].KeywordID);
                }
                for (int i = operationParameterModel.ProductPictureList.Count; i > 0; i--)
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
                //"BrickEntityNManufactoryNCityList.Manufactory," +
                //"BrickEntityNManufactoryNCityList.City.CityOeconymList.Oeconym," +
                //"BrickEntityNManufactoryNCityList.Manufactory.CityList.CityOeconymList.Oeconym," +
                //"BrickEntityNManufactoryNCityList.City.Geography," +
                //"BrickEntityNPersonList.Person," +
                //"BrickEntityNCityList.City," +
                "ProcessOfManufacture," +
                //"BrickEntityNCityList.City.CityOeconymList.Oeconym," +
                //"BrickEntityNCityList.City.Geography," +
                //"BrickEntityNCityList.City.CityPostalcodeList," +
                "ProductNColorVariantList.Color,");

            return [..from b in brickIEnumberable
                  select new BrickOperationParameterModel
                  {
                      BrickEntity = b,
                      BrickPotential = b.BrickPotential ?? new(),
                      //ManufactoryTupleList = [
                      //      ..from connection in b.BrickEntityNManufactoryNCityList
                      //      let manufactory = connection.Manufactory ?? unitOfWork.ManufactoryRepository.Get(
                      //          filter: m => m.ManufactoryID == connection.ManufactoryID,
                      //          includeProperties: "CityList,CityList.CityOeconymList.Oeconym,ProductionFacility"
                      //      ).FirstOrDefault() ?? new() { ManufactoryName = string.Empty }
                      //      select (manufactory, connection.CityID, manufactory.CityList)
                      //  ],
                      ProductPictureList = [.. b.ProductPictureList],
                      Brickname = b.BrickPotential?.BricknameSynonymList.FirstOrDefault() ?? new() {Name = string.Empty},
                      //BrickEntityNPersonList = b.BrickEntityNPersonList,
                      //BrickEntityNManufactoryNCityList = b.BrickEntityNManufactoryNCityList,
                      //ManufactoryNCityList = [.. from bemc in b.BrickEntityNManufactoryNCityList
                      //                           let manufactory = bemc.Manufactory ?? unitOfWork.ManufactoryRepository.Get(
                      //          filter: m => m.ManufactoryID == bemc.ManufactoryID,
                      //          includeProperties: "CityList.CityOeconymList.Oeconym"
                      //      ).FirstOrDefault() ?? new() { ManufactoryName = string.Empty }
                      //      select new ManufactoryCityView
                      //      {
                      //          ManufactoryName = manufactory.ManufactoryName,
                      //          City = bemc.City?.CityOeconymList.FirstOrDefault(x => x.CurrentName)?.Oeconym.OeconymName
                      //      }],
                      //BrickEntityNCityList = b.BrickEntityNCityList,
                      ProcessOfManufacture = b.ProcessOfManufacture ?? new() { Mainprocess = string.Empty, ProcessOfManufactureName = string.Empty },
                      ProductNColorVariantList = b.ProductNColorVariantList,
                      ColorList = [.. unitOfWork.ColorRepository.Get()],
                      ConditionList = [.. unitOfWork.ConditionRepository.Get()],
                  }];
        }

        private void ConnectManufactoryToBrickEntity(BrickEntity brickEntity, int manufactoryID, int? cityID)
        {
            if (manufactoryID <= 0)
            {
                return;
            }

            Manufactory? manufactory = unitOfWork.ManufactoryRepository.GetByID(manufactoryID);
            if (manufactory == null)
            {
                return;
            }

            BrickEntityNManufactoryNCity entityNManufactoryNCity = new()
            {
                BrickEntityID = brickEntity.BrickEntityID,
                ManufactoryID = manufactory.ManufactoryID,
                CityID = cityID
            };

            _ = unitOfWork.BrickEntityNManufactoryNCityRepository.Insert(entityNManufactoryNCity);
            unitOfWork.Save();
        }
        private void SyncManufactoryCityConnections(BrickEntity existingBrickEntity, List<BrickEntityNManufactoryNCity> newConnections)
        {
            List<BrickEntityNManufactoryNCity> currentConnections = existingBrickEntity.BrickEntityNManufactoryNCityList;

            foreach (BrickEntityNManufactoryNCity? current in currentConnections)
            {
                BrickEntityNManufactoryNCity? updatedConnection = newConnections.FirstOrDefault(x => x.ManufactoryID == current.ManufactoryID);
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

            foreach (BrickEntityNManufactoryNCity newItem in newConnections)
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
                    unitOfWork.BrickEntityNManufactoryNCityRepository.Delete(entityNManufactoryNCity);
                    unitOfWork.Save();
                }
            }
        }

        //private void ConnectPersonToBrickEntity(BrickEntity brickEntity, int personID, string? relationship)
        //{
        //    if (personID <= 0)
        //    {
        //        return;
        //    }

        //    Person? person = unitOfWork.PersonRepository.GetByID(personID);
        //    if (person is null)
        //    {
        //        return;
        //    }

        //    BrickEntityNPerson brickEntityNPerson = new()
        //    {
        //        BrickEntityID = brickEntity.BrickEntityID,
        //        PersonID = personID,
        //        Relationship = relationship
        //    };
        //    _ = unitOfWork.BrickEntityNPersonRepository.Insert(brickEntityNPerson);
        //    unitOfWork.Save();
        //}
        //private void SyncPersonConnections(BrickEntity existingBrickEntity, List<BrickEntityNPerson> newConnections)
        //{
        //    List<BrickEntityNPerson> currentConnections = existingBrickEntity.BrickEntityNPersonList;

        //    foreach (BrickEntityNPerson? current in currentConnections)
        //    {
        //        BrickEntityNPerson? updated = newConnections.FirstOrDefault(x => x.PersonID == current.PersonID);

        //        if (updated == null)
        //        {
        //            DisconnectPersonConnection(existingBrickEntity, current.PersonID);
        //        }
        //        else if (updated is not null && updated.Relationship != current.Relationship)
        //        {
        //            UpdateBrickEntityNPerson(existingBrickEntity, updated);
        //        }
        //    }

        //    foreach (BrickEntityNPerson newItem in newConnections)
        //    {
        //        bool exists = currentConnections.Any(x => x.PersonID == newItem.PersonID);
        //        if (!exists)
        //        {
        //            ConnectPersonToBrickEntity(existingBrickEntity, newItem.PersonID, newItem.Relationship);
        //        }
        //    }
        //}
        //private void UpdateBrickEntityNPerson(BrickEntity existingBrickEntity, BrickEntityNPerson updated)
        //{
        //    BrickEntityNPerson? brickEntityNPerson = (from bep in unitOfWork.BrickEntityNPersonRepository.Get(includeProperties: "Person")
        //                                              where bep.PersonID == updated.PersonID && bep.BrickEntity == existingBrickEntity
        //                                              select bep).FirstOrDefault();
        //    if (brickEntityNPerson != null)
        //    {
        //        brickEntityNPerson.Relationship = updated.Relationship;
        //        unitOfWork.Save();
        //    }
        //}
        //private void DisconnectPersonConnection(BrickEntity brickEntity, int personID)
        //{
        //    if (brickEntity.BrickEntityID > 0 && personID > 0)
        //    {
        //        BrickEntityNPerson? brickEntityNPerson = (from bep in unitOfWork.BrickEntityNPersonRepository.Get(includeProperties: "Person")
        //                                                  where bep.PersonID == personID && bep.BrickEntity == brickEntity
        //                                                  select bep).FirstOrDefault();

        //        if (brickEntityNPerson != null)
        //        {
        //            unitOfWork.BrickEntityNPersonRepository.Delete(brickEntityNPerson);
        //            unitOfWork.Save();
        //        }
        //    }
        //}
        //private void ConnectPartyToBrickEntity(BrickEntity brickEntity, int partyID, string? relationship)
        //{
        //    if (partyID <= 0)
        //    {
        //        return;
        //    }
        //    Party? party = unitOfWork.PartyRepository.GetByID(partyID);
        //    if (party is null)
        //    {
        //        return;
        //    }

        //    ProductEntityNParty productEntityNParty = new()
        //    {
        //        ProductEntityID = brickEntity.BrickEntityID,
        //        PartyID = partyID,
        //        Relationship = relationship
        //    };
        //    _ = unitOfWork.ProductEntityNPartyRepository.Insert(productEntityNParty);
        //    unitOfWork.Save();
        //}
        //private void SyncPartyConnections(BrickEntity existingBrickEntity, List<BrickEntityNPerson> newConnections)
        //{
        //    List<BrickEntityNPerson> currentConnections = existingBrickEntity.BrickEntityNPersonList;

        //    foreach (BrickEntityNPerson? current in currentConnections)
        //    {
        //        BrickEntityNPerson? updated = newConnections.FirstOrDefault(x => x.PersonID == current.PersonID);

        //        if (updated == null)
        //        {
        //            DisconnectPersonConnection(existingBrickEntity, current.PersonID);
        //        }
        //        else if (updated is not null && updated.Relationship != current.Relationship)
        //        {
        //            UpdateBrickEntityNPerson(existingBrickEntity, updated);
        //        }
        //    }

        //    foreach (BrickEntityNPerson newItem in newConnections)
        //    {
        //        bool exists = currentConnections.Any(x => x.PersonID == newItem.PersonID);
        //        if (!exists)
        //        {
        //            ConnectPersonToBrickEntity(existingBrickEntity, newItem.PersonID, newItem.Relationship);
        //        }
        //    }
        //}
        //private void UpdateProductEntityNPerson(ProductEntity existingPrductEntity, ProductEntityNParty updated)
        //{
        //    ProductEntityNParty productEntityNParty = (from bep in unitOfWork.ProductEntityNPartyRepository.Get(includeProperties: "Party")
        //                                          where bep.PartyID == updated.PartyID && bep.ProductEntity == existingPrductEntity
        //                                          select bep).FirstOrDefault();

        //    if (brickEntityNPerson != null)
        //    {
        //        brickEntityNPerson.Relationship = updated.Relationship;
        //        unitOfWork.Save();
        //    }
        //}
        //private void DisconnectPersonConnection(BrickEntity brickEntity, int personID)
        //{
        //    if (brickEntity.BrickEntityID > 0 && personID > 0)
        //    {
        //        BrickEntityNPerson? brickEntityNPerson = (from bep in unitOfWork.BrickEntityNPersonRepository.Get(includeProperties: "Person")
        //                                                  where bep.PersonID == personID && bep.BrickEntity == brickEntity
        //                                                  select bep).FirstOrDefault();

        //        if (brickEntityNPerson != null)
        //        {
        //            unitOfWork.BrickEntityNPersonRepository.Delete(brickEntityNPerson);
        //            unitOfWork.Save();
        //        }
        //    }
        //}

        private void ConnectCityToBrickEntity(BrickEntity brickEntity, int cityID, string? relationship)
        {
            if (cityID <= 0)
            {
                return;
            }

            City? city = unitOfWork.CityRepository.GetByID(cityID);
            if (city is null)
            {
                return;
            }

            BrickEntityNCity brickEntityNCity = new()
            {
                BrickEntityID = brickEntity.BrickEntityID,
                CityID = cityID,
                Relationship = relationship
            };
            _ = unitOfWork.BrickEntityNCityRepository.Insert(brickEntityNCity);
            unitOfWork.Save();
        }
        private void SyncCityConnections(BrickEntity existingBrickEntity, List<BrickEntityNCity> newConnections)
        {
            List<BrickEntityNCity> currentConnections = existingBrickEntity.BrickEntityNCityList;

            foreach (BrickEntityNCity? current in currentConnections)
            {
                BrickEntityNCity? updated = newConnections.FirstOrDefault(x => x.CityID == current.CityID);

                if (updated == null)
                {
                    DisconnectCityConnection(existingBrickEntity, current.CityID);
                }
                else if (updated is not null && updated.Relationship != current.Relationship)
                {
                    UpdateBrickEntityNCity(existingBrickEntity, updated);
                }
            }

            foreach (BrickEntityNCity newItem in newConnections)
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
            if (productNColorVariant.ColorID <= 0)
            {
                return;
            }

            Color? color = unitOfWork.ColorRepository.GetByID(productNColorVariant.ColorID);
            if (color is null)
            {
                return;
            }

            productNColorVariant.BrickEntityID = brickEntity.BrickEntityID;

            _ = unitOfWork.ProductNColorVariantRepository.Insert(productNColorVariant);
            unitOfWork.Save();
        }
        private void SyncColorConnections(BrickEntity existingBrickEntity, List<ProductNColorVariant> newConnections)
        {
            List<ProductNColorVariant> currentConnections = existingBrickEntity.ProductNColorVariantList;

            foreach (ProductNColorVariant? current in currentConnections)
            {
                ProductNColorVariant? updated = newConnections.FirstOrDefault(x => x.ColorID == current.ColorID);

                if (updated == null)
                {
                    DisconnectColorConnection(existingBrickEntity, current.ColorID);
                }
                else if (updated is not null && (updated.IsPrimaryColor != current.IsPrimaryColor || updated.Note != current.Note))
                {
                    UpdateProductNColorVariant(existingBrickEntity, updated);
                }
            }

            foreach (ProductNColorVariant newItem in newConnections)
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

        private void ConnectMaterialToProductEntity(BrickEntity brickEntity, ProductNMaterial productNMatreial)
        {
            if (productNMatreial.MaterialID <= 0)
            {
                return;
            }

            Material? material = unitOfWork.MaterialRepository.GetByID(productNMatreial.MaterialID);
            if (material is null)
            {
                return;
            }

            productNMatreial.BrickEntityID = brickEntity.BrickEntityID;

            _ = unitOfWork.ProductNMaterialRepository.Insert(productNMatreial);
            unitOfWork.Save();
        }
        private void SyncMaterialConnections(BrickEntity existingProductEntity, List<ProductNMaterial> newConnections)
        {
            List<ProductNMaterial> currentConnections = existingProductEntity.ProductNMaterialList;

            foreach (ProductNMaterial? current in currentConnections)
            {
                ProductNMaterial? updated = newConnections.FirstOrDefault(x => x.MaterialID == current.MaterialID);

                if (updated == null)
                {
                    DisconnectMaterialConnection(existingProductEntity, current.MaterialID);
                }
                else if (updated is not null && (updated.IsPrimaryMaterial != current.IsPrimaryMaterial))
                {
                    UpdateProductNMaterial(existingProductEntity, updated);
                }
            }

            foreach (ProductNMaterial newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.MaterialID == newItem.MaterialID);
                if (!exists)
                {
                    ConnectMaterialToProductEntity(existingProductEntity, newItem);
                }
            }
        }
        private void UpdateProductNMaterial(BrickEntity existingBrickEntity, ProductNMaterial updated)
        {
            ProductNMaterial? productNMaterial = (from pnm in unitOfWork.ProductNMaterialRepository.Get(includeProperties: "Material")
                                                          where pnm.MaterialID == updated.MaterialID && pnm.BrickEntity == existingBrickEntity
                                                          select pnm).FirstOrDefault();

            if (productNMaterial != null)
            {
                productNMaterial.IsPrimaryMaterial = updated.IsPrimaryMaterial;
                unitOfWork.Save();
            }
        }
        private void DisconnectMaterialConnection(BrickEntity brickEntity, int materialID)
        {
            if (brickEntity.BrickEntityID > 0 && materialID > 0)
            {
                ProductNMaterial? productNMaterial = (from pnm in unitOfWork.ProductNMaterialRepository.Get(includeProperties: "Material")
                                                              where pnm.MaterialID == materialID && pnm.BrickEntity == brickEntity
                                                              select pnm).FirstOrDefault();

                if (productNMaterial != null)
                {
                    unitOfWork.ProductNMaterialRepository.Delete(productNMaterial);
                    unitOfWork.Save();
                }
            }
        }

        private void ConnectKeywordToProductEntity(BrickEntity brickEntity, ProductNKeyword productNKeyword)
        {
            if (productNKeyword.KeywordID <= 0)
            {
                return;
            }
            Keyword? keyword = unitOfWork.KeywordRepository.GetByID(productNKeyword.KeywordID);
            if (keyword is null)
            {
                return;
            }

            productNKeyword.BrickEntityID = brickEntity.BrickEntityID;

            _ = unitOfWork.ProductNKeywordRepository.Insert(productNKeyword);
            unitOfWork.Save();
        }
        private void SyncKeywordConnections(BrickEntity existingBrickEntity, List<ProductNKeyword> newConnections)
        {
            List<ProductNKeyword> currentConnections = existingBrickEntity.ProductNKeywordList;

            foreach (ProductNKeyword? current in currentConnections)
            {
                ProductNKeyword? updated = newConnections.FirstOrDefault(x => x.KeywordID == current.KeywordID);

                if (updated == null)
                {
                    DisconnectKeywordConnection(existingBrickEntity, current.KeywordID);
                }
            }

            foreach (ProductNKeyword newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.KeywordID == newItem.KeywordID);
                if (!exists)
                {
                    ConnectKeywordToProductEntity(existingBrickEntity, newItem);
                }
            }
        }
        private void DisconnectKeywordConnection(BrickEntity brickEntity, int keywordID)
        {
            if (brickEntity.BrickEntityID > 0 && keywordID > 0)
            {
                ProductNKeyword? productNKeyword = (from pnk in unitOfWork.ProductNKeywordRepository.Get(includeProperties: "Keyword")
                                                              where pnk.KeywordID == keywordID && pnk.BrickEntity == brickEntity
                                                              select pnk).FirstOrDefault();

                if (productNKeyword != null)
                {
                    unitOfWork.ProductNKeywordRepository.Delete(productNKeyword);
                    unitOfWork.Save();
                }
            }
        }   

        private void SyncPictureConnections(BrickEntity existingBrickEntity, List<ProductPicture> newConnections)
        {
            List<ProductPicture> currentConnections = existingBrickEntity.ProductPictureList;

            foreach (ProductPicture? current in currentConnections)
            {
                ProductPicture? updated = newConnections.FirstOrDefault(x => x.ProductPictureID == current.ProductPictureID);

                if (updated == null)
                {
                    (ProductPicture _, int _, string _) = processProductPicture.Delete(current);
                }
                else if (updated != null)
                {
                    (ProductPicture _, int _, string _) = processProductPicture.Edit(updated, existingBrickEntity);
                }
            }

            foreach (ProductPicture newItem in newConnections)
            {
                bool exists = currentConnections.Any(x => x.ProductPictureID == newItem.ProductPictureID);
                if (!exists)
                {
                    (ProductPicture _, int _, string _) = processProductPicture.Create(newItem, existingBrickEntity);
                }
            }
        }
        
    }
}
