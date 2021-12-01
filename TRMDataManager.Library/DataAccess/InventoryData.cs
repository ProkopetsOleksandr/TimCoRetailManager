using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRMDataManager.Library.Internal.DataAccess;
using TRMDataManager.Library.Models;

namespace TRMDataManager.Library.DataAccess
{
    public class InventoryData
    {
        private readonly IConfiguration _configuration;

        public InventoryData(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<InventoryModel> GetInventory()
        {
            SqlDataAccess sql = new SqlDataAccess(_configuration);

            return sql.LoadData<InventoryModel, dynamic>("dbo.spInventory_GetAll", null, "TRMData");
        }

        public void SaveInventoryRecord(InventoryModel item)
        {
            SqlDataAccess sql = new SqlDataAccess(_configuration);

            sql.SaveData("dbo.spInventory_Insert", item, "TRMData");
        }
    }
}
