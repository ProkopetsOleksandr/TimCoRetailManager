using System;
using System.Collections.Generic;
using System.Linq;
using TRMDataManager.Library.Internal.DataAccess;
using TRMDataManager.Library.Models;

namespace TRMDataManager.Library.DataAccess
{
    public class SaleData
    {
        public void SaveSale(SaleModel saleInfo, string cashierId)
        {
            var productData = new ProductData();
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

            SqlDataAccess sql = new SqlDataAccess();
            sql.SaveData("dbo.spSale_Insert", sale, "TRMData");

            sale.Id = sql.LoadData<int, dynamic>("spSale_Lookup", new { sale.CashierId, sale.SaleDate }, "TRMData").FirstOrDefault();

            foreach(var item in details)
            {
                item.SaleId = sale.Id;
                sql.SaveData("dbo.spSaleDetail_Insert", new { item.SaleId, item.ProductId, item.Quantity, item.PurchasePrice, item.Tax }, "TRMData");
            }
        }
    }
}
