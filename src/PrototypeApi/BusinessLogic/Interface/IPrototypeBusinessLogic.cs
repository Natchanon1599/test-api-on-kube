using PrototypeLibrary.Models;
using PrototypeLibrary.Models.Services;

namespace PrototypeApi.Interfaces;

public interface IPrototypeBusinessLogic
{
    // External API call (through IPrototypeService)
    Task<JsonTest?> GetExternalDataAsync(CancellationToken ct = default);

    // DB-backed operations (through IPrototypeRepository).
    // GetById/Update/Delete throw NotFoundException when the id does not exist (-> 404 via GlobalExceptionHandler).
    Task<IEnumerable<PrototypeTest>> GetAllAsync(CancellationToken ct = default);
    Task<PrototypeTest> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PrototypeTest> CreateAsync(PrototypeTest item, CancellationToken ct = default);
    Task UpdateAsync(int id, PrototypeTest input, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
