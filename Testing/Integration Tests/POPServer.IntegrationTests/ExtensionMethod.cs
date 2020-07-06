using Newtonsoft.Json;

namespace Weatherford.POP.Server.IntegrationTests
{
    /// <summary>
    /// To Serialize object
    /// </summary>
    public static class ExtensionMethod
    {
        /// <summary>
        /// To Serialize any Object to String
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="genericObject"></param>
        /// <returns></returns>
        public static string GetJsonStringFromObject<T>(this T genericObject)
        {
            return JsonConvert.SerializeObject(genericObject);
        }
    }
}
