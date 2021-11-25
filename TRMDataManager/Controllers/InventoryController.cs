using System.Collections.Generic;
using System.Web.Http;
using TRMDataManager.Library.DataAccess;
using TRMDataManager.Library.Models;

namespace TRMDataManager.Controllers
{
    [Authorize]
    public class InventoryController : ApiController
    {
        [Authorize(Roles = "Manager,Admin")]
        public List<InventoryModel> Get()
        {
            var inventoryData = new InventoryData();

            return inventoryData.GetInventory();
        }

        [Authorize(Roles = "Admin")]
        public void Post(InventoryModel item)
        {
            var inventoryData = new InventoryData();
            inventoryData.SaveInventoryRecord(item);
        }
    }
}