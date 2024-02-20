using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.UI._2D.Helpers
{
	public class ResourcesHelpers
	{
		public static void RequestSpriteForImage(string path, Image image, IItemUpdated item)
		{
			if (image == null)
			{
				MonoBehaviour.print("RequestSpriteForImage psrite for image from cache image null");
				return;
			}
			if (ResourcesHelpers.SpritesCache.ContainsKey(path) && image.overrideSprite != ResourcesHelpers.SpritesCache[path])
			{
				image.overrideSprite = ResourcesHelpers.SpritesCache[path];
			}
			else if (!ResourcesHelpers.SpritesCache.ContainsKey(path))
			{
				ResourcesHelpers.GetInventorySpriteFromCache(path, item);
			}
			ResourcesHelpers.keysUsage.Remove(path);
			ResourcesHelpers.keysUsage.Add(path);
		}

		private static void GetInventorySpriteFromCache(string path, IItemUpdated item)
		{
			if (string.IsNullOrEmpty(path))
			{
				return;
			}
			if (ResourcesHelpers.SpritesCache.ContainsKey(path))
			{
				item.IsItemUpdate = true;
				return;
			}
			if (ResourcesHelpers.keysUsage.Count == ResourcesHelpers.maxSpritesInCache)
			{
				ResourcesHelpers.SpritesCache.Remove(ResourcesHelpers.keysUsage[0]);
				ResourcesHelpers.keysUsage.RemoveAt(0);
			}
			bool flag = ResourcesHelpers.keysUsage.Contains(path);
			if (!flag)
			{
				ResourcesHelpers.keysUsage.Add(path);
			}
			MessageBoxList.Instance.StartCoroutine(ResourcesHelpers.SetSpriteToImageAsync(path, item, flag));
		}

		public static IEnumerator GetInventoryShortSpriteAsync(int? id, int index, Action<Sprite, int> callback)
		{
			if (id != null)
			{
				ResourceRequest request = Resources.LoadAsync<Sprite>(string.Format("Textures/Inventory/{0}", id));
				yield return request;
				if (callback != null)
				{
					callback(request.asset as Sprite, index);
				}
			}
			else if (ResourcesHelpers.transparentSprite == null)
			{
				ResourceRequest request = Resources.LoadAsync<Sprite>("Textures/backgroundTransparent2pixels");
				yield return request;
				ResourcesHelpers.transparentSprite = request.asset as Sprite;
				if (callback != null)
				{
					callback(request.asset as Sprite, index);
				}
			}
			else if (callback != null)
			{
				callback(ResourcesHelpers.transparentSprite, index);
			}
			yield break;
		}

		public static void LoadResource(string path, Action<Object> callback)
		{
			MessageBoxList.Instance.StartCoroutine(ResourcesHelpers.LoadResourceWithCallback(path, callback));
		}

		private static IEnumerator LoadResourceWithCallback(string path, Action<Object> callback)
		{
			ResourceRequest request = Resources.LoadAsync<Object>(path);
			yield return request;
			callback(request.asset);
			yield break;
		}

		public static Sprite GetTransparentSprite()
		{
			if (ResourcesHelpers.transparentSprite == null)
			{
				ResourcesHelpers.transparentSprite = Resources.Load<Sprite>("Textures/backgroundTransparent2pixels");
			}
			return ResourcesHelpers.transparentSprite;
		}

		public static IEnumerator SetPlaceholderHighlightToImageAsync(string name, Image image, IItemUpdated item)
		{
			if (!string.IsNullOrEmpty(name))
			{
				yield return ResourcesHelpers.SetSpriteToImageAsync(string.Format("Textures/Placeholders/{0}", name), image, item);
			}
			else
			{
				yield return ResourcesHelpers.SetSpriteToImageAsync(string.Empty, image, item);
			}
			yield break;
		}

		private static IEnumerator SetSpriteToImageAsync(string path, Image image, IItemUpdated item)
		{
			bool hasImage = false;
			if (!string.IsNullOrEmpty(path))
			{
				ResourceRequest request = Resources.LoadAsync<Sprite>(path);
				hasImage = true;
				yield return request;
				if (image != null && image.gameObject != null)
				{
					if (!image.gameObject.activeSelf)
					{
						image.gameObject.SetActive(true);
					}
					image.overrideSprite = request.asset as Sprite;
					if (item != null)
					{
						item.IsItemUpdate = hasImage;
					}
				}
			}
			else if (ResourcesHelpers.transparentSprite == null)
			{
				ResourceRequest request = Resources.LoadAsync<Sprite>("Textures/backgroundTransparent2pixels");
				yield return request;
				ResourcesHelpers.transparentSprite = request.asset as Sprite;
				if (image != null && image.gameObject != null)
				{
					if (!image.gameObject.activeSelf)
					{
						image.gameObject.SetActive(true);
					}
					image.overrideSprite = ResourcesHelpers.transparentSprite;
					if (item != null)
					{
						item.IsItemUpdate = hasImage;
					}
				}
			}
			else if (image != null && image.gameObject != null)
			{
				if (!image.gameObject.activeSelf)
				{
					image.gameObject.SetActive(true);
				}
				image.overrideSprite = ResourcesHelpers.transparentSprite;
				if (item != null)
				{
					item.IsItemUpdate = hasImage;
				}
			}
			yield break;
		}

		private static IEnumerator SetSpriteToImageAsync(string path, IItemUpdated item, bool isLoadingElsewhere = false)
		{
			if (!isLoadingElsewhere)
			{
				if (!string.IsNullOrEmpty(path))
				{
					ResourceRequest request = Resources.LoadAsync<Sprite>(path);
					yield return request;
					if (!ResourcesHelpers.SpritesCache.ContainsKey(path))
					{
						ResourcesHelpers.SpritesCache.Add(path, request.asset as Sprite);
					}
					item.IsItemUpdate = true;
				}
				else if (ResourcesHelpers.transparentSprite == null)
				{
					ResourceRequest request = Resources.LoadAsync<Sprite>("Textures/backgroundTransparent2pixels");
					yield return request;
					ResourcesHelpers.transparentSprite = request.asset as Sprite;
					ResourcesHelpers.SpritesCache.Add(path, ResourcesHelpers.transparentSprite);
					item.IsItemUpdate = true;
				}
				else
				{
					ResourcesHelpers.SpritesCache.Add(path, ResourcesHelpers.transparentSprite);
					item.IsItemUpdate = true;
				}
			}
			else
			{
				while (!ResourcesHelpers.SpritesCache.ContainsKey(path))
				{
					yield return null;
				}
				item.IsItemUpdate = true;
			}
			yield break;
		}

		public const string InventoryPathString = "Textures/Inventory/{0}";

		public const string TMP_FontsPathString = "TMP_Fonts/{0}";

		public const string PlaceholdersPathString = "Textures/Placeholders/{0}";

		public const string TransparentSpritePath = "Textures/backgroundTransparent2pixels";

		public const string ExpiredChumThumbnailBID = "sour-foodball";

		private static int maxSpritesInCache = 30;

		public static Dictionary<string, Sprite> SpritesCache = new Dictionary<string, Sprite>(ResourcesHelpers.maxSpritesInCache);

		private static List<string> keysUsage = new List<string>(ResourcesHelpers.maxSpritesInCache);

		private static Sprite transparentSprite;

		public class AsyncLoadableData
		{
			public Image Image { get; set; }

			public int? ImagePath { get; set; }

			public string Pattern { get; set; }
		}

		[Serializable]
		public class AsyncLoadableImage : IItemUpdated
		{
			public void Load(List<ResourcesHelpers.AsyncLoadableData> images, string pattern = "Textures/Inventory/{0}")
			{
				if (images.Count == 0)
				{
					return;
				}
				int i = 0;
				this.OnLoaded.AddListener(delegate
				{
					i++;
					if (i < images.Count)
					{
						this.Load(images[i], "Textures/Inventory/{0}");
					}
					else
					{
						this.OnLoaded.RemoveAllListeners();
					}
				});
				this.Load(images[i], "Textures/Inventory/{0}");
			}

			public void Load(ResourcesHelpers.AsyncLoadableData im, string pattern = "Textures/Inventory/{0}")
			{
				this.Load(im.ImagePath, im.Image, string.IsNullOrEmpty(im.Pattern) ? pattern : im.Pattern);
			}

			public void Load(int? imagePath, Image img, string pattern = "Textures/Inventory/{0}")
			{
				this.Load(string.Format(pattern, imagePath), img);
			}

			public void Load(string imagePath, Image img)
			{
				this.Image = img;
				this.Load(imagePath);
			}

			public void Load(string imagePath)
			{
				this.path = imagePath;
				this.Image.overrideSprite = ResourcesHelpers.GetTransparentSprite();
				ResourcesHelpers.GetInventorySpriteFromCache(this.path, this);
			}

			public void Load(int imagePath)
			{
				this.path = string.Format("Textures/Inventory/{0}", imagePath);
				this.Image.overrideSprite = ResourcesHelpers.GetTransparentSprite();
				ResourcesHelpers.GetInventorySpriteFromCache(this.path, this);
			}

			public bool IsItemUpdate
			{
				get
				{
					return this._isItemUpdate;
				}
				set
				{
					if (value && !string.IsNullOrEmpty(this.path))
					{
						ResourcesHelpers.RequestSpriteForImage(this.path, this.Image, this);
						if (this.OnLoaded != null)
						{
							this.OnLoaded.Invoke();
						}
					}
				}
			}

			public Image Image;

			public UnityEvent OnLoaded = new UnityEvent();

			private bool _isItemUpdate;

			private string path;
		}
	}
}
