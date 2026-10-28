namespace FourPrep.Modules.Catalogue.Presentation.Imports;

using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Catalogue.Application.Imports.ImportPrograms;

internal sealed class ImportPrograms : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapImport("admin/programs/import", file => new ImportProgramsCommand(file));
    }
}
