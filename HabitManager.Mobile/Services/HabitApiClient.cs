using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HabitManager.Contracts;

namespace HabitManager.Mobile.Services;

internal sealed class HabitApiClient(HttpClient http) : IHabitClient
{
    private const string Habits = "api/habits";

    public async Task<HabitDto[]> ListAsync()
    {
        using var response = await SendAsync(HttpMethod.Get, Habits);
        return await ReadAsync(response, HabitJson.Default.HabitDtoArray);
    }

    public async Task<HabitDto> GetAsync(Guid id)
    {
        using var response = await SendAsync(HttpMethod.Get, $"{Habits}/{id}");
        return await ReadAsync(response, HabitJson.Default.HabitDto);
    }

    public async Task CreateAsync(CreateHabitRequest request)
    {
        using var response = await SendAsync(HttpMethod.Post, Habits,
            JsonContent.Create(request, HabitJson.Default.CreateHabitRequest));
    }

    public async Task UpdateAsync(Guid id, UpdateHabitRequest request)
    {
        using var response = await SendAsync(HttpMethod.Put, $"{Habits}/{id}",
            JsonContent.Create(request, HabitJson.Default.UpdateHabitRequest));
    }

    public async Task SetArchivedAsync(Guid id, bool archived)
    {
        using var response = await SendAsync(archived ? HttpMethod.Put : HttpMethod.Delete, $"{Habits}/{id}/archive");
    }

    public async Task SetCompletedAsync(Guid id, DateOnly date, bool completed)
    {
        var path = FormattableString.Invariant($"{Habits}/{id}/completions/{date:yyyy-MM-dd}");
        using var response = await SendAsync(completed ? HttpMethod.Put : HttpMethod.Delete, path);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, HttpContent? content = null)
    {
        using var request = new HttpRequestMessage(method, path) { Content = content };
        try
        {
            var response = await http.SendAsync(request);
            if (response.IsSuccessStatusCode)
                return response;

            using (response)
            {
                ApiProblem? problem = null;
                try
                {
                    problem = await response.Content.ReadFromJsonAsync(HabitJson.Default.ApiProblem);
                }
                catch (JsonException)
                {
                    // Some hosts return a non-JSON error page.
                }

                throw new HabitClientException(problem?.Detail ?? problem?.Title ??
                    $"The API returned HTTP {(int)response.StatusCode}.");
            }
        }
        catch (HttpRequestException error)
        {
            throw new HabitClientException($"Cannot reach the API at {http.BaseAddress}. Check that it is running.", error);
        }
        catch (OperationCanceledException error)
        {
            throw new HabitClientException($"The API at {http.BaseAddress} did not respond in time.", error);
        }
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> typeInfo) where T : class
    {
        try
        {
            return await response.Content.ReadFromJsonAsync(typeInfo)
                ?? throw new HabitClientException("The API returned an empty response.");
        }
        catch (JsonException error)
        {
            throw new HabitClientException("The API returned an unexpected response.", error);
        }
    }
}

internal sealed class HabitClientException(string message, Exception? inner = null) : Exception(message, inner);

internal sealed record ApiProblem(string? Title, string? Detail);

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, UseStringEnumConverter = true)]
[JsonSerializable(typeof(HabitDto[]))]
[JsonSerializable(typeof(HabitDto))]
[JsonSerializable(typeof(CreateHabitRequest))]
[JsonSerializable(typeof(UpdateHabitRequest))]
[JsonSerializable(typeof(ApiProblem))]
internal partial class HabitJson : JsonSerializerContext
{
}
