using System;

[TriggerName(Name = "Caught fishes same category")]
[Serializable]
public class SameCategoryCaught : TutorialTrigger
{
	public override void AcceptArguments(string[] arguments)
	{
		if (arguments == null || arguments.Length != 1)
		{
			throw new ArgumentException("arguments array for SameCategoryCaught has to contain 1 argument");
		}
		this.FishNumber = int.Parse(arguments[0]);
	}

	public override bool IsTriggering()
	{
		return this.FishNumber == this._fishAccumulator;
	}

	public override void Update()
	{
		base.Update();
		if (!this._eventSigned && GameFactory.FishSpawner != null)
		{
			GameFactory.FishSpawner.FishCaught += this.FishSpawnerFishCaught;
			this._eventSigned = true;
		}
	}

	private void FishSpawnerFishCaught(object sender, FishCaughtEventArgs e)
	{
		if (e.CaughtFish.CategoryId != this._category)
		{
			this._fishAccumulator = 0;
		}
		if (this._fishAccumulator == 0)
		{
			this._category = e.CaughtFish.CategoryId;
		}
		this._fishAccumulator++;
	}

	[TriggerVariable(Name = "Number")]
	public int FishNumber;

	private int _fishAccumulator;

	private bool _eventSigned;

	private int _category = -1;
}
