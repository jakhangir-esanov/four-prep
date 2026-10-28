namespace FourPrep.Common.Application.Behaviors;

using FluentValidation;
using System.Text.Json;
using FourPrep.Common.Domain;
using System.Globalization;
using FluentValidation.Results;
using FluentValidation.Internal;

public static class ValidationFailureExtensions
{
    private static readonly CultureInfo Uzbek = CultureInfo.GetCultureInfo("uz");
    private static readonly CultureInfo Russian = CultureInfo.GetCultureInfo("ru");

    public static FieldError ToFieldError(this ValidationFailure failure) =>
        failure.ToFieldError(ToJsonPath(failure.PropertyName));

    public static FieldError ToFieldError(this ValidationFailure failure, string propertyName) =>
        new(failure.ErrorCode, failure.GetMessages(), propertyName);

    private static LocalizedMessage GetMessages(this ValidationFailure failure)
    {
        if (failure.CustomState is Error catalogError)
        {
            return catalogError.Messages;
        }

        return new LocalizedMessage(
            En: failure.ErrorMessage,
            Uz: failure.TranslateTo(Uzbek),
            Ru: failure.TranslateTo(Russian));
    }

    private static string TranslateTo(this ValidationFailure failure, CultureInfo language)
    {
        string template = ValidatorOptions.Global.LanguageManager.GetString(failure.ErrorCode, language);

        if (string.IsNullOrEmpty(template))
        {
            return failure.ErrorMessage;
        }

        MessageFormatter formatter = ValidatorOptions.Global.MessageFormatterFactory();

        foreach ((string placeholder, object value) in failure.FormattedMessagePlaceholderValues)
        {
            formatter.AppendArgument(placeholder, value);
        }

        return formatter.BuildMessage(template);
    }

    private static string ToJsonPath(string propertyPath) =>
        string.Join('.', propertyPath.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));
}
