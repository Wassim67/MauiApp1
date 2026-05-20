using System.Net;
using System.Net.Http.Json;
using MauiApp1.Models;

namespace MauiApp1.Services;

public class MorpionApiClient
{
    private readonly HttpClient _httpClient;

    public MorpionApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GameDto> CreateGameAsync()
    {
        var response = await _httpClient.PostAsync("/api/morpion/games", null);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<GameDto>() ??
               throw new InvalidOperationException("La reponse API est vide.");
    }

    public async Task<GameDto?> GetCurrentGameAsync()
    {
        var response = await _httpClient.GetAsync("/api/morpion/games/current");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<GameDto>();
    }

    public async Task<GameDto> PlayMoveAsync(int index)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/api/morpion/games/current/moves",
            new PlayMoveRequest { Index = index });

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<GameDto>() ??
               throw new InvalidOperationException("La reponse API est vide.");
    }
}
