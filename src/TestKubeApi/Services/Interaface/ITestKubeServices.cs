using Microsoft.AspNetCore.Mvc;
using TestKubeLibrary.Models.Services;
namespace TestKubeApi.Interfaces
{
    public interface ITestKubeService
    {
        Task<ActionResult<JsonTest>> GetData();
    }
}