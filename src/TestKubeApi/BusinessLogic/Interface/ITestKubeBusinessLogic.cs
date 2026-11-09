using TestKubeLibrary.Models;
using TestKubeLibrary.Models.Services;

namespace TestKubeApi.Interfaces;

public interface ITestKubeBusinessLogic
{
    // External API call (through ITestKubeService)
    Task<JsonTest?> GetExternalDataAsync(CancellationToken ct = default);

    // DB-backed operations (through ITestKubeRepository).
    // GetById/Update/Delete throw NotFoundException when the id does not exist (-> 404 via GlobalExceptionHandler).
    Task<IEnumerable<TestKubeTest>> GetAllAsync(CancellationToken ct = default);
    Task<TestKubeTest> GetByIdAsync(int id, CancellationToken ct = default);
    Task<TestKubeTest> CreateAsync(TestKubeTest item, CancellationToken ct = default);
    Task UpdateAsync(int id, TestKubeTest input, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
