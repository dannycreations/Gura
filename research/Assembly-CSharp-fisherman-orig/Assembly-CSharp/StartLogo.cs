using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class StartLogo : MonoBehaviour
{
	private void Start()
	{
		GameState.State = GameStates.LogoState;
		CustomPlayerPrefs.Init(this);
		this._canStart = true;
		base.StartCoroutine(this.Waiting());
	}

	private IEnumerator Waiting()
	{
		yield return new WaitUntil(() => this._canStart);
		this.RunSplashScreen();
		yield break;
	}

	private void RunSplashScreen()
	{
		this._audioSource = base.GetComponent<AudioSource>();
		VideoPlayer component = base.GetComponent<VideoPlayer>();
		component.clip = this.RetailSteamVideoClip;
		component.Play();
		base.StartCoroutine(StartLogo.LoadNewLevel("StartScene", 8f));
	}

	public void CloseMessage()
	{
		if (CustomPlayerPrefs.HasKey("WrongSysReqShowCount"))
		{
			CustomPlayerPrefs.SetInt("WrongSysReqShowCount", CustomPlayerPrefs.GetInt("WrongSysReqShowCount") + 1);
		}
		else
		{
			CustomPlayerPrefs.SetInt("WrongSysReqShowCount", 1);
		}
		this.WrongSysReqPanel.SetActive(true);
		base.StartCoroutine(StartLogo.LoadNewLevel("StartScene", 0.25f));
	}

	private static IEnumerator LoadNewLevel(string sceneName, float waiting)
	{
		yield return new WaitForSeconds(waiting);
		AsyncOperation async = Application.LoadLevelAsync(sceneName);
		while (!async.isDone)
		{
			yield return async;
		}
		yield break;
	}

	private void OnDestroy()
	{
	}

	private string SysInfoToString(SysInfo sysInfo)
	{
		return string.Format("OS: {0} \n Cpu: {1} \n Ram: {2} \n DirectX: {3} \n VideoAdapter: {4}", new object[] { sysInfo.Os, sysInfo.Cpu, sysInfo.Ram, sysInfo.DirectX, sysInfo.VideoAdapter });
	}

	public GameObject WrongSysReqPanel;

	private AudioSource _audioSource;

	private bool _canStart;

	public string MoviePath;

	public Renderer psMovieRenderer;

	public Renderer xbMovieRenderer;

	public CanvasAlphaTween MadeWithUnityScreen;

	public VideoClip RetailSteamVideoClip;
}
