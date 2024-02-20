using System;

public interface IReel
{
	string Asset { get; }

	int ItemId { get; }

	ReelTypes ReelType { get; }
}
