namespace FourPrep.Common.Application.Paging;

public interface IPagingRequest : ISortingRequest
{
    int First { get; init; }
    int Rows { get; init; }
}
