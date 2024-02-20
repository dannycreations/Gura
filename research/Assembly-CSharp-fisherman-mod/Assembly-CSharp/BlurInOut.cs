using System;
using UnityEngine;
using UnityEngine.UI;

public class BlurInOut : MonoBehaviour
{
	private void Awake()
	{
		this._canvasGroup = base.GetComponent<CanvasGroup>();
		this._cachedMaterial = this._blureBehind.material;
	}

	private void Update()
	{
		if (this._alpha != this._canvasGroup.alpha)
		{
			this._alpha = this._canvasGroup.alpha;
			if (this._alpha >= 0.8f)
			{
				this._blureBehind.material = this._cachedMaterial;
			}
			else
			{
				this._blureBehind.material = null;
			}
		}
	}

	private float _alpha;

	private CanvasGroup _canvasGroup;

	[SerializeField]
	private Graphic _blureBehind;

	private Material _cachedMaterial;
}
