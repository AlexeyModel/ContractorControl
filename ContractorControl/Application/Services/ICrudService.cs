using ContractorControl.Application.DTOs;
using ContractorControl.Domain.Common;
using System.Text.Json;

namespace ContractorControl.Application.Services;

public interface ICrudService
{
    Task<ApiResponse> InsertAsync(SetPropertyInfo body);
    Task<ApiResponse> UpdateAsync(SetPropertyInfo body);
    Task<ApiResponse> SoftDeleteAsync(SetPropertyInfo body);
}
