using System;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using ObjectModel;

public class LureAssemblySlides : SlideUnit
{
	public LureAssemblySlides()
	{
		this.AvailableLevel = 2;
		this.Caption = "FireworkSlidesCaption";
		this.IsActive = true;
		this.Slides.Add(new Slide
		{
			Text = "FireworkSlide1Text",
			ImageName = "fireworks_tutor_1"
		});
		this.Slides.Add(new Slide
		{
			Text = "FireworkSlide2Text",
			ImageName = "fireworks_tutor_2"
		});
		this.Slides.Add(new Slide
		{
			Text = "FireworkSlide3Text",
			ImageName = "fireworks_tutor_3"
		});
	}

	public override bool CanRun()
	{
		if (StaticUserData.CurrentLocation != null && StaticUserData.CurrentLocation.LocationId == 10136)
		{
			if (PhotonConnectionFactory.Instance.Profile.Inventory.Any((InventoryItem x) => x.ItemSubType == ItemSubTypes.Firework) && LureAssemblySlides._pondHelpers.PondControllerList != null)
			{
				return LureAssemblySlides._pondHelpers.PondControllerList.Game3DPond.activeSelf;
			}
		}
		return false;
	}

	private static PondHelpers _pondHelpers = new PondHelpers();
}
