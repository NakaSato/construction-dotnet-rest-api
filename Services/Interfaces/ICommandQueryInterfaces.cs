using dotnet_rest_api.Common;

namespace dotnet_rest_api.Services.Interfaces;

/// <summary>
/// Command and Query interfaces for CQRS pattern implementation
/// </summary>

#region Command Interfaces

public interface ICommand<TResult>
{
    Task<Result<TResult>> ExecuteAsync();
}

public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<Result<TResult>> HandleAsync(TCommand command);
}

#endregion

#region Query Interfaces

public interface IQuery<TResult>
{
    Task<Result<TResult>> ExecuteAsync();
}

public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<Result<TResult>> HandleAsync(TQuery query);
}

#endregion
