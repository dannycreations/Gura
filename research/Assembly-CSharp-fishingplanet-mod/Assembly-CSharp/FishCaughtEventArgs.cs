using System;
using ObjectModel;

public class FishCaughtEventArgs : EventArgs
{
	public Fish CaughtFish;

	public bool IsIllegal;

	public bool IsTrophy;

	public bool IsUnique;

	public bool CanTake;

	public bool CanRelease;

	public int Penalty;
}
