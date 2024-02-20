using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class LaunchInit : MonoBehaviour
{
	private void Awake()
	{
		Screen.SetResolution(1000, 700, false);
	}

	private void Start()
	{
		this._path = LaunchInit.ApplicationPath();
		this.ButtonStart.SetActive(false);
		this.ButtonDownload.SetActive(false);
		this.ButtonRestart.SetActive(false);
		this.ProgressBar.SetActive(false);
		this.ConnectingText.SetActive(true);
		this.VersionInfo.text = string.Format("Version: {0}", LaunchInit.CurrentVersion);
	}

	private void GetUpdateUrl()
	{
		GelocationUtil gelocationUtil = new GelocationUtil();
		GeoData myContinent = gelocationUtil.GetMyContinent();
		this._updateURL = LaunchInit.GetUpdateUrl(myContinent);
	}

	private void Update()
	{
		this.UpdaterStateInfo.text = this._progressText;
	}

	private void ShowReleaseNotes()
	{
	}

	private void CheckFileExist()
	{
		string text = this._updateURL + "versions.txt";
		HttpWebResponse httpWebResponse = null;
		HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(text);
		httpWebRequest.Method = "HEAD";
		try
		{
			httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			this._fileExist = LaunchInit.IntedState.Inited;
		}
		catch (WebException ex)
		{
			this._fileExist = LaunchInit.IntedState.Error;
		}
		finally
		{
			if (httpWebResponse != null)
			{
				httpWebResponse.Close();
			}
		}
	}

	private string CheckNewVersion()
	{
		WebResponse webResponse = null;
		try
		{
			WebRequest webRequest = WebRequest.Create(this._updateURL + "versions.txt");
			webRequest.Timeout = 1800000;
			webRequest.UseDefaultCredentials = true;
			webResponse = webRequest.GetResponse();
			using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream()))
			{
				while (!streamReader.EndOfStream)
				{
					string text = streamReader.ReadLine();
					if (text != null && text.Split(new char[] { '_' })[0] == LaunchInit.CurrentVersion)
					{
						return text;
					}
				}
			}
		}
		finally
		{
			if (webResponse != null)
			{
				webResponse.Close();
			}
		}
		return null;
	}

	public void btnRestart_Click()
	{
		try
		{
			FileInfo fileInfo = new FileInfo("Updater/Updater.exe");
			new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = fileInfo.FullName,
					Arguments = "-popupwindow"
				}
			}.Start();
		}
		catch (Exception ex)
		{
			MonoBehaviour.print(ex);
		}
		Application.Quit();
	}

	public void btnClose_Click()
	{
		Application.Quit();
	}

	public void btnStart_Click()
	{
		Application.LoadLevel("StartScene");
	}

	private static string ApplicationPath()
	{
		string text = Application.dataPath;
		if (Application.platform == 1)
		{
			text += "/../../../";
		}
		else if (Application.platform == 2)
		{
			text += "/../";
		}
		return text;
	}

	private static string GetUpdateUrl(GeoData geoData)
	{
		string continent_code = geoData.continent_code;
		if (continent_code != null)
		{
			if (continent_code == "EU" || continent_code == "AS")
			{
				return "http://download.fishingplanet.com/patches/";
			}
		}
		return "http://updates.fishingplanet.com/patches/";
	}

	public static string CurrentVersion = "1.1.0";

	public Text UpdaterStateInfo;

	public GameObject ButtonStart;

	public GameObject ButtonDownload;

	public GameObject ButtonRestart;

	public GameObject ProgressBar;

	public GameObject ConnectingText;

	public Image ProgressImage;

	public Text InfoContent;

	public Scrollbar InfoScrollbar;

	public Text VersionInfo;

	private string _newVersion;

	private int _progressDownloading;

	private int _progressExtracting;

	private string _extractingFile = string.Empty;

	private bool _extractingCompleted;

	private string _progressText = string.Empty;

	private string _updateURL;

	private bool _isInited;

	private LaunchInit.IntedState _fileExist = LaunchInit.IntedState.UnInited;

	private const string TempDirectory = "Temp";

	private string _path;

	private enum IntedState
	{
		Inited,
		UnInited,
		Error
	}
}
