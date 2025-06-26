using ProcessManagement.Models.KHO_TPHAM;
using System.Net.Http.Json;

namespace ProcessManagement.Services
{
    public class ViTriTPhamApiService
    {
        private readonly HttpClient _http;
        public ViTriTPhamApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ViTriTPham>?> GetListAsync(bool isgetall = false)
        {
            return await _http.GetFromJsonAsync<List<ViTriTPham>>($"api/vitritpham?isgetall={isgetall}");
        }

        public async Task<ViTriTPham?> GetByIdAsync(object id)
        {
            if (id == null) return null;
            return await _http.GetFromJsonAsync<ViTriTPham>($"api/vitritpham/{id}");
        }

        public async Task<int?> CreateAsync(ViTriTPham model)
        {
            var response = await _http.PostAsJsonAsync("api/vitritpham", model);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
                if (result != null && result.TryGetValue("id", out var id))
                    return id;
            }
            return null;
        }

        public async Task<int?> UpdateAsync(ViTriTPham model)
        {
            var id = model.VTTPID?.Value;
            if (id == null) return null;

            var response = await _http.PutAsJsonAsync($"api/vitritpham/{id}", model);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
                if (result != null && result.TryGetValue("id", out var updatedId))
                    return updatedId;
            }
            return null;
        }

        public async Task<bool> DeleteAsync(object id)
        {
            if (id == null) return false;
            var response = await _http.DeleteAsync($"api/vitritpham/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}