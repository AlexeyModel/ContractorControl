using ContractorControl.Application.DTOs;
using ContractorControl.Domain.Common;
using ContractorControl.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ContractorControl.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InitController : ControllerBase
{
    private readonly InitService _initService;
    private readonly IConfiguration _configuration;

    public InitController(InitService initService, IConfiguration configuration)
    {
        _initService = initService;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> Init([FromBody] SecretInfo body)
    {
        if (body.SecretKey.Length > 0 && _configuration["SecretKey"] != body.SecretKey)
            return Unauthorized(new ApiResponse { Status = false, Message = "Invalid Secret Key" });

        var files = new Dictionary<string, string>() { { "spCheckIsFinished", _configuration["CheckIsFinished"] }, { "spCheckStateToSet", _configuration["CheckStateToSet"] } }; 

        await _initService.InitializeDatabaseAsync(files);
        return Ok(new ApiResponse { Status = true, Message = "Database initialized" });
    }
}
