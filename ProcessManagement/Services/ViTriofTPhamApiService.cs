using System.Net.Http.Json;
using ProcessManagement.Models.KHO_TPHAM;

public class ViTriofTPhamApiService
{
    private readonly HttpClient _http;

    public ViTriofTPhamApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ViTriofTPham>?> GetListAsync(bool isgetAll = false)
    {
        return await _http.GetFromJsonAsync<List<ViTriofTPham>>($"api/vitrioftpham?isgetAll={isgetAll}");
    }

    public async Task<int?> CreateAsync(ViTriofTPham model)
    {
        var response = await _http.PostAsJsonAsync("api/vitrioftpham", model);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result?["id"];
        }
        return null;
    }

    public async Task<int?> UpdateAsync(ViTriofTPham model)
    {
        var response = await _http.PutAsJsonAsync("api/vitrioftpham", model);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result?["id"];
        }
        return null;
    }

    public async Task<bool> DeleteAsync(object id)
    {
        var response = await _http.DeleteAsync($"api/vitrioftpham/{id}");
        return response.IsSuccessStatusCode;
    }
}