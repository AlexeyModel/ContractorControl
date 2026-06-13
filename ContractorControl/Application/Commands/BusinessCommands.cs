using ContractorControl.Application.DTOs;
using MediatR;

namespace ContractorControl.Application.Commands;

public record CreateContractExecutionCommand(ContractExecutionCreateDto Dto) : IRequest<ApiResponse>;
public record SetStateCommand(SetStateDto Dto) : IRequest<ApiResponse>;
