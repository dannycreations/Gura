using System;
using Assets.Scripts.UI._2D.Helpers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PremiumRetailItemFullInfo : MonoBehaviour
{
	public bool IsActive
	{
		get
		{
			return base.gameObject.activeSelf;
		}
	}

	public void Init(int? imageBid, string currency, string price)
	{
		this._image.overrideSprite = null;
		this._imageLdbl.Load(imageBid, this._image, "Textures/Inventory/{0}");
		this._currency.text = currency;
		this._sum.text = price;
	}

	public void Select(bool flag)
	{
		this._buyInfo.SetActive(flag);
	}

	public void MoveX(float x, float animTime)
	{
		TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOAnchorPos(this._rt, new Vector2(x, this._rt.anchoredPosition.y), animTime, false), 6);
	}

	public void DoKill(bool complete = false)
	{
		ShortcutExtensions.DOKill(this._rt, complete);
	}

	public void SetActive(bool flag)
	{
		base.gameObject.SetActive(flag);
	}

	[SerializeField]
	private Text _currency;

	[SerializeField]
	private Text _sum;

	[SerializeField]
	private GameObject _buyInfo;

	[SerializeField]
	private Image _image;

	[SerializeField]
	private RectTransform _rt;

	private readonly ResourcesHelpers.AsyncLoadableImage _imageLdbl = new ResourcesHelpers.AsyncLoadableImage();
}
