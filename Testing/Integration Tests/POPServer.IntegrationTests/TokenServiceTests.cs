using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weatherford.POP.APIClient;

namespace Weatherford.POP.Server.IntegrationTests
{
    [TestClass]
    public class TokenServiceTests : APIClientTestBase
    {
        [TestMethod]
        [TestCategory(TestCategories.TokenServiceTests)]
        public void SetLocale()
        {
            var ts = TokenService as ServiceClientBase;
            Assert.IsNull(ts.GetCookieValue("locale"), "Locale should not be set.");
            for (int ii = 0; ii < 20; ii++)
            {
                string locale = ii % 2 == 0 ? "es-MX" : "en-US";
                TokenService.SetLocale(locale);
                Assert.AreEqual(locale, ts.GetCookieValue("locale"), "Locale cookie should match set locale.");
            }
        }
    }
}
