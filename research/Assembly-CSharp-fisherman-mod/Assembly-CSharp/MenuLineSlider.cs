using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MenuLineSlider : MonoBehaviour
{
	protected void Awake()
	{
		if (this.IsInited)
		{
			return;
		}
		for (int i = 0; i < this.Container.Length; i++)
		{
			MenuLineSlider.MenuLineSliderData item = this.Container[i];
			item.Tgl.onValueChanged.AddListener(delegate(bool isOn)
			{
				if (this.gameObject.activeInHierarchy)
				{
					this.SelectItem(item);
				}
			});
		}
		this.IsInited = true;
	}

	protected void OnDestroy()
	{
		base.StopAllCoroutines();
	}

	protected void OnEnable()
	{
		this.Awake();
		for (int i = 0; i < this.Container.Length; i++)
		{
			MenuLineSlider.MenuLineSliderData menuLineSliderData = this.Container[i];
			if (menuLineSliderData.Tgl.isOn)
			{
				this.SelectItem(menuLineSliderData);
				break;
			}
		}
	}

	protected IEnumerator ImSelectedPlayAnim(MenuLineSlider.MenuLineSliderData o)
	{
		yield return new WaitForEndOfFrame();
		ShortcutExtensions.DOKill(this.ImSelected, true);
		ShortcutExtensions.DOAnchorPos(this.ImSelected, new Vector2(o.Rt.anchoredPosition.x + o.MwS.PreferredWidth / 2f, this.ImSelected.anchoredPosition.y), this.AnimTime, false);
		ShortcutExtensions.DOSizeDelta(this.ImSelected, new Vector2(o.MwS.PreferredWidth, this.ImSelected.rect.height), this.AnimTime, false);
		yield break;
	}

	protected void SelectItem(MenuLineSlider.MenuLineSliderData o)
	{
		base.StopAllCoroutines();
		base.StartCoroutine(this.ImSelectedPlayAnim(o));
	}

	[SerializeField]
	protected MenuLineSlider.MenuLineSliderData[] Container;

	[SerializeField]
	protected RectTransform ImSelected;

	[SerializeField]
	protected float AnimTime = 0.4f;

	protected bool IsInited;

	[Serializable]
	public class MenuLineSliderData
	{
		[SerializeField]
		public RectTransform Rt;

		[SerializeField]
		public Toggle Tgl;

		[SerializeField]
		public MenuWidthSet MwS;
	}
}
