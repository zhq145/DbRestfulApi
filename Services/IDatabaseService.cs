using System.Collections.Generic;
using System.Threading.Tasks;

namespace DbRestfulApi.Services
{
    public interface IDatabaseService
    {
        Task<(IEnumerable<Dictionary<string, object>> Items, int Total)> ListAsync(string table, int page, int pageSize);
        Task<Dictionary<string, object>?> GetAsync(string table, int id);
        Task<int> AddAsync(string table, Dictionary<string, object> data);
        Task<bool> UpdateAsync(string table, int id, Dictionary<string, object> data);
        Task<bool> DeleteAsync(string table, int id);
    }
}
