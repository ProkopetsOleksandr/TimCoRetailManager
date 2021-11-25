using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using TRMDataManager.Library.DataAccess;
using TRMDataManager.Library.Models;

namespace TRMDataManager.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        public List<InventoryModel> Get()
        {
            var inventoryData = new InventoryData();

            return inventoryData.GetInventory();
        }

        public ActionResult Post(InventoryModel item)
        {
            var inventoryData = new InventoryData();
            inventoryData.SaveInventoryRecord(item);

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}