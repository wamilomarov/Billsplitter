using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Billsplitter.Models
{
    public static class JsonResponse<T>
    {
        public static JObject GenerateResponse(T data)
        {
            var result = new JObject();
            result["data"] = JToken.FromObject(data);
            return result;
        }
    }
}