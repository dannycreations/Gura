using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.Util
{
	public class LoadSceneOnClick : MonoBehaviour
	{
		private void Start()
		{
			base.GetComponent<Button>().onClick.AddListener(new UnityAction(this.LoadScene));
		}

		private void LoadScene()
		{
			SceneManager.LoadScene(this.sceneName);
		}

		private int GetIdxToLoad(int curIdx, int numScenes, int incr)
		{
			return (curIdx + numScenes + incr) % numScenes;
		}

		public string sceneName;
	}
}
