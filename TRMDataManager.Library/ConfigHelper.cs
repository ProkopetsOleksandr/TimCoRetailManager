using System.Configuration;

namespace TRMDataManager.Library
{
    public class ConfigHelper
    {
        public static decimal GetTaxRate()
        {
            var taxRateSetting = ConfigurationManager.AppSettings["taxRate"];
            if (!decimal.TryParse(taxRateSetting, out var taxRate))
            {
                throw new ConfigurationErrorsException("Tax rate is not found");
            }

            return taxRate;
        }
    }
}
