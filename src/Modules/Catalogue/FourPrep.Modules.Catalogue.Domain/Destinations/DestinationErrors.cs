namespace FourPrep.Modules.Catalogue.Domain.Destinations;

using FourPrep.Common.Domain;

public static class DestinationErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Destination.NotFound",
        en: "This country is not a service destination.",
        uz: "Bu davlat xizmat yo'nalishlari ro'yxatida yo'q.",
        ru: "Эта страна не входит в список направлений.");
}
