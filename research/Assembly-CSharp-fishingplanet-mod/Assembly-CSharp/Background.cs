using System;
using UnityEngine;
using UnityEngine.UI;

public class Background : MonoBehaviour
{
	private void Awake()
	{
		this._canvasGroup = base.GetComponent<CanvasGroup>();
		Object.DontDestroyOnLoad(this);
		this._bg.overrideSprite = this._bgF2P;
	}

	public void SetVisibility(bool flag)
	{
		this._canvasGroup.alpha = (float)((!flag) ? 0 : 1);
		this._canvasGroup.interactable = flag;
		this._canvasGroup.blocksRaycasts = flag;
	}

	[SerializeField]
	private Sprite _bgF2P;

	[SerializeField]
	private Image _bg;

	private CanvasGroup _canvasGroup;
}
