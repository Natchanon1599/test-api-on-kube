using Microsoft.AspNetCore.Mvc;
using PrototypeLibrary.Models.Services;
namespace PrototypeApi.Interfaces
{
    public interface IPrototypeService
    {
        Task<ActionResult<JsonTest>> GetData();
    }
}