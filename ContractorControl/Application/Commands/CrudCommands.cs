using ContractorControl.Application.DTOs;
using MediatR;

namespace ContractorControl.Application.Commands;

public record InsertCommand(CrudRequestDto Request) : IRequest<ApiResponse>;
public record UpdateCommand(CrudRequestDto Request) : IRequest<ApiResponse>;
public record SoftDeleteCommand(CrudRequestDto Request) : IRequest<ApiResponse>;
