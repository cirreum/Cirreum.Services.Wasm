namespace Cirreum.State;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

sealed class StateBuilder(
	IServiceCollection services
) : IStateBuilder {

	internal IServiceCollection Services => services;

	/// <inheritdoc/>
	public IStateBuilder RegisterState<TInterface, TImplementation>()
		where TInterface : class, IApplicationState
		where TImplementation : class, TInterface {
		services.TryAddScoped<TInterface, TImplementation>();
		return this;
	}

	/// <inheritdoc/>
	public IStateBuilder RegisterState<TImplementation>()
		where TImplementation : class, IApplicationState {
		services.TryAddScoped<TImplementation>();
		return this;
	}

	/// <inheritdoc/>
	public IStateBuilder RegisterRemoteState<TInterface, TImplementation>()
		where TInterface : class, IRemoteState
		where TImplementation : class, TInterface {
		services.TryAddScoped<TInterface, TImplementation>();
		if (typeof(IInitializable).IsAssignableFrom(typeof(TImplementation))) {
			services.AddScoped(sp =>
				(IInitializable)sp.GetRequiredService<TInterface>());
		}
		return this;
	}

	/// <inheritdoc/>
	public IStateBuilder RegisterEncryptor(IStateContainerEncryption encryption) {
		services.AddSingleton(encryption);
		services.AddKeyedSingleton(encryption.AlgorithmId, encryption);
		return this;
	}

	/// <inheritdoc/>
	public IStateBuilder RegisterDecryptor(IStateContainerEncryption previousEncryption) {
		if (previousEncryption.AlgorithmKindId != StateEncryptionKinds.NONE &&
			previousEncryption.AlgorithmKindId != StateEncryptionKinds.BASE64) {
			services.AddKeyedSingleton(previousEncryption.AlgorithmId, previousEncryption);
		}
		return this;
	}

}