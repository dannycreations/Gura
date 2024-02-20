using System;

public interface IBobberIndicatorController
{
	void Show();

	void Hide();

	float Sensitivity { get; set; }
}
