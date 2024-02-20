using System;
using System.Collections.Generic;

public abstract class SlideUnit
{
	public abstract bool CanRun();

	public int AvailableLevel = 2;

	public string Caption;

	public List<Slide> Slides = new List<Slide>();

	public bool IsActive;
}
