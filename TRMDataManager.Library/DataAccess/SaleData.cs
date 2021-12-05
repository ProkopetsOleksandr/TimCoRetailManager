using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using TRMDataManager.Library.Internal.DataAccess;
using TRMDataManager.Library.Models;

namespace TRMDataManager.Library.DataAccess
{
    public class SaleData : ISaleData
    {
        private readonly IProductData _productData;
        private readonly ISqlDataAccess _sqlDataAccess;

        public SaleData(IProductData productData, ISqlDataAccess sqlDataAccess)
        {
            _productData = productData;
            _sqlDataAccess = sqlDataAccess;
        }

        public void SaveSale(SaleModel saleInfo, string cashierId)
        {
            var taxRate = ConfigHelper.GetTaxRate() / 100;

            var details = new List<SaleDetailDAO>();
            foreach (var item in saleInfo.SaleDetails)
            {
                var detail = new SaleDetailDAO
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                };

                var productInfo = _productData.GetProductById(item.ProductId);

                if (productInfo == null)
                {
                    throw new Exception($"Product #{item.ProductId} was not found.");
                }

                detail.PurchasePrice = productInfo.RetailPrice * detail.Quantity;
                if (productInfo.IsTaxable)
                {
                    detail.Tax = detail.PurchasePrice * taxRate;
                }

                details.Add(detail);
            }

            var sale = new SaleDAO
            {
                SubTotal = details.Sum(m => m.PurchasePrice),
                Tax = details.Sum(m => m.Tax),
                CashierId = cashierId
            };

            sale.Total = sale.SubTotal + sale.Tax;

            try
            {
                _sqlDataAccess.StartTransaction("TRMData");
                _sqlDataAccess.SaveDataInTransaction("dbo.spSale_Insert", sale);

                sale.Id = _sqlDataAccess.LoadDataInTransaction<int, dynamic>("spSale_Lookup", new { sale.CashierId, sale.SaleDate }).FirstOrDefault();

                foreach (var item in details)
                {
                    item.SaleId = sale.Id;
                    _sqlDataAccess.SaveDataInTransaction("dbo.spSaleDetail_Insert", new { item.SaleId, item.ProductId, item.Quantity, item.PurchasePrice, item.Tax });
                }

                _sqlDataAccess.CommitTransaction();
            }
            catch
            {
                _sqlDataAccess.RollbackTransaction();
                throw;
            }
        }

        public List<SaleReportModel> GetSaleReport()
        {
            return _sqlDataAccess.LoadData<SaleReportModel, dynamic>("dbo.spSale_SaleReport", null, "TRMData");
        }
    }
}
