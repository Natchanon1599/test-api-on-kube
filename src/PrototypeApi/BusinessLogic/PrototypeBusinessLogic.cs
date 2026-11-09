using PrototypeApi.Interfaces;
using PrototypeApi.Repositories;
using PrototypeApi.Services;
using PrototypeLibrary.Exceptions;
using PrototypeLibrary.Models;
using PrototypeLibrary.Models.Services;

namespace PrototypeApi.BusinessLogic;

// Orchestration layer the controller calls into.
// Just does the work and throws - logging lives in the controller (see PrototypeController).
public class PrototypeBusinessLogic : IPrototypeBusinessLogic
{
    private readonly IPrototypeService _service;
    private readonly IPrototypeRepository _repository;

    public PrototypeBusinessLogic(IPrototypeService service, IPrototypeRepository repository)
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

    public async Task<IEnumerable<PrototypeTest>> GetAllAsync(CancellationToken ct = default)
    {
        return await _repository.GetAllAsync(ct);
    }

    public async Task<PrototypeTest> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var item = await _repository.GetByIdAsync(id, ct);
        return item ?? throw new NotFoundException(nameof(PrototypeTest), id);
    }

    public async Task<PrototypeTest> CreateAsync(PrototypeTest item, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(item.Title))
        {
            throw new ValidationException("Title is required");
        }

        return await _repository.CreateAsync(item, ct);
    }

    public async Task UpdateAsync(int id, PrototypeTest input, CancellationToken ct = default)
    {
        var updated = await _repository.UpdateAsync(id, input, ct);
        if (!updated)
        {
            throw new NotFoundException(nameof(PrototypeTest), id);
        }
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var deleted = await _repository.DeleteAsync(id, ct);
        if (!deleted)
        {
            throw new NotFoundException(nameof(PrototypeTest), id);
        }
    }
}
