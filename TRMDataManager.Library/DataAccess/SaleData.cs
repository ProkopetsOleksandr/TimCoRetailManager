using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using TRMDataManager.Library.Internal.DataAccess;
using TRMDataManager.Library.Models;

namespace TRMDataManager.Library.DataAccess
{
    public class SaleData
    {
        private readonly IConfiguration _configuration;

        public SaleData(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SaveSale(SaleModel saleInfo, string cashierId)
        {
            var productData = new ProductData(_configuration);
            var taxRate = ConfigHelper.GetTaxRate() / 100;

            var details = new List<SaleDetailDAO>();
            foreach (var item in saleInfo.SaleDetails)
            {
                var detail = new SaleDetailDAO
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                };

                var productInfo = productData.GetProductById(item.ProductId);

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

            using(SqlDataAccess sql = new SqlDataAccess(_configuration))
            {
                try
                {
                    sql.StartTransaction("TRMData");
                    sql.SaveDataInTransaction("dbo.spSale_Insert", sale);

                    sale.Id = sql.LoadDataInTransaction<int, dynamic>("spSale_Lookup", new { sale.CashierId, sale.SaleDate }).FirstOrDefault();

                    foreach (var item in details)
                    {
                        item.SaleId = sale.Id;
                        sql.SaveDataInTransaction("dbo.spSaleDetail_Insert", new { item.SaleId, item.ProductId, item.Quantity, item.PurchasePrice, item.Tax });
                    }

                    sql.CommitTransaction();
                } 
                catch
                {
                    sql.RollbackTransaction();
                    throw;
                }
            }
        }

        public List<SaleReportModel> GetSaleReport()
        {
            SqlDataAccess sql = new SqlDataAccess(_configuration);

            return sql.LoadData<SaleReportModel, dynamic>("dbo.spSale_SaleReport", null, "TRMData");
        }
    }
}
