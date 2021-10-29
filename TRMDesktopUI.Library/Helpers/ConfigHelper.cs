using System.Configuration;

namespace TRMDesktopUI.Library.Helpers
{
    public class ConfigHelper : IConfigHelper
    {
        public decimal GetTaxRate()
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
