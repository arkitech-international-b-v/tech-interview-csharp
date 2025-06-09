using MongoDB.Bson;
using MongoDB.Driver;
using ArkitechDataApi.Models;

namespace ArkitechDataApi.Repositories
{
    public class MongoRepository
    {
        private readonly IMongoCollection<DataItem> _collection;

        public MongoRepository(MongoSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _collection = database.GetCollection<DataItem>(settings.CollectionName);

            var indexKeysBuilder = Builders<DataItem>.IndexKeys;
            _collection.Indexes.CreateOne(
                new CreateIndexModel<DataItem>(
                    indexKeysBuilder.Ascending(d => d.Topic)
                )
            );
            _collection.Indexes.CreateOne(
                new CreateIndexModel<DataItem>(
                    indexKeysBuilder.Descending(d => d.Timestamp)
                )
            );
        }

        public async Task<DataItem?> GetLatestItemAsync(string? topic = null)
        {
            FilterDefinition<DataItem> filter = FilterDefinition<DataItem>.Empty;

            if (!string.IsNullOrEmpty(topic))
            {
                // “find_one(query, sort=[('timestamp', -1)])” logic
                var topicStart = topic;
                var topicEnd = topic + "\ufff0";
                filter = Builders<DataItem>.Filter.And(
                    Builders<DataItem>.Filter.Gte(d => d.Topic, topicStart),
                    Builders<DataItem>.Filter.Lt(d => d.Topic, topicEnd)
                );
            }

            return await _collection
                .Find(filter)
                .SortByDescending(d => d.Timestamp)
                .FirstOrDefaultAsync()
                ;
        }

        public async Task<List<DataItem>> GetLatestItemsAsync(int limit = 100, string? topic = null)
        {
            FilterDefinition<DataItem> filter = FilterDefinition<DataItem>.Empty;

            if (!string.IsNullOrEmpty(topic))
            {
                var topicStart = topic;
                var topicEnd = topic + "\ufff0";
                filter = Builders<DataItem>.Filter.And(
                    Builders<DataItem>.Filter.Gte(d => d.Topic, topicStart),
                    Builders<DataItem>.Filter.Lt(d => d.Topic, topicEnd)
                );
            }

            return await _collection
                .Find(filter)
                .SortByDescending(d => d.Timestamp)
                .Limit(limit)
                .ToListAsync()
                ;
        }

        public async Task<List<DataItem>> GetItemsByTimeRangeAsync(
            DateTime startTime,
            DateTime endTime,
            string? topic = null
        )
        {
            var builder = Builders<DataItem>.Filter;
            var timeFilter = builder.Gte(d => d.Timestamp, startTime) & builder.Lte(d => d.Timestamp, endTime);

            if (!string.IsNullOrEmpty(topic))
            {
                timeFilter &= builder.Eq(d => d.Topic, topic);
            }

            return await _collection
                .Find(timeFilter)
                .SortByDescending(d => d.Timestamp)
                .ToListAsync()
                ;
        }


        public async Task InsertItemAsync(DataItem item)
        {
            await _collection.InsertOneAsync(item);
        }

        public async Task InsertItemsAsync(IEnumerable<DataItem> items)
        {
            await _collection.InsertManyAsync(items);
        }
    }
}
