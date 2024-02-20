using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace I2.Loc
{
	[AddComponentMenu("I2/Localization/Localize")]
	public class Localize : MonoBehaviour
	{
		public string Term
		{
			get
			{
				return this.mTerm;
			}
			set
			{
				this.SetTerm(value, null);
			}
		}

		public string SecondaryTerm
		{
			get
			{
				return this.mTermSecondary;
			}
			set
			{
				this.SetTerm(null, value);
			}
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action EventFindTarget;

		private void Awake()
		{
			this.RegisterTargets();
			if (this.HasTargetCache())
			{
				this.EventFindTarget();
			}
			if (this.LocalizeOnAwake)
			{
				this.OnLocalize(false);
			}
		}

		private void RegisterTargets()
		{
			if (this.EventFindTarget != null)
			{
				return;
			}
			Localize.RegisterEvents_NGUI();
			Localize.RegisterEvents_DFGUI();
			this.RegisterEvents_UGUI();
			Localize.RegisterEvents_2DToolKit();
			Localize.RegisterEvents_TextMeshPro();
			this.RegisterEvents_UnityStandard();
			Localize.RegisterEvents_SVG();
		}

		private void OnEnable()
		{
			this.OnLocalize(false);
		}

		public void OnLocalize(bool Force = false)
		{
			if (!Force && (!base.enabled || base.gameObject == null || !base.gameObject.activeInHierarchy))
			{
				return;
			}
			if (string.IsNullOrEmpty(LocalizationManager.CurrentLanguage))
			{
				return;
			}
			if (!this.AlwaysForceLocalize && !Force && !this.LocalizeCallBack.HasCallback() && this.LastLocalizedLanguage == LocalizationManager.CurrentLanguage)
			{
				return;
			}
			this.LastLocalizedLanguage = LocalizationManager.CurrentLanguage;
			if (!this.HasTargetCache())
			{
				this.FindTarget();
			}
			if (!this.HasTargetCache())
			{
				return;
			}
			if (string.IsNullOrEmpty(this.FinalTerm) || string.IsNullOrEmpty(this.FinalSecondaryTerm))
			{
				this.GetFinalTerms(out this.FinalTerm, out this.FinalSecondaryTerm);
			}
			if (string.IsNullOrEmpty(this.FinalTerm) && string.IsNullOrEmpty(this.FinalSecondaryTerm))
			{
				return;
			}
			Localize.CallBackTerm = this.FinalTerm;
			Localize.CallBackSecondaryTerm = this.FinalSecondaryTerm;
			Localize.MainTranslation = LocalizationManager.GetTermTranslation(this.FinalTerm, false);
			Localize.SecondaryTranslation = LocalizationManager.GetTermTranslation(this.FinalSecondaryTerm, false);
			if (string.IsNullOrEmpty(Localize.MainTranslation) && string.IsNullOrEmpty(Localize.SecondaryTranslation))
			{
				return;
			}
			Localize.CurrentLocalizeComponent = this;
			if (Application.isPlaying)
			{
				this.LocalizeCallBack.Execute(this);
				LocalizationManager.ApplyLocalizationParams(ref Localize.MainTranslation, this);
			}
			if (LocalizationManager.IsRight2Left && !this.IgnoreRTL)
			{
				if (this.AllowMainTermToBeRTL && !string.IsNullOrEmpty(Localize.MainTranslation))
				{
					Localize.MainTranslation = LocalizationManager.ApplyRTLfix(Localize.MainTranslation, this.MaxCharactersInRTL);
				}
				if (this.AllowSecondTermToBeRTL && !string.IsNullOrEmpty(Localize.SecondaryTranslation))
				{
					Localize.SecondaryTranslation = LocalizationManager.ApplyRTLfix(Localize.SecondaryTranslation);
				}
			}
			switch (this.PrimaryTermModifier)
			{
			case Localize.TermModification.ToUpper:
				Localize.MainTranslation = Localize.MainTranslation.ToUpper();
				break;
			case Localize.TermModification.ToLower:
				Localize.MainTranslation = Localize.MainTranslation.ToLower();
				break;
			case Localize.TermModification.ToUpperFirst:
				Localize.MainTranslation = GoogleTranslation.UppercaseFirst(Localize.MainTranslation);
				break;
			case Localize.TermModification.ToTitle:
				Localize.MainTranslation = GoogleTranslation.TitleCase(Localize.MainTranslation);
				break;
			}
			switch (this.SecondaryTermModifier)
			{
			case Localize.TermModification.ToUpper:
				Localize.SecondaryTranslation = Localize.SecondaryTranslation.ToUpper();
				break;
			case Localize.TermModification.ToLower:
				Localize.SecondaryTranslation = Localize.SecondaryTranslation.ToLower();
				break;
			case Localize.TermModification.ToUpperFirst:
				Localize.SecondaryTranslation = GoogleTranslation.UppercaseFirst(Localize.SecondaryTranslation);
				break;
			case Localize.TermModification.ToTitle:
				Localize.SecondaryTranslation = GoogleTranslation.TitleCase(Localize.SecondaryTranslation);
				break;
			}
			this.EventDoLocalize(Localize.MainTranslation.Replace("<br>", "\n"), Localize.SecondaryTranslation);
			Localize.CurrentLocalizeComponent = null;
		}

		public bool FindTarget()
		{
			if (this.HasTargetCache())
			{
				return true;
			}
			if (this.EventFindTarget == null)
			{
				this.RegisterTargets();
			}
			this.EventFindTarget();
			return this.HasTargetCache();
		}

		public void FindAndCacheTarget<T>(ref T targetCache, Localize.DelegateSetFinalTerms setFinalTerms, Localize.DelegateDoLocalize doLocalize, bool UseSecondaryTerm, bool MainRTL, bool SecondRTL) where T : Component
		{
			if (this.mTarget != null)
			{
				targetCache = this.mTarget as T;
			}
			else
			{
				this.mTarget = (targetCache = base.GetComponent<T>());
			}
			if (targetCache != null)
			{
				this.EventSetFinalTerms = setFinalTerms;
				this.EventDoLocalize = doLocalize;
				this.CanUseSecondaryTerm = UseSecondaryTerm;
				this.AllowMainTermToBeRTL = MainRTL;
				this.AllowSecondTermToBeRTL = SecondRTL;
			}
		}

		private void FindAndCacheTarget(ref GameObject targetCache, Localize.DelegateSetFinalTerms setFinalTerms, Localize.DelegateDoLocalize doLocalize, bool UseSecondaryTerm, bool MainRTL, bool SecondRTL)
		{
			if (this.mTarget != targetCache && targetCache)
			{
				Object.Destroy(targetCache);
			}
			if (this.mTarget != null)
			{
				targetCache = this.mTarget as GameObject;
			}
			else
			{
				Transform transform = base.transform;
				GameObject gameObject;
				targetCache = (gameObject = ((transform.childCount >= 1) ? transform.GetChild(0).gameObject : null));
				this.mTarget = gameObject;
			}
			if (targetCache != null)
			{
				this.EventSetFinalTerms = setFinalTerms;
				this.EventDoLocalize = doLocalize;
				this.CanUseSecondaryTerm = UseSecondaryTerm;
				this.AllowMainTermToBeRTL = MainRTL;
				this.AllowSecondTermToBeRTL = SecondRTL;
			}
		}

		private bool HasTargetCache()
		{
			return this.EventDoLocalize != null;
		}

		public void GetFinalTerms(out string PrimaryTerm, out string SecondaryTerm)
		{
			if (this.EventSetFinalTerms == null || (!this.mTarget && !this.HasTargetCache()))
			{
				this.FindTarget();
			}
			PrimaryTerm = string.Empty;
			SecondaryTerm = string.Empty;
			if (this.mTarget != null && (string.IsNullOrEmpty(this.mTerm) || string.IsNullOrEmpty(this.mTermSecondary)) && this.EventSetFinalTerms != null)
			{
				this.EventSetFinalTerms(this.mTerm, this.mTermSecondary, out PrimaryTerm, out SecondaryTerm);
			}
			if (!string.IsNullOrEmpty(this.mTerm))
			{
				PrimaryTerm = this.mTerm;
			}
			if (!string.IsNullOrEmpty(this.mTermSecondary))
			{
				SecondaryTerm = this.mTermSecondary;
			}
		}

		public string GetMainTargetsText()
		{
			string text = null;
			string text2 = null;
			if (this.EventSetFinalTerms != null)
			{
				this.EventSetFinalTerms(null, null, out text, out text2);
			}
			return (!string.IsNullOrEmpty(text)) ? text : this.mTerm;
		}

		private void SetFinalTerms(string Main, string Secondary, out string PrimaryTerm, out string SecondaryTerm, bool RemoveNonASCII)
		{
			PrimaryTerm = ((!RemoveNonASCII || string.IsNullOrEmpty(Main)) ? Main : Regex.Replace(Main, "[^a-zA-Z0-9_ ]+", " "));
			SecondaryTerm = Secondary;
		}

		public void SetTerm(string primary, string secondary = null)
		{
			if (!string.IsNullOrEmpty(primary))
			{
				this.mTerm = primary;
				this.FinalTerm = primary;
			}
			if (!string.IsNullOrEmpty(secondary))
			{
				this.mTermSecondary = secondary;
				this.FinalSecondaryTerm = secondary;
			}
			this.OnLocalize(true);
		}

		private T GetSecondaryTranslatedObj<T>(ref string MainTranslation, ref string SecondaryTranslation) where T : Object
		{
			string text;
			string text2;
			this.DeserializeTranslation(MainTranslation, out text, out text2);
			T t = (T)((object)null);
			if (!string.IsNullOrEmpty(text2))
			{
				t = this.GetObject<T>(text2);
				if (t != null)
				{
					MainTranslation = text;
					SecondaryTranslation = text2;
				}
			}
			if (t == null)
			{
				t = this.GetObject<T>(SecondaryTranslation);
			}
			return t;
		}

		private T GetObject<T>(string Translation) where T : Object
		{
			if (string.IsNullOrEmpty(Translation))
			{
				return (T)((object)null);
			}
			T t = this.GetTranslatedObject<T>(Translation);
			if (t == null)
			{
				int num = Translation.LastIndexOfAny("/\\".ToCharArray());
				if (num >= 0)
				{
					Translation = Translation.Substring(num + 1);
					t = this.GetTranslatedObject<T>(Translation);
				}
			}
			return t;
		}

		private T GetTranslatedObject<T>(string Translation) where T : Object
		{
			return this.FindTranslatedObject<T>(Translation);
		}

		private void DeserializeTranslation(string translation, out string value, out string secondary)
		{
			if (!string.IsNullOrEmpty(translation) && translation.Length > 1 && translation[0] == '[')
			{
				int num = translation.IndexOf(']');
				if (num > 0)
				{
					secondary = translation.Substring(1, num - 1);
					value = translation.Substring(num + 1);
					return;
				}
			}
			value = translation;
			secondary = string.Empty;
		}

		public T FindTranslatedObject<T>(string value) where T : Object
		{
			if (string.IsNullOrEmpty(value))
			{
				return (T)((object)null);
			}
			if (this.TranslatedObjects != null)
			{
				int i = 0;
				int num = this.TranslatedObjects.Length;
				while (i < num)
				{
					if (this.TranslatedObjects[i] as T != null && value == this.TranslatedObjects[i].name)
					{
						return this.TranslatedObjects[i] as T;
					}
					i++;
				}
			}
			T t = LocalizationManager.FindAsset(value) as T;
			if (t)
			{
				return t;
			}
			return ResourceManager.pInstance.GetAsset<T>(value);
		}

		public bool HasTranslatedObject(Object Obj)
		{
			return Array.IndexOf<Object>(this.TranslatedObjects, Obj) >= 0 || ResourceManager.pInstance.HasAsset(Obj);
		}

		public void AddTranslatedObject(Object Obj)
		{
			Array.Resize<Object>(ref this.TranslatedObjects, this.TranslatedObjects.Length + 1);
			this.TranslatedObjects[this.TranslatedObjects.Length - 1] = Obj;
		}

		public void SetGlobalLanguage(string Language)
		{
			LocalizationManager.CurrentLanguage = Language;
		}

		public static void RegisterEvents_2DToolKit()
		{
		}

		public static void RegisterEvents_DFGUI()
		{
		}

		public static void RegisterEvents_NGUI()
		{
		}

		public static void RegisterEvents_SVG()
		{
		}

		public static void RegisterEvents_TextMeshPro()
		{
		}

		public void RegisterEvents_UGUI()
		{
			this.EventFindTarget += this.FindTarget_uGUI_Text;
			this.EventFindTarget += this.FindTarget_uGUI_Image;
			this.EventFindTarget += this.FindTarget_uGUI_RawImage;
		}

		private void FindTarget_uGUI_Text()
		{
			this.FindAndCacheTarget<Text>(ref this.mTarget_uGUI_Text, new Localize.DelegateSetFinalTerms(this.SetFinalTerms_uGUI_Text), new Localize.DelegateDoLocalize(this.DoLocalize_uGUI_Text), true, true, false);
		}

		private void FindTarget_uGUI_Image()
		{
			this.FindAndCacheTarget<Image>(ref this.mTarget_uGUI_Image, new Localize.DelegateSetFinalTerms(this.SetFinalTerms_uGUI_Image), new Localize.DelegateDoLocalize(this.DoLocalize_uGUI_Image), false, false, false);
		}

		private void FindTarget_uGUI_RawImage()
		{
			this.FindAndCacheTarget<RawImage>(ref this.mTarget_uGUI_RawImage, new Localize.DelegateSetFinalTerms(this.SetFinalTerms_uGUI_RawImage), new Localize.DelegateDoLocalize(this.DoLocalize_uGUI_RawImage), false, false, false);
		}

		private void SetFinalTerms_uGUI_Text(string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
		{
			string text = ((!(this.mTarget_uGUI_Text.font != null)) ? string.Empty : this.mTarget_uGUI_Text.font.name);
			this.SetFinalTerms(this.mTarget_uGUI_Text.text, text, out primaryTerm, out secondaryTerm, true);
		}

		public void SetFinalTerms_uGUI_Image(string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
		{
			this.SetFinalTerms(this.mTarget_uGUI_Image.mainTexture.name, null, out primaryTerm, out secondaryTerm, false);
		}

		public void SetFinalTerms_uGUI_RawImage(string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
		{
			this.SetFinalTerms(this.mTarget_uGUI_RawImage.texture.name, null, out primaryTerm, out secondaryTerm, false);
		}

		public static T FindInParents<T>(Transform tr) where T : Component
		{
			if (!tr)
			{
				return (T)((object)null);
			}
			T t = tr.GetComponent<T>();
			while (!t && tr)
			{
				t = tr.GetComponent<T>();
				tr = tr.parent;
			}
			return t;
		}

		public void DoLocalize_uGUI_Text(string MainTranslation, string SecondaryTranslation)
		{
			Font secondaryTranslatedObj = this.GetSecondaryTranslatedObj<Font>(ref MainTranslation, ref SecondaryTranslation);
			if (secondaryTranslatedObj != null && secondaryTranslatedObj != this.mTarget_uGUI_Text.font)
			{
				this.mTarget_uGUI_Text.font = secondaryTranslatedObj;
			}
			if (this.mInitializeAlignment)
			{
				this.mInitializeAlignment = false;
				this.mOriginalAlignmentUGUI = this.mTarget_uGUI_Text.alignment;
			}
			if (!string.IsNullOrEmpty(MainTranslation) && this.mTarget_uGUI_Text.text != MainTranslation)
			{
				if (Localize.CurrentLocalizeComponent.CorrectAlignmentForRTL)
				{
					if (this.mTarget_uGUI_Text.alignment == null || this.mTarget_uGUI_Text.alignment == 1 || this.mTarget_uGUI_Text.alignment == 2)
					{
						this.mTarget_uGUI_Text.alignment = ((!LocalizationManager.IsRight2Left) ? this.mOriginalAlignmentUGUI : 2);
					}
					else if (this.mTarget_uGUI_Text.alignment == 3 || this.mTarget_uGUI_Text.alignment == 4 || this.mTarget_uGUI_Text.alignment == 5)
					{
						this.mTarget_uGUI_Text.alignment = ((!LocalizationManager.IsRight2Left) ? this.mOriginalAlignmentUGUI : 5);
					}
					else if (this.mTarget_uGUI_Text.alignment == 6 || this.mTarget_uGUI_Text.alignment == 7 || this.mTarget_uGUI_Text.alignment == 8)
					{
						this.mTarget_uGUI_Text.alignment = ((!LocalizationManager.IsRight2Left) ? this.mOriginalAlignmentUGUI : 8);
					}
				}
				this.mTarget_uGUI_Text.text = MainTranslation;
				this.mTarget_uGUI_Text.SetVerticesDirty();
			}
		}

		public void DoLocalize_uGUI_Image(string MainTranslation, string SecondaryTranslation)
		{
			Sprite sprite = this.mTarget_uGUI_Image.sprite;
			if (sprite == null || sprite.name != MainTranslation)
			{
				this.mTarget_uGUI_Image.sprite = this.FindTranslatedObject<Sprite>(MainTranslation);
			}
		}

		public void DoLocalize_uGUI_RawImage(string MainTranslation, string SecondaryTranslation)
		{
			Texture texture = this.mTarget_uGUI_RawImage.texture;
			if (texture == null || texture.name != MainTranslation)
			{
				this.mTarget_uGUI_RawImage.texture = this.FindTranslatedObject<Texture>(MainTranslation);
			}
		}

		public void RegisterEvents_UnityStandard()
		{
			this.EventFindTarget += this.FindTarget_GUIText;
			this.EventFindTarget += this.FindTarget_TextMesh;
			this.EventFindTarget += this.FindTarget_AudioSource;
			this.EventFindTarget += this.FindTarget_GUITexture;
			this.EventFindTarget += this.FindTarget_Child;
			this.EventFindTarget += this.FindTarget_SpriteRenderer;
		}

		private void FindTarget_GUIText()
		{
			this.FindAndCacheTarget<GUIText>(ref this.mTarget_GUIText, new Localize.DelegateSetFinalTerms(this.SetFinalTerms_GUIText), new Localize.DelegateDoLocalize(this.DoLocalize_GUIText), true, true, false);
		}

		private void FindTarget_TextMesh()
		{
			this.FindAndCacheTarget<TextMesh>(ref this.mTarget_TextMesh, new Localize.DelegateSetFinalTerms(this.SetFinalTerms_TextMesh), new Localize.DelegateDoLocalize(this.DoLocalize_TextMesh), true, true, false);
		}

		private void FindTarget_AudioSource()
		{
			this.FindAndCacheTarget<AudioSource>(ref this.mTarget_AudioSource, new Localize.DelegateSetFinalTerms(this.SetFinalTerms_AudioSource), new Localize.DelegateDoLocalize(this.DoLocalize_AudioSource), false, false, false);
		}

		private void FindTarget_GUITexture()
		{
			this.FindAndCacheTarget<GUITexture>(ref this.mTarget_GUITexture, new Localize.DelegateSetFinalTerms(this.SetFinalTerms_GUITexture), new Localize.DelegateDoLocalize(this.DoLocalize_GUITexture), false, false, false);
		}

		private void FindTarget_Child()
		{
			this.FindAndCacheTarget(ref this.mTarget_Child, new Localize.DelegateSetFinalTerms(this.SetFinalTerms_Child), new Localize.DelegateDoLocalize(this.DoLocalize_Child), false, false, false);
		}

		private void FindTarget_SpriteRenderer()
		{
			this.FindAndCacheTarget<SpriteRenderer>(ref this.mTarget_SpriteRenderer, new Localize.DelegateSetFinalTerms(this.SetFinalTerms_SpriteRenderer), new Localize.DelegateDoLocalize(this.DoLocalize_SpriteRenderer), false, false, false);
		}

		public void SetFinalTerms_GUIText(string Main, string Secondary, out string PrimaryTerm, out string SecondaryTerm)
		{
			if (string.IsNullOrEmpty(Secondary) && this.mTarget_GUIText.font != null)
			{
				Secondary = this.mTarget_GUIText.font.name;
			}
			this.SetFinalTerms(this.mTarget_GUIText.text, Secondary, out PrimaryTerm, out SecondaryTerm, true);
		}

		public void SetFinalTerms_TextMesh(string Main, string Secondary, out string PrimaryTerm, out string SecondaryTerm)
		{
			string text = ((!(this.mTarget_TextMesh.font != null)) ? string.Empty : this.mTarget_TextMesh.font.name);
			this.SetFinalTerms(this.mTarget_TextMesh.text, text, out PrimaryTerm, out SecondaryTerm, true);
		}

		public void SetFinalTerms_GUITexture(string Main, string Secondary, out string PrimaryTerm, out string SecondaryTerm)
		{
			if (!this.mTarget_GUITexture || !this.mTarget_GUITexture.texture)
			{
				this.SetFinalTerms(string.Empty, string.Empty, out PrimaryTerm, out SecondaryTerm, false);
			}
			else
			{
				this.SetFinalTerms(this.mTarget_GUITexture.texture.name, string.Empty, out PrimaryTerm, out SecondaryTerm, false);
			}
		}

		public void SetFinalTerms_AudioSource(string Main, string Secondary, out string PrimaryTerm, out string SecondaryTerm)
		{
			if (!this.mTarget_AudioSource || !this.mTarget_AudioSource.clip)
			{
				this.SetFinalTerms(string.Empty, string.Empty, out PrimaryTerm, out SecondaryTerm, false);
			}
			else
			{
				this.SetFinalTerms(this.mTarget_AudioSource.clip.name, string.Empty, out PrimaryTerm, out SecondaryTerm, false);
			}
		}

		public void SetFinalTerms_Child(string Main, string Secondary, out string PrimaryTerm, out string SecondaryTerm)
		{
			this.SetFinalTerms(this.mTarget_Child.name, string.Empty, out PrimaryTerm, out SecondaryTerm, false);
		}

		public void SetFinalTerms_SpriteRenderer(string Main, string Secondary, out string PrimaryTerm, out string SecondaryTerm)
		{
			this.SetFinalTerms((!(this.mTarget_SpriteRenderer.sprite != null)) ? string.Empty : this.mTarget_SpriteRenderer.sprite.name, string.Empty, out PrimaryTerm, out SecondaryTerm, false);
		}

		private void DoLocalize_GUIText(string MainTranslation, string SecondaryTranslation)
		{
			Font secondaryTranslatedObj = this.GetSecondaryTranslatedObj<Font>(ref MainTranslation, ref SecondaryTranslation);
			if (secondaryTranslatedObj != null && this.mTarget_GUIText.font != secondaryTranslatedObj)
			{
				this.mTarget_GUIText.font = secondaryTranslatedObj;
			}
			if (this.mInitializeAlignment)
			{
				this.mInitializeAlignment = false;
				this.mOriginalAlignmentStd = this.mTarget_GUIText.alignment;
			}
			if (!string.IsNullOrEmpty(MainTranslation) && this.mTarget_GUIText.text != MainTranslation)
			{
				if (Localize.CurrentLocalizeComponent.CorrectAlignmentForRTL)
				{
					this.mTarget_GUIText.alignment = ((!LocalizationManager.IsRight2Left) ? this.mOriginalAlignmentStd : 2);
				}
				this.mTarget_GUIText.text = MainTranslation;
			}
		}

		private void DoLocalize_TextMesh(string MainTranslation, string SecondaryTranslation)
		{
			Font secondaryTranslatedObj = this.GetSecondaryTranslatedObj<Font>(ref MainTranslation, ref SecondaryTranslation);
			if (secondaryTranslatedObj != null && this.mTarget_TextMesh.font != secondaryTranslatedObj)
			{
				this.mTarget_TextMesh.font = secondaryTranslatedObj;
				base.GetComponent<Renderer>().sharedMaterial = secondaryTranslatedObj.material;
			}
			if (this.mInitializeAlignment)
			{
				this.mInitializeAlignment = false;
				this.mOriginalAlignmentStd = this.mTarget_TextMesh.alignment;
			}
			if (!string.IsNullOrEmpty(MainTranslation) && this.mTarget_TextMesh.text != MainTranslation)
			{
				if (Localize.CurrentLocalizeComponent.CorrectAlignmentForRTL)
				{
					this.mTarget_TextMesh.alignment = ((!LocalizationManager.IsRight2Left) ? this.mOriginalAlignmentStd : 2);
				}
				this.mTarget_TextMesh.text = MainTranslation;
			}
		}

		private void DoLocalize_AudioSource(string MainTranslation, string SecondaryTranslation)
		{
			bool isPlaying = this.mTarget_AudioSource.isPlaying;
			AudioClip clip = this.mTarget_AudioSource.clip;
			AudioClip audioClip = this.FindTranslatedObject<AudioClip>(MainTranslation);
			if (clip != audioClip)
			{
				this.mTarget_AudioSource.clip = audioClip;
			}
			if (isPlaying && this.mTarget_AudioSource.clip)
			{
				this.mTarget_AudioSource.Play();
			}
		}

		private void DoLocalize_GUITexture(string MainTranslation, string SecondaryTranslation)
		{
			Texture texture = this.mTarget_GUITexture.texture;
			if (texture != null && texture.name != MainTranslation)
			{
				this.mTarget_GUITexture.texture = this.FindTranslatedObject<Texture>(MainTranslation);
			}
		}

		private void DoLocalize_Child(string MainTranslation, string SecondaryTranslation)
		{
			if (this.mTarget_Child && this.mTarget_Child.name == MainTranslation)
			{
				return;
			}
			GameObject gameObject = this.mTarget_Child;
			GameObject gameObject2 = this.FindTranslatedObject<GameObject>(MainTranslation);
			if (gameObject2)
			{
				this.mTarget_Child = Object.Instantiate<GameObject>(gameObject2);
				Transform transform = this.mTarget_Child.transform;
				Transform transform2 = ((!gameObject) ? gameObject2.transform : gameObject.transform);
				transform.parent = base.transform;
				transform.localScale = transform2.localScale;
				transform.localRotation = transform2.localRotation;
				transform.localPosition = transform2.localPosition;
			}
			if (gameObject)
			{
				Object.Destroy(gameObject);
			}
		}

		private void DoLocalize_SpriteRenderer(string MainTranslation, string SecondaryTranslation)
		{
			Sprite sprite = this.mTarget_SpriteRenderer.sprite;
			if (sprite == null || sprite.name != MainTranslation)
			{
				this.mTarget_SpriteRenderer.sprite = this.FindTranslatedObject<Sprite>(MainTranslation);
			}
		}

		public string mTerm = string.Empty;

		public string mTermSecondary = string.Empty;

		[NonSerialized]
		public string FinalTerm;

		[NonSerialized]
		public string FinalSecondaryTerm;

		public Localize.TermModification PrimaryTermModifier;

		public Localize.TermModification SecondaryTermModifier;

		public bool LocalizeOnAwake = true;

		private string LastLocalizedLanguage;

		public Object mTarget;

		public Localize.DelegateSetFinalTerms EventSetFinalTerms;

		public Localize.DelegateDoLocalize EventDoLocalize;

		public bool CanUseSecondaryTerm;

		public bool AllowMainTermToBeRTL;

		public bool AllowSecondTermToBeRTL;

		public bool IgnoreRTL;

		public int MaxCharactersInRTL;

		public bool CorrectAlignmentForRTL = true;

		public Object[] TranslatedObjects;

		public EventCallback LocalizeCallBack = new EventCallback();

		public static string MainTranslation;

		public static string SecondaryTranslation;

		public static string CallBackTerm;

		public static string CallBackSecondaryTerm;

		public static Localize CurrentLocalizeComponent;

		public bool AlwaysForceLocalize;

		public bool mGUI_ShowReferences;

		public bool mGUI_ShowTems = true;

		public bool mGUI_ShowCallback;

		private Text mTarget_uGUI_Text;

		private Image mTarget_uGUI_Image;

		private RawImage mTarget_uGUI_RawImage;

		private TextAnchor mOriginalAlignmentUGUI;

		private GUIText mTarget_GUIText;

		private TextMesh mTarget_TextMesh;

		private AudioSource mTarget_AudioSource;

		private GUITexture mTarget_GUITexture;

		private GameObject mTarget_Child;

		private SpriteRenderer mTarget_SpriteRenderer;

		private bool mInitializeAlignment = true;

		private TextAlignment mOriginalAlignmentStd;

		public enum TermModification
		{
			DontModify,
			ToUpper,
			ToLower,
			ToUpperFirst,
			ToTitle
		}

		public delegate void DelegateSetFinalTerms(string Main, string Secondary, out string primaryTerm, out string secondaryTerm);

		public delegate void DelegateDoLocalize(string primaryTerm, string secondaryTerm);
	}
}
