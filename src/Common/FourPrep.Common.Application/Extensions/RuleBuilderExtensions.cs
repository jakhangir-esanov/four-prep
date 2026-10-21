namespace FourPrep.Common.Application.Extensions;

using FluentValidation;
using FourPrep.Common.Domain;

public static class RuleBuilderExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        Error error)
    {
        return rule
            .WithErrorCode(error.Code)
            .WithMessage(error.Message)
            .WithState(_ => error);
    }
}
