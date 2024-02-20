using System;

public interface IBoatWithEngineMinigameView
{
	bool IsEngineIgnitionSuccess { get; }

	void SetActiveMinigame(bool flag);

	void FinishEngineIgnition();

	void FinishEngineIgnitionWithHighlight();

	void ShakeEngine(bool success);

	void IndicateFail();

	void SetupForEngineIgnition(float v);

	void SetValue(float v);

	void SetValueAndIndication(float v);

	void SetActiveEngine(bool flag);

	void SetEngineIndicatorOn(bool value);
}
