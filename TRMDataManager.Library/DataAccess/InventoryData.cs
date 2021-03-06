using System.Collections.Generic;
using TRMDataManager.Library.Internal.DataAccess;
using TRMDataManager.Library.Models;

namespace TRMDataManager.Library.DataAccess
{
    public class InventoryData : IInventoryData
    {
        private readonly ISqlDataAccess _sqlDataAccess;

        public InventoryData(ISqlDataAccess sqlDataAccess)
        {
            _sqlDataAccess = sqlDataAccess;
        }

        public List<InventoryModel> GetInventory()
        {
            return _sqlDataAccess.LoadData<InventoryModel, dynamic>("dbo.spInventory_GetAll", null, "TRMData");
        }

        public void SaveInventoryRecord(InventoryModel item)
        {
            _sqlDataAccess.SaveData("dbo.spInventory_Insert", item, "TRMData");
        }
    }
}
