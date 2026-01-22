namespace Cirreum.State;

sealed class StateHandle<T>(T value, Action onChanged)
	: IStateValueHandle<T>, ISerializableStateHandle
	where T : notnull {

	private T _value = value;
	public T Value {
		get {
			return _value;
		}
		private set {
			this._value = value;
		}
	}

	public Task SetValue(T newValue) {
		this.Value = newValue;
		onChanged();
		return Task.CompletedTask;
	}

	public void ResetValue(T defaultValue) {
		this.Value = defaultValue;
	}

	public void ResetValue(object defaultValue) {
		if (defaultValue is T typedValue) {
			this.ResetValue(typedValue);
			return;
		}
	}

	public void Deconstruct(out T value, out Func<T, Task> setter) {
		value = this.Value;
		setter = this.SetValue;
	}

	object ISerializableStateHandle.GetSerializableValue() => this.Value;

}
