namespace FourPrep.Common.Application.Messaging;

using MediatR;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Paging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;

public interface IPagedListQuery<TResponse> : IRequest<PagedList<TResponse>>;
