using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.Util
{
	public class SimpleImageDownloader : MonoBehaviour
	{
		public static SimpleImageDownloader Instance
		{
			get
			{
				if (SimpleImageDownloader._Instance == null)
				{
					SimpleImageDownloader._Instance = new GameObject(typeof(SimpleImageDownloader).Name).AddComponent<SimpleImageDownloader>();
				}
				return SimpleImageDownloader._Instance;
			}
		}

		public int MaxConcurrentRequests { get; set; }

		private IEnumerator Start()
		{
			if (this.MaxConcurrentRequests == 0)
			{
				this.MaxConcurrentRequests = 20;
			}
			for (;;)
			{
				while (this._ExecutingRequests.Count >= this.MaxConcurrentRequests)
				{
					yield return this._Wait1Sec;
				}
				int lastIndex = this._QueuedRequests.Count - 1;
				if (lastIndex >= 0)
				{
					SimpleImageDownloader.Request request = this._QueuedRequests[lastIndex];
					this._QueuedRequests.RemoveAt(lastIndex);
					base.StartCoroutine(this.DownloadCoroutine(request));
				}
				yield return null;
			}
			yield break;
		}

		private void OnDestroy()
		{
			SimpleImageDownloader._Instance = null;
		}

		public void Enqueue(SimpleImageDownloader.Request request)
		{
			this._QueuedRequests.Add(request);
		}

		private IEnumerator DownloadCoroutine(SimpleImageDownloader.Request request)
		{
			this._ExecutingRequests.Add(request);
			WWW www = new WWW(request.url);
			yield return www;
			if (string.IsNullOrEmpty(www.error))
			{
				if (request.onDone != null)
				{
					SimpleImageDownloader.Result result = new SimpleImageDownloader.Result(www);
					request.onDone(result);
				}
			}
			else if (request.onError != null)
			{
				request.onError();
			}
			www.Dispose();
			this._ExecutingRequests.Remove(request);
			yield break;
		}

		private static SimpleImageDownloader _Instance;

		private const int DEFAULT_MAX_CONCURRENT_REQUESTS = 20;

		private List<SimpleImageDownloader.Request> _QueuedRequests = new List<SimpleImageDownloader.Request>();

		private List<SimpleImageDownloader.Request> _ExecutingRequests = new List<SimpleImageDownloader.Request>();

		private WaitForSeconds _Wait1Sec = new WaitForSeconds(1f);

		public class Request
		{
			public string url;

			public Action<SimpleImageDownloader.Result> onDone;

			public Action onError;
		}

		public class Result
		{
			public Result(WWW www)
			{
				this._UsedWWW = www;
			}

			public Texture2D CreateTextureFromReceivedData()
			{
				return this._UsedWWW.texture;
			}

			public void LoadTextureInto(Texture2D existingTexture)
			{
				this._UsedWWW.LoadImageIntoTexture(existingTexture);
			}

			private WWW _UsedWWW;
		}
	}
}
