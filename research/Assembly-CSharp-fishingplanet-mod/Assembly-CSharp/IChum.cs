using System;

public interface IChum
{
	Guid? InstanceId { get; }

	string Asset { get; }

	int ItemId { get; }

	bool IsExpired { get; }

	bool WasThrown { get; set; }
}
