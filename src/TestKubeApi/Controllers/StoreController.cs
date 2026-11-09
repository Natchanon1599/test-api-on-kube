using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TestKubeApi.BusinessLogic;
using TestKubeApi.Interfaces;
using TestKubeLibrary.Interfaces;
using TestKubeLibrary.Models.Services;

namespace TestKubeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoreController : ControllerBase
{
    private readonly IStoreBusinessLogic _businessLogic;
    private readonly ILogService _logService;

    public StoreController(IStoreBusinessLogic businessLogic, ILogService logService)
    {
        _businessLogic = businessLogic;
        _logService = logService;
    }

    [HttpGet("products")]
    public async Task<ActionResult<List<StoreAPIRs>>> GetProducts(CancellationToken ct)
    {
        long requestId = await _logService.LogRequestAsync(null, username: null, ct);

        try
        {
            List<StoreAPIRs> items = await _businessLogic.GetProductsAsync(ct);

            await _logService.LogSuccessAsync(requestId, Source(nameof(GetProducts)), JsonSerializer.Serialize(items), ct);
            return Ok(items);
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(requestId, Source(nameof(GetProducts)), ex.Message, ex, ErrorClassifier.Classify(ex), ct);
            throw;
        }
    }

    [HttpGet("products/{id:int}")]
    public async Task<ActionResult<StoreAPIRs>> GetProductById(int id, CancellationToken ct)
    {
        long requestId = await _logService.LogRequestAsync(JsonSerializer.Serialize(new { id }), username: null, ct);

        try
        {
            var item = await _businessLogic.GetProductByIdAsync(id, ct);

            await _logService.LogSuccessAsync(requestId, Source(nameof(GetProductById)), JsonSerializer.Serialize(item), ct);
            return Ok(item);
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(requestId, Source(nameof(GetProductById)), ex.Message, ex, ErrorClassifier.Classify(ex), ct);
            throw;
        }
    }

    private static string Source(string action)
    {
        return nameof(StoreController) + "." + action;
    }
}
