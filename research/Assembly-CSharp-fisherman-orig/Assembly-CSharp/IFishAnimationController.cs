using System;

public interface IFishAnimationController
{
	void StartSwiming();

	void StartBeating();

	void StartShaking();

	void FinishBeating();

	void FinishShaking();
}
