namespace Cirreum.Security;

sealed class UserAccessor(IUserState user) : IUserStateAccessor {
	public ValueTask<IUserState> GetUser() => ValueTask.FromResult(user);
}