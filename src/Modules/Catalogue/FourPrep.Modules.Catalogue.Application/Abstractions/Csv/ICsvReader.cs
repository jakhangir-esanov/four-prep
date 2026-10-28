namespace FourPrep.Modules.Catalogue.Application.Abstractions.Csv;

using FourPrep.Common.Domain;

public interface ICsvReader
{
    Result<CsvTable> Read(Stream stream);
}
