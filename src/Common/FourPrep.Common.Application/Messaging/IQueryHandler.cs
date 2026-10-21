namespace FourPrep.Common.Application.Messaging;

using MediatR;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Paging;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;

public interface IPagedListQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, PagedList<TResponse>>
    where TQuery : IPagedListQuery<TResponse>;
