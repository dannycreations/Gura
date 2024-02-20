using System;
using frame8.ScrollRectItemsAdapter.Util;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.MultiplePrefabsExample
{
	[RequireComponent(typeof(RawImage))]
	public class RemoteImageBehaviour : MonoBehaviour
	{
		private void Awake()
		{
			if (!this._RawImage)
			{
				this._RawImage = base.GetComponent<RawImage>();
			}
		}

		public void Load(string imageURL, bool loadCachedIfAvailable = true, Action<bool, bool> onCompleted = null, Action onCanceled = null)
		{
			if (loadCachedIfAvailable && this._CurrentRequestedURL == imageURL && this._RecycledTexture)
			{
				if (this._RecycledTexture != this._RawImage.texture)
				{
					this._RawImage.texture = this._RecycledTexture;
				}
				if (onCompleted != null)
				{
					onCompleted(true, true);
				}
				return;
			}
			if (this._RawImage.texture)
			{
				this._RecycledTexture = this._RawImage.texture as Texture2D;
				if (this._RecycledTexture == this._LoadingTexture || this._RecycledTexture == this._ErrorTexture)
				{
					this._RecycledTexture = null;
				}
			}
			else
			{
				this._RecycledTexture = null;
			}
			this._CurrentRequestedURL = imageURL;
			this._RawImage.texture = this._LoadingTexture;
			SimpleImageDownloader.Request request = new SimpleImageDownloader.Request
			{
				url = imageURL,
				onDone = delegate(SimpleImageDownloader.Result result)
				{
					if (!this._DestroyPending && imageURL == this._CurrentRequestedURL)
					{
						Texture2D texture2D;
						if (this._RecycledTexture)
						{
							result.LoadTextureInto(this._RecycledTexture);
							texture2D = this._RecycledTexture;
						}
						else
						{
							texture2D = result.CreateTextureFromReceivedData();
						}
						this._RawImage.texture = texture2D;
						if (onCompleted != null)
						{
							onCompleted(false, true);
						}
					}
					else if (onCanceled != null)
					{
						onCanceled();
					}
				},
				onError = delegate
				{
					if (!this._DestroyPending && imageURL == this._CurrentRequestedURL)
					{
						this._RawImage.texture = this._ErrorTexture;
						if (onCompleted != null)
						{
							onCompleted(false, false);
						}
					}
					else if (onCanceled != null)
					{
						onCanceled();
					}
				}
			};
			SimpleImageDownloader.Instance.Enqueue(request);
		}

		private void OnDestroy()
		{
			this._DestroyPending = true;
		}

		[Tooltip("If not assigned, will try to find it in this game object")]
		[SerializeField]
		private RawImage _RawImage;

		[SerializeField]
		private Texture2D _LoadingTexture;

		[SerializeField]
		private Texture2D _ErrorTexture;

		private string _CurrentRequestedURL;

		private bool _DestroyPending;

		private Texture2D _RecycledTexture;
	}
}
