using ProcessManagement.Models.KHO_TPHAM;

namespace ProcessManagement.API.Controllers
{
    public class ViTriTPhamService
    {
        private readonly HttpClient _httpClient;

        public ViTriTPhamService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(List<ViTriTPham> viTriTPhams, string error)> GetViTriTPhamsAsync(Dictionary<string, object?> parameters, bool isGetAll)
        {
            var queryString = $"?isgetall={isGetAll}";

            foreach (var param in parameters)
            {
                queryString += $"&{param.Key}={param.Value}";
            }

            var response = await _httpClient.GetAsync($"api/vitrithpham{queryString}");

            if (response.IsSuccessStatusCode)
            {
                var vitritphams = await response.Content.ReadFromJsonAsync<List<ViTriTPham>>();

                return (vitritphams ?? new(), string.Empty);
            }
            else
            {
                return (new List<ViTriTPham>(), $"Error: {response.ReasonPhrase}");
            }
        }
    }
}
