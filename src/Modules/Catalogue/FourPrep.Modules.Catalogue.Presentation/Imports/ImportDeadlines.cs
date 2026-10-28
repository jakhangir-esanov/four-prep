namespace FourPrep.Modules.Catalogue.Presentation.Imports;

using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Catalogue.Application.Imports.ImportDeadlines;

internal sealed class ImportDeadlines : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapImport("admin/deadlines/import", file => new ImportDeadlinesCommand(file));
    }
}
