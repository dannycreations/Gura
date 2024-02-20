using System;

public class BaseSpinningSlides : SlideUnit
{
	public BaseSpinningSlides()
	{
		this.AvailableLevel = 5;
		this.Caption = "BaseSpinningSlidesCaption";
		this.IsActive = true;
		this.Slides.Add(new Slide
		{
			Text = "TestText",
			ImageName = "Slide1"
		});
		this.Slides.Add(new Slide
		{
			Text = "TestText",
			ImageName = "Slide2"
		});
		this.Slides.Add(new Slide
		{
			Text = "TestText",
			ImageName = "Slide3"
		});
	}

	public override bool CanRun()
	{
		return false;
	}
}
