using System.Collections.Generic;
using System.Threading.Tasks;
using TRMDesktopUI.Library.Models;

namespace TRMDesktopUI.Library.Api
{
    public interface ISaleEndpoint
    {
        Task<List<ProductModel>> GetAll();
        Task PostSale(SaleModel sale);
    }
}