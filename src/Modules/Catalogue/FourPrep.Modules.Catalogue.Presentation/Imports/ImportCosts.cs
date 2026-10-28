namespace FourPrep.Modules.Catalogue.Presentation.Imports;

using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Catalogue.Application.Imports.ImportCosts;

internal sealed class ImportCosts : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapImport("admin/costs/import", file => new ImportCostsCommand(file));
    }
}
