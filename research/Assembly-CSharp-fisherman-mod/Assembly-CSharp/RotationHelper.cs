using System;
using DG.Tweening;
using UnityEngine;

public class RotationHelper : MonoBehaviour
{
	private void Start()
	{
		TweenSettingsExtensions.SetLoops<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DORotate(base.transform, new Vector3((float)this.rotation, 0f, 0f), this.duration, 3), 1), -1);
	}

	public float duration;

	public int rotation;
}
