using System;
using DG.Tweening;
using UnityEngine;

public class Sequences : MonoBehaviour
{
	private void Start()
	{
		Sequence sequence = DOTween.Sequence();
		TweenSettingsExtensions.Append(sequence, ShortcutExtensions.DOMoveY(this.target, 2f, 1f, false));
		TweenSettingsExtensions.Join(sequence, ShortcutExtensions.DORotate(this.target, new Vector3(0f, 135f, 0f), 1f, 0));
		TweenSettingsExtensions.Append(sequence, ShortcutExtensions.DOScaleY(this.target, 0.2f, 1f));
		TweenSettingsExtensions.Insert(sequence, 0f, TweenSettingsExtensions.SetRelative<Tweener>(ShortcutExtensions.DOMoveX(this.target, 4f, TweenExtensions.Duration(sequence, true), false)));
		TweenSettingsExtensions.SetLoops<Sequence>(sequence, 4, 1);
	}

	public Transform target;
}
