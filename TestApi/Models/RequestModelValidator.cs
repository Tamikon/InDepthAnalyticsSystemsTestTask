using FluentValidation;

namespace TestApi.Models;

public class RequestModelValidator : AbstractValidator<RequestModel>
{
    public RequestModelValidator()
    {
        RuleFor(x => x.Selector)
            .NotNull().WithMessage("Selector is required")
            .NotEmpty().WithMessage("Selector cannot be empty");

        RuleFor(x => x.Attribute)
            .NotNull().WithMessage("Attribute is required")
            .NotEmpty().WithMessage("Attribute cannot be empty");

        RuleFor(x => x.Url_b64)
            .NotNull().WithMessage("Url_b64 is required")
            .NotEmpty().WithMessage("Url_b64 cannot be empty");

        RuleFor(x => x.Encrypted_text_bytes_b64)
            .NotNull().WithMessage("Encrypted_text_bytes_b64 is required")
            .NotEmpty().WithMessage("Encrypted_text_bytes_b64 cannot be empty");

        RuleFor(x => x.Key_bytes_b64)
            .NotNull().WithMessage("Key_bytes_b64 is required")
            .NotEmpty().WithMessage("Key_bytes_b64 cannot be empty");

        RuleFor(x => x.Page_b64)
            .NotNull().WithMessage("Page_b64 is required")
            .NotEmpty().WithMessage("Page_b64 cannot be empty");
    }
}
