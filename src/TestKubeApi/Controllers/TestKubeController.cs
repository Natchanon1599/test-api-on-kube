using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TestKubeApi.BusinessLogic;
using TestKubeApi.Interfaces;
using TestKubeLibrary.Interfaces;
using TestKubeLibrary.Models;
using TestKubeLibrary.Models.Services;

namespace TestKubeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestKubeController : ControllerBase
{
    private readonly ITestKubeBusinessLogic _businessLogic;
    private readonly ILogService _logService;

    public TestKubeController(ITestKubeBusinessLogic businessLogic, ILogService logService)
    {
        _businessLogic = businessLogic;
        _logService = logService;
    }

    [HttpGet("get-data")]
    public async Task<ActionResult<JsonTest>> GetExternalData(CancellationToken ct)
    {
        long requestId = await _logService.LogRequestAsync(null, username: null, ct);

        try
        {
            var data = await _businessLogic.GetExternalDataAsync(ct);

            await _logService.LogSuccessAsync(requestId, Source(nameof(GetExternalData)), JsonSerializer.Serialize(data), ct);
            return data is null ? NotFound() : Ok(data);
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(requestId, Source(nameof(GetExternalData)), ex.Message, ex, ErrorClassifier.Classify(ex), ct);
            throw;
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TestKubeTest>>> GetAll(CancellationToken ct)
    {
        long requestId = await _logService.LogRequestAsync(null, username: null, ct);

        try
        {
            var items = await _businessLogic.GetAllAsync(ct);

            await _logService.LogSuccessAsync(requestId, Source(nameof(GetAll)), JsonSerializer.Serialize(items), ct);
            return Ok(items);
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(requestId, Source(nameof(GetAll)), ex.Message, ex, ErrorClassifier.Classify(ex), ct);
            throw;
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TestKubeTest>> GetById(int id, CancellationToken ct)
    {
        long requestId = await _logService.LogRequestAsync(JsonSerializer.Serialize(new { id }), username: null, ct);

        try
        {
            var item = await _businessLogic.GetByIdAsync(id, ct);

            await _logService.LogSuccessAsync(requestId, Source(nameof(GetById)), JsonSerializer.Serialize(item), ct);
            return Ok(item);
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(requestId, Source(nameof(GetById)), ex.Message, ex, ErrorClassifier.Classify(ex), ct);
            throw;
        }
    }

    [HttpPost]
    public async Task<ActionResult<TestKubeTest>> Create(TestKubeTest item, CancellationToken ct)
    {
        long requestId = await _logService.LogRequestAsync(JsonSerializer.Serialize(item), username: null, ct);

        try
        {
            var created = await _businessLogic.CreateAsync(item, ct);

            await _logService.LogSuccessAsync(requestId, Source(nameof(Create)), JsonSerializer.Serialize(created), ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(requestId, Source(nameof(Create)), ex.Message, ex, ErrorClassifier.Classify(ex), ct);
            throw;
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, TestKubeTest input, CancellationToken ct)
    {
        long requestId = await _logService.LogRequestAsync(JsonSerializer.Serialize(input), username: null, ct);

        try
        {
            await _businessLogic.UpdateAsync(id, input, ct);

            await _logService.LogSuccessAsync(requestId, Source(nameof(Update)), JsonSerializer.Serialize(new { id, input.Title, input.IsDone }), ct);
            return NoContent();
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(requestId, Source(nameof(Update)), ex.Message, ex, ErrorClassifier.Classify(ex), ct);
            throw;
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        long requestId = await _logService.LogRequestAsync(JsonSerializer.Serialize(new { id }), username: null, ct);

        try
        {
            await _businessLogic.DeleteAsync(id, ct);

            await _logService.LogSuccessAsync(requestId, Source(nameof(Delete)), JsonSerializer.Serialize(new { id }), ct);
            return NoContent();
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(requestId, Source(nameof(Delete)), ex.Message, ex, ErrorClassifier.Classify(ex), ct);
            throw;
        }
    }

    private static string Source(string action)
    {
        return nameof(TestKubeController) + "." + action;
    }
}
