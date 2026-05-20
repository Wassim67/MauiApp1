using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MauiApp1.Models;

namespace MauiApp1.Services;

public class MorpionApiClient
{
    private const string AccessTokenKey = "morpion_access_token";

    private readonly HttpClient _httpClient;

    public MorpionApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> TryRestoreAccessTokenAsync()
    {
        var accessToken = await SecureStorage.Default.GetAsync(AccessTokenKey);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return false;
        }

        SetBearerToken(accessToken);
        return true;
    }

    public async Task LoginAsync(string email, string password)
    {
        var response = await _httpClient.PutAsJsonAsync(
            "/auth/access-token",
            new LoginUserDto { Email = email, Password = password });

        await StoreAccessTokenAsync(response);
    }

    public async Task RegisterAsync(string email, string password)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/auth/access-token",
            new RegisterUserDto { Email = email, Password = password });

        await StoreAccessTokenAsync(response);
    }

    public void Logout()
    {
        SecureStorage.Default.Remove(AccessTokenKey);
        _httpClient.DefaultRequestHeaders.Authorization = null;
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

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new MorpionApiException("Connecte-toi pour accéder au jeu.", isUnauthorized: true);
        }

        var message = await ReadErrorMessageAsync(response);

        throw new MorpionApiException(message);
    }

    private async Task StoreAccessTokenAsync(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new MorpionApiException("Email ou mot de passe incorrect.");
        }

        await EnsureSuccessAsync(response);

        var accessToken = await response.Content.ReadFromJsonAsync<AccessTokenDto>() ??
                          throw new InvalidOperationException("La réponse API est vide.");

        await SecureStorage.Default.SetAsync(AccessTokenKey, accessToken.AccessToken);
        SetBearerToken(accessToken.AccessToken);
    }

    private void SetBearerToken(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>();
            if (error?.Errors is not null && error.Errors.Count > 0)
            {
                return string.Join(
                    Environment.NewLine,
                    error.Errors
                        .SelectMany(field => field.Value)
                        .Where(message => !string.IsNullOrWhiteSpace(message)));
            }

            return error?.Detail ?? error?.Message ?? error?.Title ?? "Erreur API.";
        }
        catch (JsonException)
        {
            return "Erreur API.";
        }
    }
}
