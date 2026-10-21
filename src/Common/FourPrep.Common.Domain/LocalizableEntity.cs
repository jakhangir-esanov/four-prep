namespace FourPrep.Common.Domain;

public abstract class LocalizableEntity : ILocalizableEntity
{
    public string Name { get; set; } = null!;

    public string NameRu { get; set; } = null!;

    public string NameUz { get; set; } = null!;
}
