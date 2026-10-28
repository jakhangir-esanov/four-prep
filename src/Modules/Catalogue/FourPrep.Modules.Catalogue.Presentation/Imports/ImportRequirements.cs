namespace FourPrep.Modules.Catalogue.Presentation.Imports;

using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Catalogue.Application.Imports.ImportRequirements;

internal sealed class ImportRequirements : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapImport("admin/requirements/import", file => new ImportRequirementsCommand(file));
    }
}
