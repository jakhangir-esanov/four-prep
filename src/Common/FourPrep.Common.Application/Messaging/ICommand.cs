namespace FourPrep.Common.Application.Messaging;

using MediatR;
using FourPrep.Common.Domain;

public interface ICommand : IRequest<Result>, IBaseCommand;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;

public interface IBaseCommand;
