using System;
using System.Collections;
using System.Threading;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class LogoUpload : MonoBehaviour
{
	private IEnumerator GrabCoroutine()
	{
		yield return new WaitForEndOfFrame();
		Texture2D texture = new Texture2D(Screen.width, Screen.height, 3, false);
		texture.ReadPixels(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), 0, 0);
		yield return 0;
		float aspectRation = (float)Screen.width / (float)Screen.height;
		LogoUpload.ThreadedScale(texture, 800, (int)(800f / aspectRation), true);
		texture.Apply();
		byte[] bytes = ImageConversion.EncodeToJPG(texture, 30);
		PhotonConnectionFactory.Instance.SaveBinaryData(bytes);
		Object.DestroyObject(texture);
		yield break;
	}

	private void Update()
	{
		if (PhotonConnectionFactory.Instance.Profile != null && PhotonConnectionFactory.Instance.Profile.Tournament != null && PhotonConnectionFactory.Instance.Profile.Tournament.KindId == 1 && GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.Fish != null && GameFactory.Player.Tackle.Fish.State == typeof(FishHooked) && this._lastFishInstanceId != GameFactory.Player.Tackle.Fish.InstanceGuid)
		{
			this._lastFishInstanceId = GameFactory.Player.Tackle.Fish.InstanceGuid;
			LogoUpload._grabFlag = true;
			base.Invoke("Send", (float)this.t);
		}
		else if (PhotonConnectionFactory.Instance.Profile != null && PhotonConnectionFactory.Instance.Profile.Tournament != null && PhotonConnectionFactory.Instance.Profile.Tournament.KindId == 3 && GameFactory.Player != null && GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.Fish != null && GameFactory.Player.Tackle.Fish.State == typeof(FishHooked) && this._lastFishInstanceId != GameFactory.Player.Tackle.Fish.InstanceGuid)
		{
			if (this.cid != PhotonConnectionFactory.Instance.Profile.Tournament.TournamentId)
			{
				this.cid = PhotonConnectionFactory.Instance.Profile.Tournament.TournamentId;
				this.cfc = 0;
			}
			this._lastFishInstanceId = GameFactory.Player.Tackle.Fish.InstanceGuid;
			this.cfc = ObscuredInt.op_Increment(this.cfc);
			if (this.cfc == 2 || this.cfc == 6)
			{
				LogoUpload._grabFlag = true;
				base.Invoke("Send", (float)this.t);
			}
		}
	}

	private void Start()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		PhotonConnectionFactory.Instance.OnTournamentLogoRequested += this.Instance_OnTournamentLogoRequested;
	}

	private void Instance_OnTournamentLogoRequested()
	{
		LogoUpload._grabFlag = true;
	}

	private void OnPostRender()
	{
		if (LogoUpload._grabFlag)
		{
			LogoUpload._grabFlag = false;
			base.StartCoroutine(this.GrabCoroutine());
		}
	}

	private void Send()
	{
		LogoUpload._grabFlag = true;
	}

	public static void SendStatic()
	{
		LogoUpload._grabFlag = true;
	}

	private static void ThreadedScale(Texture2D tex, int newWidth, int newHeight, bool useBilinear)
	{
		LogoUpload.texColors = tex.GetPixels();
		LogoUpload.newColors = new Color[newWidth * newHeight];
		if (useBilinear)
		{
			LogoUpload.ratioX = 1f / ((float)newWidth / (float)(tex.width - 1));
			LogoUpload.ratioY = 1f / ((float)newHeight / (float)(tex.height - 1));
		}
		else
		{
			LogoUpload.ratioX = (float)tex.width / (float)newWidth;
			LogoUpload.ratioY = (float)tex.height / (float)newHeight;
		}
		LogoUpload.w = tex.width;
		LogoUpload.w2 = newWidth;
		int num = Mathf.Min(SystemInfo.processorCount, newHeight);
		int num2 = newHeight / num;
		LogoUpload.finishCount = 0;
		if (LogoUpload.mutex == null)
		{
			LogoUpload.mutex = new Mutex(false);
		}
		if (num > 1)
		{
			int i;
			LogoUpload.ThreadData threadData;
			for (i = 0; i < num - 1; i++)
			{
				threadData = new LogoUpload.ThreadData(num2 * i, num2 * (i + 1));
				ParameterizedThreadStart parameterizedThreadStart = ((!useBilinear) ? new ParameterizedThreadStart(LogoUpload.PointScale) : new ParameterizedThreadStart(LogoUpload.BilinearScale));
				Thread thread = new Thread(parameterizedThreadStart);
				thread.Start(threadData);
			}
			threadData = new LogoUpload.ThreadData(num2 * i, newHeight);
			if (useBilinear)
			{
				LogoUpload.BilinearScale(threadData);
			}
			else
			{
				LogoUpload.PointScale(threadData);
			}
			while (LogoUpload.finishCount < num)
			{
				Thread.Sleep(1);
			}
		}
		else
		{
			LogoUpload.ThreadData threadData2 = new LogoUpload.ThreadData(0, newHeight);
			if (useBilinear)
			{
				LogoUpload.BilinearScale(threadData2);
			}
			else
			{
				LogoUpload.PointScale(threadData2);
			}
		}
		tex.Resize(newWidth, newHeight);
		tex.SetPixels(LogoUpload.newColors);
		tex.Apply();
	}

	public static void BilinearScale(object obj)
	{
		LogoUpload.ThreadData threadData = (LogoUpload.ThreadData)obj;
		for (int i = threadData.start; i < threadData.end; i++)
		{
			int num = (int)Mathf.Floor((float)i * LogoUpload.ratioY);
			int num2 = num * LogoUpload.w;
			int num3 = (num + 1) * LogoUpload.w;
			int num4 = i * LogoUpload.w2;
			for (int j = 0; j < LogoUpload.w2; j++)
			{
				int num5 = (int)Mathf.Floor((float)j * LogoUpload.ratioX);
				float num6 = (float)j * LogoUpload.ratioX - (float)num5;
				LogoUpload.newColors[num4 + j] = LogoUpload.ColorLerpUnclamped(LogoUpload.ColorLerpUnclamped(LogoUpload.texColors[num2 + num5], LogoUpload.texColors[num2 + num5 + 1], num6), LogoUpload.ColorLerpUnclamped(LogoUpload.texColors[num3 + num5], LogoUpload.texColors[num3 + num5 + 1], num6), (float)i * LogoUpload.ratioY - (float)num);
			}
		}
		LogoUpload.mutex.WaitOne();
		LogoUpload.finishCount++;
		LogoUpload.mutex.ReleaseMutex();
	}

	public static void PointScale(object obj)
	{
		LogoUpload.ThreadData threadData = (LogoUpload.ThreadData)obj;
		for (int i = threadData.start; i < threadData.end; i++)
		{
			int num = (int)(LogoUpload.ratioY * (float)i) * LogoUpload.w;
			int num2 = i * LogoUpload.w2;
			for (int j = 0; j < LogoUpload.w2; j++)
			{
				LogoUpload.newColors[num2 + j] = LogoUpload.texColors[(int)((float)num + LogoUpload.ratioX * (float)j)];
			}
		}
		LogoUpload.mutex.WaitOne();
		LogoUpload.finishCount++;
		LogoUpload.mutex.ReleaseMutex();
	}

	private static Color ColorLerpUnclamped(Color c1, Color c2, float value)
	{
		return new Color(c1.r + (c2.r - c1.r) * value, c1.g + (c2.g - c1.g) * value, c1.b + (c2.b - c1.b) * value, c1.a + (c2.a - c1.a) * value);
	}

	public int frameRate = 60;

	public float duration = 10f;

	private static ObscuredBool _grabFlag = false;

	private Guid _lastFishInstanceId;

	private static Color[] texColors;

	private static Color[] newColors;

	private static int w;

	private static float ratioX;

	private static float ratioY;

	private static int w2;

	private static int finishCount;

	private static Mutex mutex;

	private ObscuredInt t = 5;

	private ObscuredInt cfc = 0;

	private ObscuredInt cid = 0;

	public class ThreadData
	{
		public ThreadData(int s, int e)
		{
			this.start = s;
			this.end = e;
		}

		public int start;

		public int end;
	}
}
