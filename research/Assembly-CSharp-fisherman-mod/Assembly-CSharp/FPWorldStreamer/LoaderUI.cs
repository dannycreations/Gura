using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FPWorldStreamer
{
	public class LoaderUI : MonoBehaviour
	{
		private void Awake()
		{
			this._streamer.EStartLoading += this.StreamerOnStartLoading;
		}

		private void Start()
		{
			base.gameObject.SetActive(false);
		}

		private void StreamerOnStartLoading(int scenesToLoad)
		{
			if (base.gameObject.activeInHierarchy)
			{
				return;
			}
			base.gameObject.SetActive(true);
			this._streamer.ESceneLoaded += this.StreamerOnSceneLoaded;
			this._allScenes = scenesToLoad;
			this._loadedScenes = 0;
			this.UpdateProgress();
		}

		private void StreamerOnSceneLoaded()
		{
			this._loadedScenes++;
			this.UpdateProgress();
			if (this._loadedScenes == this._allScenes)
			{
				this._streamer.ESceneLoaded -= this.StreamerOnSceneLoaded;
				base.StartCoroutine(this.Hide());
			}
		}

		private void UpdateProgress()
		{
			this._progressBar.fillAmount = (float)this._loadedScenes / (float)this._allScenes;
		}

		private IEnumerator Hide()
		{
			yield return new WaitForSeconds(this._hideDelay);
			base.gameObject.SetActive(false);
			yield break;
		}

		private void OnDestroyed()
		{
			this._streamer.EStartLoading -= this.StreamerOnStartLoading;
			this._streamer.ESceneLoaded -= this.StreamerOnSceneLoaded;
		}

		[SerializeField]
		private Streamer _streamer;

		[SerializeField]
		private Image _progressBar;

		[SerializeField]
		private float _hideDelay = 0.5f;

		private int _loadedScenes;

		private int _allScenes;
	}
}
