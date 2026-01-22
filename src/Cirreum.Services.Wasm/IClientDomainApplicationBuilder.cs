namespace Cirreum;

/// <summary>
/// Provides a mechanism to configure and build a client-specific domain application.
/// </summary>
/// <remarks>This interface extends <see cref="IDomainApplicationBuilder"/> to include functionality specific to
/// client domain applications. It is typically used during the application setup phase to configure client-specific
/// services, settings, or behaviors.</remarks>
public interface IClientDomainApplicationBuilder : IDomainApplicationBuilder {

}