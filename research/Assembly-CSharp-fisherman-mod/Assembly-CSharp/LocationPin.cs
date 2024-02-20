using System;
using UnityEngine;
using UnityEngine.UI;

public class LocationPin : MonoBehaviour
{
	public LocationPin.ViewType CurrentType { get; set; }

	private void ApplyColorSet(LocationPin.ColorSet set)
	{
		this.ColorTransitions.HighlightSpriteColor = set.HighlightSpriteColor;
		this.ColorTransitions.ActiveSpriteColor = set.ActiveSpriteColor;
		this.ColorTransitions.NormalSpriteColor = set.NormalSpriteColor;
		this.ColorTransitions.PressSpriteColor = set.PressSpriteColor;
		this.ColorTransitions.DisableSpriteColor = set.DisableSpriteColor;
	}

	public void SetType(LocationPin.ViewType t)
	{
		this.CurrentType = t;
		this.PersonIcon.gameObject.SetActive(t == LocationPin.ViewType.Usual);
		this.BoatIcon.gameObject.SetActive(t == LocationPin.ViewType.Boat);
		this.RestoreIcon.gameObject.SetActive(t == LocationPin.ViewType.Restored);
		this.RestoreBoatIcon.gameObject.SetActive(t == LocationPin.ViewType.RestoredBoat);
		if (t == LocationPin.ViewType.Usual || t == LocationPin.ViewType.Boat)
		{
			this.ApplyColorSet(this.NormalColorSet);
		}
		else
		{
			this.ApplyColorSet(this.RestoredColorSet);
		}
	}

	public LocationPin.ColorSet NormalColorSet;

	public LocationPin.ColorSet RestoredColorSet;

	[Space(10f)]
	public Graphic PersonIcon;

	public Graphic BoatIcon;

	public Graphic RestoreIcon;

	public Graphic RestoreBoatIcon;

	[Space(10f)]
	public RequestLocationDescriptioAction RLDA;

	public DoubleClickAction DoubleClick;

	public ToggleColorTransitionChanges ColorTransitions;

	public Toggle Toggle;

	public HintElementId ElementId;

	public GameObject TutorialArrow;

	[Serializable]
	public class ColorSet
	{
		public Color HighlightSpriteColor;

		public Color ActiveSpriteColor;

		public Color NormalSpriteColor;

		public Color PressSpriteColor;

		public Color DisableSpriteColor;
	}

	public enum ViewType
	{
		Usual,
		Boat,
		Restored,
		RestoredBoat
	}
}
