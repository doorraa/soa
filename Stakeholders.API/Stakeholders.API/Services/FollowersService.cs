using System.Net.Http.Headers;

namespace Stakeholders.API.Services
{
    public class FollowersService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public FollowersService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> IsFollowingAsync(int followerId, int followingId, string token)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("FollowersAPI");

                // Dodaj JWT token u header
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"/api/followers/is-following/{followingId}");

                if (!response.IsSuccessStatusCode)
                    return false;

                var result = await response.Content.ReadFromJsonAsync<IsFollowingResponse>();
                return result?.IsFollowing ?? false;
            }
            catch
            {
                return false;
            }
        }

        private class IsFollowingResponse
        {
            public bool IsFollowing { get; set; }
        }
    }
}
