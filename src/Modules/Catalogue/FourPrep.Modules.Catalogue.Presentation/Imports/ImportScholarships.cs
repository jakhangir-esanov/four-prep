namespace FourPrep.Modules.Catalogue.Presentation.Imports;

using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Catalogue.Application.Imports.ImportScholarships;

internal sealed class ImportScholarships : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapImport("admin/scholarships/import", file => new ImportScholarshipsCommand(file));
    }
}
