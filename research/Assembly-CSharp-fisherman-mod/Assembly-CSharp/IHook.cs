using System;

public interface IHook
{
	string Asset { get; }

	int ItemId { get; }

	float HookSize { get; }
}
