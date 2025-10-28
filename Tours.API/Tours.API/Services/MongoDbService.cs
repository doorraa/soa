using MongoDB.Driver;
using Tours.API.Models;

namespace Tours.API.Services
{
    public class MongoDbService
    {
        private readonly IMongoCollection<Tour> _toursCollection;
        private readonly IMongoCollection<TouristPosition> _positionsCollection;
        private readonly IMongoCollection<TourPurchaseToken> _purchaseTokensCollection;
        private readonly IMongoCollection<ShoppingCart> _cartsCollection;
        private readonly IMongoCollection<TourExecution> _executionsCollection;

        public MongoDbService(IConfiguration configuration)
        {
            var connectionString = configuration["MongoDB:ConnectionString"]
                ?? "mongodb://admin:password123@localhost:27017";
            var databaseName = configuration["MongoDB:DatabaseName"] ?? "ToursDB";
            var collectionName = configuration["MongoDB:CollectionName"] ?? "Tours";

            var mongoClient = new MongoClient(connectionString);
            var database = mongoClient.GetDatabase(databaseName);

            _toursCollection = database.GetCollection<Tour>(collectionName);
            _positionsCollection = database.GetCollection<TouristPosition>("TouristPositions");
            _purchaseTokensCollection = database.GetCollection<TourPurchaseToken>("PurchaseTokens");
            _cartsCollection = database.GetCollection<ShoppingCart>("ShoppingCarts");
            _executionsCollection = database.GetCollection<TourExecution>("TourExecutions");
        }

        public IMongoCollection<Tour> Tours => _toursCollection;
        public IMongoCollection<TouristPosition> TouristPositions => _positionsCollection;
        public IMongoCollection<TourPurchaseToken> PurchaseTokens => _purchaseTokensCollection;
        public IMongoCollection<ShoppingCart> ShoppingCarts => _cartsCollection;
        public IMongoCollection<TourExecution> TourExecutions => _executionsCollection;

    }
}
