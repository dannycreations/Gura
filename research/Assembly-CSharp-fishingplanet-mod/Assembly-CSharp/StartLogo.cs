using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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
		this.psMovieRenderer.gameObject.SetActive(false);
		base.GetComponent<AudioSource>().Play();
		RawImage component = base.GetComponent<RawImage>();
		MovieTexture movieTexture = (MovieTexture)component.mainTexture;
		float duration = movieTexture.duration;
		movieTexture.loop = false;
		movieTexture.Play();
		Debug.Log(this.SysInfoToString(SysInfoHelper.GetSysInfo()));
		int @int = CustomPlayerPrefs.GetInt("WrongSysReqShowCount");
		if (((SystemInfo.systemMemorySize != 0 && SystemInfo.systemMemorySize <= 3900) || (SystemInfo.graphicsMemorySize != 0 && SystemInfo.graphicsMemorySize <= 490)) && @int < 3)
		{
			this.WrongSysReqPanel.SetActive(true);
		}
		else
		{
			base.StartCoroutine(StartLogo.LoadNewLevel("StartScene", duration));
		}
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
		RawImage component = base.GetComponent<RawImage>();
		MovieTexture movieTexture = (MovieTexture)component.mainTexture;
		Object.Destroy(component);
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
