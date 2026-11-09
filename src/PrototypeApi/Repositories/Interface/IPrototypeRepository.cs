using PrototypeLibrary.Models;

namespace PrototypeApi.Repositories;

public interface IPrototypeRepository
{
    Task<IEnumerable<PrototypeTest>> GetAllAsync(CancellationToken ct = default);
    Task<PrototypeTest?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PrototypeTest> CreateAsync(PrototypeTest item, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, PrototypeTest input, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
