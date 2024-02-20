using System;
using DG.Tweening;
using UnityEngine;

public class ArrowJamping : MonoBehaviour
{
	private void OnEnable()
	{
		RectTransform component = base.GetComponent<RectTransform>();
		if (!this._isInited)
		{
			this._isInited = true;
			this._startScale = component.localScale;
			this._startPosition = component.anchoredPosition;
			this._startJumpPosition = new Vector2(component.anchoredPosition.x + this._offsetPos.x, component.anchoredPosition.y + this._offsetPos.y);
			DOTween.Init(new bool?(false), new bool?(true), new LogBehaviour?(2));
		}
		ShortcutExtensions.DOKill(component, false);
		switch (this.Direction)
		{
		case DirectionJumping.LeftToRight:
			TweenSettingsExtensions.SetLoops<Tweener>(ShortcutExtensions.DOAnchorPos(component, new Vector3(this._startJumpPosition.x - this.JumpDistance, this._startJumpPosition.y, this._startJumpPosition.z), this._speed, false), -1, 1);
			break;
		case DirectionJumping.RightToLeft:
			TweenSettingsExtensions.SetLoops<Tweener>(ShortcutExtensions.DOAnchorPos(component, new Vector3(this._startJumpPosition.x + this.JumpDistance, this._startJumpPosition.y, this._startJumpPosition.z), this._speed, false), -1, 1);
			break;
		case DirectionJumping.TopToBottom:
			TweenSettingsExtensions.SetLoops<Tweener>(ShortcutExtensions.DOAnchorPos(component, new Vector3(this._startJumpPosition.x, this._startJumpPosition.y - this.JumpDistance, this._startJumpPosition.z), this._speed, false), -1, 1);
			break;
		case DirectionJumping.BottomToTop:
			TweenSettingsExtensions.SetLoops<Tweener>(ShortcutExtensions.DOAnchorPos(component, new Vector3(this._startJumpPosition.x, this._startJumpPosition.y + this.JumpDistance, this._startJumpPosition.z), this._speed, false), -1, 1);
			break;
		}
		if (this._scale > 0f)
		{
			TweenSettingsExtensions.SetLoops<Tweener>(ShortcutExtensions.DOScale(component, this._startScale * this._scale, this._speed), -1, 1);
		}
	}

	private void OnDisable()
	{
		RectTransform component = base.GetComponent<RectTransform>();
		ShortcutExtensions.DOKill(component, false);
		Sequence sequence = DOTween.Sequence();
		TweenSettingsExtensions.Join(sequence, ShortcutExtensions.DOScale(component, this._startScale, this._speedBack));
		TweenSettingsExtensions.Join(sequence, ShortcutExtensions.DOAnchorPos(component, this._startPosition, this._speedBack, false));
	}

	[SerializeField]
	private Vector2 _offsetPos = Vector2.zero;

	[SerializeField]
	private float _speed = 0.5f;

	[SerializeField]
	private float _speedBack = 0.25f;

	[SerializeField]
	private float _scale;

	private Vector3 _startPosition;

	public float JumpDistance = 5f;

	public DirectionJumping Direction = DirectionJumping.BottomToTop;

	private Vector3 _startJumpPosition;

	private Vector3 _startScale;

	private bool _isInited;
}
