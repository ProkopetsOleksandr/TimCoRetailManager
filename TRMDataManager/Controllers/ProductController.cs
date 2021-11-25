﻿using System.Collections.Generic;
using System.Web.Http;
using TRMDataManager.Library.DataAccess;
using TRMDataManager.Library.Models;

namespace TRMDataManager.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ProductController : ApiController
    {
        public List<ProductModel> Get()
        {
            var data = new ProductData();
            return data.GetProducts();
        }
    }
}
