using System;
using DG.Tweening;

public class PinHint : ManagedHintObject
{
	private void Start()
	{
		base.transform.SetAsFirstSibling();
	}

	protected override void Show()
	{
		base.transform.SetAsFirstSibling();
		ShortcutExtensions.DOFade(this.group, 1f, 0.1f);
		if (StaticUserData.CurrentPond == null)
		{
			SetPondsOnGlobalMap.GlobeHelp.SetActiveHint((this.observer == null) ? null : this.observer.Message.ElementId);
		}
	}

	protected override void Hide()
	{
		ShortcutExtensions.DOFade(this.group, 0f, 0.1f);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (StaticUserData.CurrentPond == null)
		{
			SetPondsOnGlobalMap.GlobeHelp.SetActiveHint(null);
		}
	}
}
