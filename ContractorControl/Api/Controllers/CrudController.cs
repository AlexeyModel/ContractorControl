using ContractorControl.Application.DTOs;
using ContractorControl.Application.Services;
using ContractorControl.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ContractorControl.Api.Controllers;

[Route("api/crud")]
[ApiController]
public class CrudController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ICrudService _crudService;

    public CrudController(IConfiguration configuration, ICrudService crudService)
    {
        _configuration = configuration;
        _crudService = crudService;
    }

    private bool ValidateKey(string key) => _configuration["SecretKey"] == key;

    [HttpPost("insert")]
    public async Task<IActionResult> Insert([FromBody] SetPropertyInfo body)
    {
        if (!ValidateKey(body.SecretKey!))
            return Unauthorized(new ApiResponse { Status = false, Message = "Invalid Secret Key" });

        return Ok(await _crudService.InsertAsync(body));
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] SetPropertyInfo body)
    {
        if (!ValidateKey(body.SecretKey!))
            return Unauthorized(new ApiResponse { Status = false, Message = "Invalid Secret Key" });

        return Ok(await _crudService.UpdateAsync(body));
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete([FromBody] SetPropertyInfo body)
    {
        if (!ValidateKey(body.SecretKey!))
            return Unauthorized(new ApiResponse { Status = false, Message = "Invalid Secret Key" });

        return Ok(await _crudService.SoftDeleteAsync(body));
    }
}
