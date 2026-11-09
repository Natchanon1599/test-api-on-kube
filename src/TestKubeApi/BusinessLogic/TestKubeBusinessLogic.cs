using TestKubeApi.Interfaces;
using TestKubeApi.Repositories;
using TestKubeApi.Services;
using TestKubeLibrary.Exceptions;
using TestKubeLibrary.Models;
using TestKubeLibrary.Models.Services;

namespace TestKubeApi.BusinessLogic;

// Orchestration layer the controller calls into.
// Just does the work and throws - logging lives in the controller (see TestKubeController).
public class TestKubeBusinessLogic : ITestKubeBusinessLogic
{
    private readonly ITestKubeService _service;
    private readonly ITestKubeRepository _repository;

    public TestKubeBusinessLogic(ITestKubeService service, ITestKubeRepository repository)
    {
        _service = service;
        _repository = repository;
    }

    public async Task<JsonTest?> GetExternalDataAsync(CancellationToken ct = default)
    {
        // _service.GetData() returns ActionResult<JsonTest>; unwrap the value for the business layer
        var result = await _service.GetData();
        return result.Value;
    }

    public async Task<IEnumerable<TestKubeTest>> GetAllAsync(CancellationToken ct = default)
    {
        return await _repository.GetAllAsync(ct);
    }

    public async Task<TestKubeTest> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var item = await _repository.GetByIdAsync(id, ct);
        return item ?? throw new NotFoundException(nameof(TestKubeTest), id);
    }

    public async Task<TestKubeTest> CreateAsync(TestKubeTest item, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(item.Title))
        {
            throw new ValidationException("Title is required");
        }

        return await _repository.CreateAsync(item, ct);
    }

    public async Task UpdateAsync(int id, TestKubeTest input, CancellationToken ct = default)
    {
        var updated = await _repository.UpdateAsync(id, input, ct);
        if (!updated)
        {
            throw new NotFoundException(nameof(TestKubeTest), id);
        }
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var deleted = await _repository.DeleteAsync(id, ct);
        if (!deleted)
        {
            throw new NotFoundException(nameof(TestKubeTest), id);
        }
    }
}
