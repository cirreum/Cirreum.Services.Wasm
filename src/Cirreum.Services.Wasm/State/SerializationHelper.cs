namespace Cirreum.State;

using System.Collections;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

internal static class SerializationHelper {

	public static bool IsSerializable(Type type) {

		// Handle primitive types and string
		if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal)) {
			return true;
		}

		// Handle nullable types
		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
			return IsSerializable(Nullable.GetUnderlyingType(type)!);
		}

		// Handle arrays
		if (type.IsArray) {
			return IsSerializable(type.GetElementType()!);
		}

		// Handle collections
		if (typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType) {
			return IsSerializable(type.GetGenericArguments()[0]);
		}

		// Handle records
		if (IsRecord(type)) {
			return true;
		}

		// Handle classes and structs
		if (type.IsClass || type.IsValueType) {

			// Check for [JsonSerializable] attribute
			if (type.GetCustomAttributes(typeof(JsonSerializableAttribute), true).Length > 0) {
				return true;
			}

			// Check for DataContract attribute
			if (type.GetCustomAttributes(typeof(DataContractAttribute), true).Length > 0) {
				return true;
			}

			// For classes, ensure they have a parameterless constructor
			if (type.IsClass && !type.IsAbstract) {
				var hasParameterlessCtor = type.GetConstructor(Type.EmptyTypes) != null;
				if (!hasParameterlessCtor) {
					return false;
				}
			}

			// Check if all properties are serializable
			var properties = type.GetProperties()
				.Where(p =>
					p.CanRead
					&& p.CanWrite
					&& p.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Length == 0);

			return properties.All(p => IsSerializable(p.PropertyType));
		}

		return false;
	}

	private static bool IsRecord(Type type) {
		// Look for the compiler-generated method "<Clone>$" (specific to records)
		return type.GetMethods().Any(m => m.Name == "<Clone>$");
	}

	public static void ValidateType<T>() where T : notnull {
		var type = typeof(T);
		if (!IsSerializable(type)) {
			var error = GetTypeValidationError(type);
			throw new ArgumentException($"Type {type.Name} is not serializable: {error}", nameof(T));
		}
	}

	private static string GetTypeValidationError(Type type) {

		if (type.IsInterface) {
			return "Interfaces cannot be serialized";
		}

		if (type.IsAbstract) {
			return "Abstract classes cannot be serialized";
		}

		if (type.IsClass && !type.IsArray && type != typeof(string)) {
			var ctor = type.GetConstructor(Type.EmptyTypes);
			if (ctor == null) {
				return "Classes must have a parameterless constructor";
			}
		}

		if (typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType) {
			var elementType = type.GetGenericArguments()[0];
			if (!IsSerializable(elementType)) {
				return $"Collection element type {elementType.Name} is not serializable";
			}
		}

		var invalidProperties = type.GetProperties()
			.Where(p => p.CanRead && p.CanWrite && p.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Length == 0)
			.Where(p => !IsSerializable(p.PropertyType))
			.Select(p => $"{p.Name} ({p.PropertyType.Name})")
			.ToList();

		if (invalidProperties.Count != 0) {
			return $"The following properties are not serializable: {string.Join(", ", invalidProperties)}";
		}

		return "Unknown serialization error";
	}

}