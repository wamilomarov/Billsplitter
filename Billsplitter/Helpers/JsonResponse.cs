using Newtonsoft.Json.Linq;

namespace Billsplitter.Models
{
    public static class JsonResponse<T>
    {
        public static JObject GenerateResponse(T data)
        {
            var result = 
                data == null ? 
                new JObject {["data"] = null} : 
                new JObject {["data"] = JToken.FromObject(data)};
            return result;
        }
    }
}