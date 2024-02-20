using System;

public class GameStateChanger : ActivityStateControlled
{
	protected override void Start()
	{
		base.Start();
		if (this.OnStart)
		{
			GameState.State = this.SetState;
		}
	}

	protected override void SetHelp()
	{
		if (this.OnEnabling)
		{
			GameState.State = this.SetState;
		}
	}

	public GameStates SetState;

	public bool OnEnabling;

	public bool OnStart;
}
