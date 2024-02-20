using System;
using UnityEngine;
using UnityEngine.UI;

public class LoadingActionView : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		if (!LoadingActionView._isAutenticated && PhotonConnectionFactory.Instance.IsAuthenticated)
		{
			ManagerScenes.ProgressOfLoad += 0.016f;
			LoadingActionView._isAutenticated = true;
		}
		this.ProgressImage.fillAmount = ManagerScenes.ProgressOfLoad;
	}

	public Image ProgressImage;

	private static bool _isAutenticated;
}
