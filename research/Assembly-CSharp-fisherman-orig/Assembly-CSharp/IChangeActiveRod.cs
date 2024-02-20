using System;

public interface IChangeActiveRod
{
	event Action ChangedActiveRod;

	InitRod ActiveRod { get; set; }
}
