namespace Cirreum.State;

/// <summary>
/// Represents a handle to a serializable state object.
/// </summary>
/// <remarks>This interface is designed to provide access to an object that can be serialized. Implementations of
/// this interface should ensure that the returned value is suitable for serialization.</remarks>
internal interface ISerializableStateHandle {
	/// <summary>
	/// Retrieves a value that can be serialized.
	/// </summary>
	/// <returns>An object that is suitable for serialization. The returned value may be null if no serializable value is available.</returns>
	object GetSerializableValue();
}