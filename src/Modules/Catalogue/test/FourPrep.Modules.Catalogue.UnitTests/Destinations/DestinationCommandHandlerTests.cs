namespace FourPrep.Modules.Catalogue.UnitTests.Destinations;

using FourPrep.Modules.Catalogue.Domain.Destinations;
using FourPrep.Modules.Catalogue.Application.Destinations.SetDestination;
using FourPrep.Modules.Catalogue.Application.Destinations.RemoveDestination;

public class DestinationCommandHandlerTests
{
    [Fact]
    public async Task Set_AddsANewDestinationWithANormalizedCountryCode()
    {
        var repository = new FakeDestinationRepository();
        var unitOfWork = new FakeUnitOfWork();

        var result = await new SetDestinationCommandHandler(unitOfWork, repository, new FakeClock())
            .Handle(new SetDestinationCommand(" de ", " National visa (D) "), default);

        Assert.True(result.IsSuccess);
        var destination = Assert.Single(repository.Destinations);
        Assert.Equal("DE", destination.CountryCode);
        Assert.Equal("National visa (D)", destination.VisaType);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Set_ChangesTheVisaTypeOfAnExistingDestination()
    {
        var repository = new FakeDestinationRepository();
        var existing = repository.Add("US", "J-1");
        var unitOfWork = new FakeUnitOfWork();

        var result = await new SetDestinationCommandHandler(unitOfWork, repository, new FakeClock())
            .Handle(new SetDestinationCommand("us", "F-1"), default);

        Assert.True(result.IsSuccess);
        Assert.Same(existing, Assert.Single(repository.Destinations));
        Assert.Equal("F-1", existing.VisaType);
        Assert.Equal(new FakeClock().UtcNow, existing.UpdatedOnUtc);
    }

    [Fact]
    public async Task Remove_DeletesTheDestination()
    {
        var repository = new FakeDestinationRepository();
        repository.Add("US");
        var unitOfWork = new FakeUnitOfWork();

        var result = await new RemoveDestinationCommandHandler(unitOfWork, repository)
            .Handle(new RemoveDestinationCommand("us"), default);

        Assert.True(result.IsSuccess);
        Assert.Empty(repository.Destinations);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Remove_ReportsACountryThatIsNotADestination()
    {
        var repository = new FakeDestinationRepository();
        var unitOfWork = new FakeUnitOfWork();

        var result = await new RemoveDestinationCommandHandler(unitOfWork, repository)
            .Handle(new RemoveDestinationCommand("FR"), default);

        Assert.Equal(DestinationErrors.NotFound, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }
}
