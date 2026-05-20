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
        await EnsureSuccessAsync(response);

        return await response.Content.ReadFromJsonAsync<GameDto>() ??
               throw new InvalidOperationException("La réponse API est vide.");
    }

    public async Task<GameDto?> GetCurrentGameAsync()
    {
        var response = await _httpClient.GetAsync("/api/morpion/games/current");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response);

        return await response.Content.ReadFromJsonAsync<GameDto>();
    }

    public async Task<GameDto> PlayMoveAsync(int index)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/api/morpion/games/current/moves",
            new PlayMoveRequest { Index = index });

        await EnsureSuccessAsync(response);

        return await response.Content.ReadFromJsonAsync<GameDto>() ??
               throw new InvalidOperationException("La réponse API est vide.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>();
        var message = error?.Detail ?? error?.Message ?? error?.Title ?? "Erreur API.";

        throw new MorpionApiException(message);
    }
}
