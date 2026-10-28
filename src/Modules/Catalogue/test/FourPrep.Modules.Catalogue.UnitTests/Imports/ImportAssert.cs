namespace FourPrep.Modules.Catalogue.UnitTests.Imports;

using FourPrep.Common.Domain;

internal static class ImportAssert
{
    public static void HasError(Result result, string propertyName, Error error)
    {
        var errors = Assert.IsType<ValidationError>(result.Error).Errors.Cast<FieldError>();

        Assert.Contains(errors, field => field.PropertyName == propertyName && field.Code == error.Code);
    }

    public static void HasError(Result result, string propertyName, string code)
    {
        var errors = Assert.IsType<ValidationError>(result.Error).Errors.Cast<FieldError>();

        Assert.Contains(errors, field => field.PropertyName == propertyName && field.Code == code);
    }
}
