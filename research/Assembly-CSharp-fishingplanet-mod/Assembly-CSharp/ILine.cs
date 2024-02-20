using System;

public interface ILine
{
	int ItemId { get; }

	string Asset { get; }

	string LineOnSpoolAsset { get; }

	string LineOnBaitcastSpoolAsset { get; }

	string Color { get; }
}
