using ContractorControl.Application.Commands;
using ContractorControl.Application.DTOs;
using ContractorControl.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ContractorControl.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StatesController : ControllerBase
{
    private readonly IMediator _mediator;
    public StatesController(IMediator mediator) => _mediator = mediator;

    [HttpPost("setStates")]
    public async Task<IActionResult> SetStates([FromBody] SetStateDto dto) => Ok(await _mediator.Send(new SetStateCommand(dto)));

    [HttpPost("checkStateIsSet")]
    public async Task<IActionResult> CheckStateIsSet([FromBody] CheckStateDto dto) => Ok(await _mediator.Send(new CheckStateIsSetQuery(dto)));

    [HttpPost("checkStateToSet")]
    public async Task<IActionResult> CheckStateToSet([FromBody] CheckStateDto dto) => Ok(await _mediator.Send(new CheckStateToSetQuery(dto)));

    [HttpPost("getEvents")]
    public async Task<IActionResult> GetEvents([FromBody] InstanceDto dto) => Ok(await _mediator.Send(new GetEventsQuery(dto)));
}
