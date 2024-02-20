using System;

public class BuyItemInLocalShopToStorage : SlideUnit
{
	public BuyItemInLocalShopToStorage()
	{
		this.Caption = "LocalShopTutorialCaption";
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
