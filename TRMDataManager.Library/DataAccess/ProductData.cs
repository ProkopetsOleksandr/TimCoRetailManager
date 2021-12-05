using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using TRMDataManager.Library.Internal.DataAccess;
using TRMDataManager.Library.Models;

namespace TRMDataManager.Library.DataAccess
{
    public class ProductData : IProductData
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public ProductData(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public List<ProductModel> GetProducts()
        {
            return _sqlDataAccess.LoadData<ProductModel, dynamic>("dbo.spProduct_GetAll", null, "TRMData");
        }

        public ProductModel GetProductById(int productId)
        {
            return _sqlDataAccess.LoadData<ProductModel, dynamic>("dbo.spProduct_GetById", new { Id = productId }, "TRMData").FirstOrDefault();
        }
    }
}
