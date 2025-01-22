using LinqKit;
using Microsoft.Extensions.Logging;
using Sammlerplattform.Controllers.DAL;
using Sammlerplattform.Models.BrickDatabase;
using Sammlerplattform.Models.ProductDatabase;
using Sammlerplattform.Models.ProductPictureDatabase;
using System.Transactions;

namespace Sammlerplattform.Controllers
{
    public class ProductPictureDatabaseController
    {
    }

    public interface IProcessProductPicture
    {
        IEnumerable<ProductPictureOperationParameterModel> GetWithPredicates(ProductPictureSearchParameterModel searchModel);
        (ProductPicture productPicture, int statuscode, string message) Create(ProductPictureOperationParameterModel operationModel);
        (ProductPicture productPicture, int statuscode, string message) Delete(ProductPictureOperationParameterModel operationModel);
    }
    public class ProductPictureProcessor(IUnitOfWork unitOfWork, ILogger<ProductPictureProcessor> logger) : IProcessProductPicture
    {
        public (ProductPicture productPicture, int statuscode, string message) Create(ProductPictureOperationParameterModel operationModel)
        {
            if(GetWithPredicates(ParametersOperationToSearch(operationModel)).FirstOrDefault() != null)
            {
                return (operationModel.ProductPicture, 302, "Eintrag existiert bereits.");
            }
            else
            {
                try
                {
                    using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                    ProductPicture newProductPicture = unitOfWork.ProductPictureRepository.Insert(operationModel.ProductPicture);
                    unitOfWork.Save();

                    scope.Complete();
                    return (newProductPicture, 201, "Produktbild wurde erstellt.");
                }
                catch (Exception ex)
                {
                    logger.LogError("Fehler beim Hinzufügen des Ortes: {ex.InnerException}", ex.InnerException);
                    return (new(), 500, "Es ist ein Fehler aufgetreten: " + ex.InnerException);
                }
            }
        }

        public (ProductPicture productPicture, int statuscode, string message) Delete(ProductPictureOperationParameterModel model)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ProductPictureOperationParameterModel> GetWithPredicates(ProductPictureSearchParameterModel searchModel)
        {
            ExpressionStarter<ProductPictureOperationParameterModel> predicate = PredicateBuilder.New<ProductPictureOperationParameterModel>();
            IEnumerable<ProductPictureOperationParameterModel> productPictureIEnumerable = from pp in unitOfWork.ProductPictureRepository.Get(includeProperties: "PostcardEntity,BrickEntity")
                                                                                           select new ProductPictureOperationParameterModel
                                                                                           {
                                                                                               ProductPicture = pp
                                                                                           };
            predicate = predicate.And(GenericClasses.GenericLambdas.CreateLambdaSpanIntJoin<ProductPictureOperationParameterModel>("ProductPicture", "ProductPicture_ID", searchModel.SearchProductPicture_ID));
            predicate = predicate.And(GenericClasses.GenericLambdas.CreateLambdaStringContainsJoin<ProductPictureOperationParameterModel>("ProductPicture", "FileExtension", searchModel.SearchFileExtension));
            predicate = predicate.And(GenericClasses.GenericLambdas.CreateLambdaStringContainsJoin<ProductPictureOperationParameterModel>("ProductPicture", "Side", searchModel.SearchSide));
            if(predicate.IsStarted)
            {
                _ = productPictureIEnumerable.Where(predicate);
            }

            return productPictureIEnumerable;
        }

        private static ProductPictureSearchParameterModel ParametersOperationToSearch(ProductPictureOperationParameterModel operationModel)
        {
            ProductPictureSearchParameterModel searchModel = new();
            searchModel.SearchProductPicture_ID.Add(operationModel.ProductPicture.ProductPicture_ID);
            if(!string.IsNullOrEmpty(operationModel.ProductPicture.FileExtension))
                searchModel.SearchFileExtension.Add(operationModel.ProductPicture.FileExtension);
            searchModel.SearchSide.Add(operationModel.ProductPicture.Perspective.ToString());

            return searchModel;
        }
    }
}
