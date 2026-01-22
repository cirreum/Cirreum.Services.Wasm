namespace Cirreum.State;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extends <see cref="IStateBuilder"/> with access to services for data store registration.
/// </summary>
public interface IStateBuilderWithDataStores : IStateBuilder {
	/// <summary>
	/// Gets the collection of service descriptors for dependency injection configuration.
	/// </summary>
	/// <remarks>Use this property to register application services and configure dependency injection.
	/// Modifications to the collection affect the services available at runtime.</remarks>
	IServiceCollection Services { get; }
}