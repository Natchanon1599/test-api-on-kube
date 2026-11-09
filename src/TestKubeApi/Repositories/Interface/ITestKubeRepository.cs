using TestKubeLibrary.Models;

namespace TestKubeApi.Repositories;

public interface ITestKubeRepository
{
    Task<IEnumerable<TestKubeTest>> GetAllAsync(CancellationToken ct = default);
    Task<TestKubeTest?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<TestKubeTest> CreateAsync(TestKubeTest item, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, TestKubeTest input, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
