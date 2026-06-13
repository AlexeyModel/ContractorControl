using ContractorControl.Application.DTOs;
using MediatR;

namespace ContractorControl.Application.Queries;

public record CheckStateIsSetQuery(CheckStateDto Dto) : IRequest<ApiResponse>;
public record CheckStateToSetQuery(CheckStateDto Dto) : IRequest<ApiResponse>;
public record CheckIsFinishedQuery(InstanceDto Dto) : IRequest<ApiResponse>;
public record SetFinishedQuery(InstanceDto Dto) : IRequest<ApiResponse>;
public record GetEventsQuery(InstanceDto Dto) : IRequest<ApiResponse>;
