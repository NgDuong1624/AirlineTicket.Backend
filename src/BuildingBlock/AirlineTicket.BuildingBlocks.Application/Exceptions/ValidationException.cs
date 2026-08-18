using FluentValidation.Results;
using System.Collections.Generic;
using System.Linq;

namespace AirlineTicket.BuildingBlocks.Exceptions;

public class ValidationFailureInfo
{
    public string PropertyName { get; }
    public string ErrorCode { get; }
    public string ErrorMessage { get; }
    public object[]? CustomArgs { get; }

    public ValidationFailureInfo(string propertyName, string errorCode, string errorMessage, object[]? customArgs = null)
    {
        PropertyName = propertyName;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        CustomArgs = customArgs;
    }
}

public class ValidationException : CustomException
{
    public IDictionary<string, string[]> Errors { get; }
    public IReadOnlyList<ValidationFailureInfo> FailureInfos { get; }

    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
        FailureInfos = new List<ValidationFailureInfo>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        var failureList = failures.ToList();
        Errors = failureList
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());

        var infos = new List<ValidationFailureInfo>();
        foreach (var f in failureList)
        {
            var args = new List<object> { f.PropertyName };
            if (f.FormattedMessagePlaceholderValues != null)
            {
                if (f.FormattedMessagePlaceholderValues.TryGetValue("MinLength", out var minLen)) args.Add(minLen);
                else if (f.FormattedMessagePlaceholderValues.TryGetValue("From", out var fromVal)) args.Add(fromVal);

                if (f.FormattedMessagePlaceholderValues.TryGetValue("MaxLength", out var maxLen)) args.Add(maxLen);
                else if (f.FormattedMessagePlaceholderValues.TryGetValue("To", out var toVal)) args.Add(toVal);
            }

            var errorCode = f.ErrorCode switch
            {
                "NotEmptyValidator" => "Validation.Required",
                "NotNullValidator" => "Validation.Required",
                "EmailValidator" => "Validation.InvalidEmail",
                "LengthValidator" => "Validation.Range",
                "ExactLengthValidator" => "Validation.Range",
                "MinimumLengthValidator" => "Validation.MinLength",
                "MaximumLengthValidator" => "Validation.MaxLength",
                "InclusiveBetweenValidator" => "Validation.Range",
                "ExclusiveBetweenValidator" => "Validation.Range",
                _ => f.ErrorCode
            };

            infos.Add(new ValidationFailureInfo(f.PropertyName, errorCode, f.ErrorMessage, args.ToArray()));
        }
        FailureInfos = infos;
    }
}
