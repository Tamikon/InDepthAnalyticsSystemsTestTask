using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TestApi.Models;
using TestApi.Services;

namespace TestApi.Controllers;

[ApiController]
[Route("api")]
public class ApiController : ControllerBase
{
    private readonly IPageProcessingService _service;
    private readonly IValidator<RequestModel> _validator;
    private readonly ILogger<ApiController> _logger;

    public ApiController(
        IPageProcessingService service,
        IValidator<RequestModel> validator,
        ILogger<ApiController> logger)
    {
        _service = service;
        _validator = validator;
        _logger = logger;
    }

    [HttpPost("process")]
    public async Task<IActionResult> Process([FromBody] RequestModel? request)
    {
        if (request is null)
        {
            var error = new ResponseModel
            {
                IsError = 1,
                ErrorCode = "MISSING_BODY",
                ErrorMessage = "Request body is missing or invalid JSON",
                ElementsCount = 0,
                EmailsCount = 0,
                ElementsAttrList = new List<string>(),
                EmailsList = new List<string>()
            };
            return Content(JsonSerializer.Serialize(error, PrettyJsonOptions), "application/json");
        }

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            var error = new ResponseModel
            {
                IsError = 1,
                ErrorCode = "VALIDATION_ERROR",
                ErrorMessage = errors,
                ElementsCount = 0,
                EmailsCount = 0,
                ElementsAttrList = new List<string>(),
                EmailsList = new List<string>()
            };
            return Content(JsonSerializer.Serialize(error, PrettyJsonOptions), "application/json");
        }

        var result = await _service.ProcessAsync(request);

        var json = JsonSerializer.Serialize(result, PrettyJsonOptions);
        return Content(json, "application/json");
    }

    private static readonly JsonSerializerOptions PrettyJsonOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
}
