using TestKubeApi.Interfaces;
using TestKubeLibrary.Exceptions;
using TestKubeLibrary.Models.Services;

namespace TestKubeApi.BusinessLogic;

// Orchestration layer the controller calls into.
// Just does the work and throws - logging lives in the controller (see StoreController).
public class StoreBusinessLogic : IStoreBusinessLogic
{
    private readonly IStoreService _service;

    public StoreBusinessLogic(IStoreService service)
    {
        _service = service;
    }

    public async Task<List<StoreAPIRs>> GetProductsAsync(CancellationToken ct = default)
    {
        // _service returns the plain list (empty list when the API returns nothing)
        List<StoreAPIRs> result = await _service.GetProducts(ct);
        return result ?? new List<StoreAPIRs>();
    }

    public async Task<StoreAPIRs> GetProductByIdAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0)
        {
            throw new ValidationException("id must be greater than 0");
        }

        var result = await _service.GetProductById(id, ct);
        return result ?? throw new NotFoundException(nameof(StoreAPIRs), id);
    }
}
