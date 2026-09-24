using TestApi.Models;

namespace TestApi.Services;

public interface IPageProcessingService
{
    Task<ResponseModel> ProcessAsync(RequestModel request);
}
