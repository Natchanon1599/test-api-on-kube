using TestKubeLibrary.Models.Services;
namespace TestKubeApi.Interfaces
{
    public interface IStoreService
    {
        Task<List<StoreAPIRs>> GetProducts(CancellationToken ct = default);
        Task<StoreAPIRs?> GetProductById(int id, CancellationToken ct = default);
    }
}
