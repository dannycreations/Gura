using System;

public interface ITournamentDetails
{
	event EventHandler<EventArgs> ApplyOnClick;

	event Action OnUnregister;
}
