namespace Cirreum.Security;

sealed class UserAccessor(IUserState user) : IUserStateAccessor {
	public ValueTask<IUserState> GetUserState() => ValueTask.FromResult(user);
}