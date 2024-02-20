using System;
using DG.Tweening;
using ObjectModel;
using TMPro;
using UnityEngine;

public class ManagedHintTextUI : ManagedHintObject
{
	public override void SetObserver(ManagedHint observer, int id)
	{
		this.observer = observer;
		this.inhintId = id;
		if (observer.Message.ScenePosition != null)
		{
			this._posOrigin = new Vector3(observer.Message.ScenePosition.X, observer.Message.ScenePosition.Y, observer.Message.ScenePosition.Z);
			HintArrowType3D hintArrowType3D = observer.Message.ArrowType3D;
			if (hintArrowType3D == HintArrowType3D.Undefined)
			{
				hintArrowType3D = HintArrowType3D.Fish;
				RaycastHit raycastHit;
				if (Physics.Raycast(this._posOrigin, Vector3.down, ref raycastHit, 50f, GlobalConsts.GroundObstacleMask) && raycastHit.point.y > 0f)
				{
					hintArrowType3D = HintArrowType3D.Pointer;
				}
			}
			this._aj.enabled = hintArrowType3D == HintArrowType3D.Fish || hintArrowType3D == HintArrowType3D.Swim;
			this._icoText.text = ((!HintSettings.IcoDictionary.ContainsKey(hintArrowType3D)) ? string.Empty : HintSettings.IcoDictionary[hintArrowType3D]);
			if (this._aj.enabled)
			{
				this._icoRectText.FastShowPanel();
			}
		}
		else
		{
			this._icoText.text = string.Empty;
		}
	}

	protected override void Update()
	{
		Camera main = Camera.main;
		bool flag = base.Displayed && !string.IsNullOrEmpty(this._icoText.text) && main != null;
		if (flag)
		{
			Vector3 vector;
			vector..ctor(this._posOrigin.x, this._posOrigin.y + this._offsetY, this._posOrigin.z);
			Vector3 vector2 = main.WorldToScreenPoint(vector);
			flag = vector2.z > 0f && vector2.x >= 0f && vector2.x <= (float)Screen.width && vector2.y >= 0f && vector2.y <= (float)Screen.height;
			if (flag)
			{
				this._icoRT.anchoredPosition = new Vector2(vector2.x - (float)Screen.width / 2f, vector2.y - (float)Screen.height / 2f);
				float num = Vector3.Distance(main.transform.position, this._posOrigin);
				float num2 = this._minSize;
				float num3 = this._maxSize - this._minSize;
				if (num <= this._dist)
				{
					num2 += num3 - num3 * num / this._dist;
				}
				this._icoRT.localScale = new Vector3(num2, num2, num2);
			}
		}
		if (this._isVisible != flag)
		{
			this._isVisible = flag;
			ShortcutExtensions.DOFade(this.group, (!this._isVisible) ? 0f : 1f, 0.3f);
		}
	}

	protected override void Show()
	{
	}

	protected override void Hide()
	{
	}

	[SerializeField]
	protected TextMeshProUGUI _icoText;

	[SerializeField]
	protected AlphaFade _icoRectText;

	[SerializeField]
	protected RectTransform _icoRT;

	[SerializeField]
	protected float _offsetY = 1.8f;

	[SerializeField]
	protected float _minSize = 1f;

	[SerializeField]
	protected float _maxSize = 1.4f;

	[SerializeField]
	protected float _dist = 10f;

	[SerializeField]
	protected ArrowJamping _aj;

	protected const float MaxCastDist = 50f;

	protected Vector3 _posOrigin;

	protected bool _isVisible;
}
