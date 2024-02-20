using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using TPM.Customizaion;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LoadCustomizationScene : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> UnloadedScene;

	public void OnClick()
	{
		if (!this.IsLoadingScene)
		{
			UINavigation.SetSelectedGameObject(null);
			if (StaticUserData.CurrentPond == null)
			{
				base.StartCoroutine(this.LoadNewLevel(this.nextSceneName, false));
			}
			else
			{
				MessageBox messageBox = MenuHelpers.Instance.ShowMessage(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("CantChangeAppearanceText"), true, false, false, null);
				messageBox.GetComponent<EventAction>().ActionCalled += delegate(object obj, EventArgs args)
				{
					messageBox.Close();
				};
			}
		}
	}

	public void ShowFirstTime()
	{
		base.StartCoroutine(this.LoadNewLevel(this.nextSceneName, true));
	}

	private IEnumerator LoadNewLevel(string sceneName, bool isFirstTime = false)
	{
		LoadCustomizationScene.ActiveInstance = this;
		this.IsLoadingScene = true;
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Loading);
		AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, 1);
		UIHelper.Waiting(true, null);
		while (!async.isDone)
		{
			yield return async;
		}
		UIHelper.Waiting(false, null);
		async.allowSceneActivation = false;
		Scene nextScene = SceneManager.GetSceneByName(sceneName);
		Scene activeScene = SceneManager.GetActiveScene();
		this.DisableObjects(isFirstTime);
		SceneManager.SetActiveScene(nextScene);
		async.allowSceneActivation = true;
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		this.IsLoadingScene = false;
		yield break;
	}

	private void DisableObjects(bool isFirstTime = false)
	{
		CustomizationPages customizationPages = Object.FindObjectOfType<CustomizationPages>();
		if (customizationPages != null)
		{
			CustomizationPages customizationPages2 = customizationPages;
			customizationPages2.CloseAction = (CustomizationPages.CloseActionDelegate)Delegate.Combine(customizationPages2.CloseAction, new CustomizationPages.CloseActionDelegate(this.FinishCustomization));
			customizationPages.CurrentModes = ((!isFirstTime) ? CustomizationPages.Modes.RECUSTOMIZATION : CustomizationPages.Modes.FISRT_CUSTOMIZATION);
		}
		if (PondControllers.Instance != null)
		{
			PondControllers.Instance.PondMainMenu.SetActive(false);
			PondControllers.Instance.FirstPerson.transform.parent.parent.gameObject.SetActive(false);
		}
		if (this.disableObjects != null)
		{
			for (int i = 0; i < this.disableObjects.Count; i++)
			{
				if (this.disableObjects[i] != null)
				{
					this.disableObjects[i].SetActive(false);
				}
			}
		}
		MenuHelpers.Instance.SetEnabledGUICamera(false);
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
	}

	private void ReenableObjects()
	{
		MenuHelpers.Instance.SetEnabledGUICamera(true);
		if (PondControllers.Instance != null)
		{
			PondControllers.Instance.PondMainMenu.SetActive(true);
			PondControllers.Instance.FirstPerson.transform.parent.parent.gameObject.SetActive(true);
		}
		if (this.disableObjects != null)
		{
			for (int i = 0; i < this.disableObjects.Count; i++)
			{
				if (this.disableObjects[i] != null)
				{
					this.disableObjects[i].SetActive(true);
				}
			}
		}
	}

	public void UnloadScene()
	{
		LoadCustomizationScene.ActiveInstance = null;
		SceneManager.UnloadScene(SceneManager.GetSceneByName(this.nextSceneName).buildIndex);
		this.ReenableObjects();
	}

	private void FinishCustomization()
	{
		this.UnloadScene();
		if (this.UnloadedScene != null)
		{
			this.UnloadedScene(this, new EventArgs());
		}
	}

	private string nextSceneName = "CharacterCustomization";

	public static LoadCustomizationScene ActiveInstance;

	public List<GameObject> disableObjects;

	public bool IsLoadingScene;

	public delegate void CloseAction();
}
