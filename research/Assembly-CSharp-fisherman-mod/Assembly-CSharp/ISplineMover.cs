using System;
using SWS;

public interface ISplineMover
{
	event Action EEndOfSpline;

	void Pause(float seconds = 0f);

	void Resume();

	void ChangeSpeed(float speed);

	void SetPath(PathManager path, float timePrc, float movementSpeed);
}
