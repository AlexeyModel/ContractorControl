using ContractorControl.Application.Commands;
using ContractorControl.Application.DTOs;
using ContractorControl.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ContractorControl.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ContractExecutionController : ControllerBase
{
    private readonly IMediator _mediator;
    public ContractExecutionController(IMediator mediator) => _mediator = mediator;

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] ContractExecutionCreateDto dto) 
        => Ok(await _mediator.Send(new CreateContractExecutionCommand(dto)));

    [HttpPost("checkIsFinished")]
    public async Task<IActionResult> CheckIsFinished([FromBody] InstanceDto dto) 
        => Ok(await _mediator.Send(new CheckIsFinishedQuery(dto)));

    [HttpPost("setFinished")]
    public async Task<IActionResult> SetFinished([FromBody] InstanceDto dto)
        => Ok(await _mediator.Send(new SetFinishedQuery(dto)));
}
