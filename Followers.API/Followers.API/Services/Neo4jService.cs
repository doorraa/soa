using Neo4j.Driver;

namespace Followers.API.Services
{
    public class Neo4jService : IDisposable
    {
        private readonly IDriver _driver;

        public Neo4jService(IConfiguration configuration)
        {
            var uri = configuration["Neo4j:Uri"] ?? "bolt://localhost:7687";
            var username = configuration["Neo4j:Username"] ?? "neo4j";
            var password = configuration["Neo4j:Password"] ?? "password123";

            _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(username, password));
        }

        public IAsyncSession GetSession() => _driver.AsyncSession();

        public void Dispose()
        {
            _driver?.Dispose();
        }
    }
}
