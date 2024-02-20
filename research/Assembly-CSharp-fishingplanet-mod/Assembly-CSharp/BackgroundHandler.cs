using System;
using Assets.Scripts.Common.Managers.Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class BackgroundHandler : MonoBehaviour
{
	private void OnEnable()
	{
		if (SceneManager.GetActiveScene().name != "StartScene" || this.videoPlayer == null || this.videoPlayer.clip == null)
		{
			this.BackgroundImagePanel.SetActive(true);
			return;
		}
		base.Invoke("DisableImage", 1f);
		this.videoPlayer.gameObject.SetActive(true);
		if (this.videoPlayer.targetCamera == null)
		{
			Camera guicamera = MenuHelpers.Instance.GUICamera;
			if (guicamera != null)
			{
				this.videoPlayer.targetCamera = guicamera;
			}
		}
		this.videoPlayer.Play();
	}

	private void DisableImage()
	{
		this.BackgroundImagePanel.SetActive(false);
	}

	private void OnDisable()
	{
		if (SceneManager.GetActiveScene().name != "StartScene")
		{
			return;
		}
		this.videoPlayer.Stop();
		this.BackgroundImagePanel.SetActive(true);
	}

	public VideoPlayer videoPlayer;

	public GameObject BackgroundImagePanel;
}
