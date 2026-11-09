using TestKubeLibrary.Models.Services;

namespace TestKubeApi.Interfaces;

public interface IStoreBusinessLogic
{
    // External API calls (through IStoreService).
    // GetProductById throws NotFoundException when the id does not exist (-> 404 via GlobalExceptionHandler).
    Task<List<StoreAPIRs>> GetProductsAsync(CancellationToken ct = default);
    Task<StoreAPIRs> GetProductByIdAsync(int id, CancellationToken ct = default);
}
