using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ShowcaseGUI : MonoBehaviour
{
	private void Start()
	{
		if (ShowcaseGUI.instance)
		{
			Object.Destroy(base.gameObject);
		}
		ShowcaseGUI.instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
		SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.OnLevelWasLoadedNew);
		this.OnLevelWasLoadedNew(default(Scene), 0);
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= new UnityAction<Scene, LoadSceneMode>(this.OnLevelWasLoadedNew);
	}

	private void OnLevelWasLoadedNew(Scene scene, LoadSceneMode mode)
	{
		GameObject gameObject = GameObject.Find("Floor_Tile");
		if (gameObject)
		{
			IEnumerator enumerator = gameObject.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					transform.gameObject.SetActive(true);
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
		}
	}

	private void OnGUI()
	{
		int width = Screen.width;
		int num = 30;
		int num2 = 40;
		Rect rect;
		rect..ctor((float)(width - num * 2 - 70), 10f, (float)num, (float)num2);
		if (SceneManager.GetActiveScene().buildIndex > 0 && GUI.Button(rect, "<"))
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
		}
		else if (GUI.Button(new Rect(rect), "<"))
		{
			SceneManager.LoadScene(this.levels - 1);
		}
		GUI.Box(new Rect((float)(width - num - 70), 10f, 60f, (float)num2), string.Concat(new object[]
		{
			"Scene:\n",
			SceneManager.GetActiveScene().buildIndex + 1,
			" / ",
			this.levels
		}));
		Rect rect2;
		rect2..ctor((float)(width - num - 10), 10f, (float)num, (float)num2);
		if (SceneManager.GetActiveScene().buildIndex < this.levels - 1 && GUI.Button(new Rect(rect2), ">"))
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
		}
		else if (GUI.Button(new Rect(rect2), ">"))
		{
			SceneManager.LoadScene(0);
		}
		GUI.Box(new Rect((float)(width - 130), 50f, 120f, 55f), "Example scenes\nmust be added\nto Build Settings.");
	}

	private static ShowcaseGUI instance;

	private int levels = 9;
}
