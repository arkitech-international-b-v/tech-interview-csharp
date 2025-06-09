using ArkitechDataApi.Models;
using ArkitechDataApi.Repositories;

namespace ArkitechDataApi.Services
{
    public interface IDataService
    {
        Task<DataItem?> GetLatestItemAsync(string? topic = null);
        Task<List<DataItem>> GetLatestItemsAsync(int limit = 100, string? topic = null);
        Task<List<DataItem>> GetItemsByTimeRangeAsync(DateTime startTime, DateTime endTime, string? topic = null);
        Task InsertItemAsync(DataItem item);
        Task InsertItemsAsync(IEnumerable<DataItem> items);
    }

    public class DataService : IDataService
    {
        private readonly MongoRepository _repository;

        public DataService(MongoRepository repository)
        {
            _repository = repository;
        }

        public Task<DataItem?> GetLatestItemAsync(string? topic = null)
            => _repository.GetLatestItemAsync(topic);

        public Task<List<DataItem>> GetLatestItemsAsync(int limit = 100, string? topic = null)
            => _repository.GetLatestItemsAsync(limit, topic);

        public Task<List<DataItem>> GetItemsByTimeRangeAsync(DateTime startTime, DateTime endTime, string? topic = null)
            => _repository.GetItemsByTimeRangeAsync(startTime, endTime, topic);

        public Task InsertItemAsync(DataItem item)
            => _repository.InsertItemAsync(item);

        public Task InsertItemsAsync(IEnumerable<DataItem> items)
            => _repository.InsertItemsAsync(items);
    }
}
