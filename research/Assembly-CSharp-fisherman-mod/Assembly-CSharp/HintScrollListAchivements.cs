using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HintScrollListAchivements : HintColorBase
{
	protected override void Init()
	{
		base.Init();
		this.Achievement = base.GetComponentInParent<AchivementsInit>();
		this.Achievement.OnActive += this.Achivements_OnActive;
	}

	private void Achivements_OnActive()
	{
		if (this.Achievement != null)
		{
			this.Achievement.OnActive -= this.Achivements_OnActive;
			if (this.shouldShow)
			{
				this.CreateShine();
			}
		}
	}

	protected override void OnDestroy()
	{
		base.StopAllCoroutines();
		if (this.Achievement != null)
		{
			this.Achievement.OnActive -= this.Achivements_OnActive;
		}
		base.OnDestroy();
	}

	public override void SetObserver(ManagedHint observer, int id)
	{
		base.SetObserver(observer, id);
		this.CreateShine();
	}

	protected virtual void CreateShine()
	{
		if (this.observer != null && this.observer.Message != null && this._shines.Count == 0)
		{
			int value = this.observer.Message.Value;
			int num = 0;
			AchivementsInit achievement = this.Achievement;
			for (int i = 0; i < achievement.ContentPanel.transform.childCount; i++)
			{
				AchivInit component = achievement.ContentPanel.transform.GetChild(i).GetComponent<AchivInit>();
				if (component.AchivementId == value)
				{
					num = i;
					GameObject gameObject = Object.Instantiate<GameObject>(this._shinePrefab, component.transform);
					this._shines.Add(gameObject.GetComponent<RectTransform>());
					break;
				}
			}
			if (num > 0)
			{
				Scrollbar scrollBar = achievement.ScrollBar;
				float num2 = (float)num / (float)achievement.ContentPanel.transform.childCount;
				if (scrollBar.direction == 2)
				{
					num2 = 1f - num2;
				}
				base.StartCoroutine(this.Scroll(achievement.ScrollRect, num2));
			}
			return;
		}
	}

	protected IEnumerator Scroll(ScrollRect sr, float v)
	{
		yield return new WaitForEndOfFrame();
		sr.verticalNormalizedPosition = v;
		yield break;
	}

	protected AchivementsInit Achievement;
}
