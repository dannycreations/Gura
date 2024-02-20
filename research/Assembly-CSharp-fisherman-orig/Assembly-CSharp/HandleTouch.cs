using System;
using TouchScript.Gestures;
using UnityEngine;

public class HandleTouch : MonoBehaviour
{
	private void OnEnable()
	{
		base.GetComponent<TapGesture>().Tapped += this.tappedHandler;
	}

	private void OnDisable()
	{
		base.GetComponent<TapGesture>().Tapped -= this.tappedHandler;
	}

	private void tappedHandler(object sender, EventArgs e)
	{
		TutorialManager.touched = true;
	}
}
