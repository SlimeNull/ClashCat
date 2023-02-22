using System.Text.Json;

namespace ClashCat.Helpers
{
    static class JsonHelper
    {
        public static readonly JsonSerializerOptions Options = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
    }
}
