using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class Basics : MonoBehaviour
{
	private void Start()
	{
		DOTween.Init(new bool?(false), new bool?(true), new LogBehaviour?(2));
		TweenSettingsExtensions.SetLoops<Tweener>(TweenSettingsExtensions.SetRelative<Tweener>(ShortcutExtensions.DOMove(this.cubeA, new Vector3(-2f, 2f, 0f), 1f, false)), -1, 1);
		TweenSettingsExtensions.SetLoops<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetRelative<TweenerCore<Vector3, Vector3, VectorOptions>>(DOTween.To(() => this.cubeB.position, delegate(Vector3 x)
		{
			this.cubeB.position = x;
		}, new Vector3(-2f, 2f, 0f), 1f)), -1, 1);
	}

	public Transform cubeA;

	public Transform cubeB;
}
