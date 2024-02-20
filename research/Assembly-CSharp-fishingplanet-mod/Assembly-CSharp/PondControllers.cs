using System;
using Assets.Scripts.Common.Managers.Helpers;
using Cayman;
using UnityEngine;
using UnityEngine.EventSystems;

public class PondControllers : MonoBehaviour
{
	private void Awake()
	{
		PondControllers.Instance = this;
	}

	public Transform Collider
	{
		get
		{
			return this.FirstPerson.transform.parent;
		}
	}

	public bool IsInMenu
	{
		get
		{
			return this.PondMainMenu != null && this.PondMainMenu.activeInHierarchy && MenuHelpers.Instance.MenuPrefabsList.MainMenuCanvasGroup.interactable;
		}
	}

	public void ActivateGameHUD()
	{
		this.Game3DPond.SetActive(true);
		this.Game3DPond.SetActive(false);
	}

	public void HideMenu(bool disableGuiCamera = true, bool hideGameBg = true, bool hideMenuList = true)
	{
		if (hideGameBg)
		{
			ManagerScenes.Instance.Background.SetVisibility(false);
		}
		if (hideMenuList)
		{
			MenuHelpers.Instance.MenuPrefabsList.HideMenu();
		}
		MenuHelpers.Instance.MenuPrefabsList.SetMenuParentVisibility(false);
		if (disableGuiCamera)
		{
			MenuHelpers.Instance.SetEnabledGUICamera(false);
		}
	}

	public void ShowMenu(bool enableGuiCamera = true, bool showGameBg = true)
	{
		if (showGameBg)
		{
			ManagerScenes.Instance.Background.SetVisibility(true);
		}
		if (enableGuiCamera)
		{
			MenuHelpers.Instance.SetEnabledGUICamera(true);
		}
		MenuHelpers.Instance.MenuPrefabsList.SetMenuParentVisibility(true);
	}

	public void ShowGame()
	{
		Debug.Log("PondControllers::ShowGame()");
		if (GameFactory.Player != null)
		{
			GameFactory.Player.SetViewPause(false);
		}
		if (GameFactory.AudioController != null)
		{
			GameFactory.AudioController.InGameOnVolume();
		}
		if (CaymanGenerator.Instance != null)
		{
			CaymanGenerator.Instance.gameObject.SetActive(true);
		}
		CameraWetness component = this.FirstPerson.GetComponent<CameraWetness>();
		if (component != null)
		{
			component.UpdateVolume(true);
		}
		this.Game3DPond.SetActive(true);
		GameFactory.Player.RodPodsSetActive(true);
		this.FirstPerson.SetActive(true);
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		CursorManager.HideCursor();
		base.Invoke("SendFps", 60f);
	}

	public void HideGame()
	{
		Debug.Log("PondControllers::HideGame()");
		if (GameFactory.AudioController != null)
		{
			GameFactory.AudioController.InGameOffVolume();
		}
		if (CaymanGenerator.Instance != null)
		{
			CaymanGenerator.Instance.gameObject.SetActive(false);
		}
		CameraWetness component = this.FirstPerson.GetComponent<CameraWetness>();
		if (component != null)
		{
			component.UpdateVolume(false);
		}
		this.Game3DPond.SetActive(false);
		GameFactory.Player.RodPodsSetActive(false);
		this.FirstPerson.SetActive(false);
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		CursorManager.ShowCursor();
		if (GameFactory.Player != null)
		{
			GameFactory.Player.SetViewPause(true);
		}
	}

	public void ShowPlayer()
	{
		if (GameFactory.Player != null)
		{
			GameFactory.Player.SetViewPause(false);
		}
		CameraWetness component = this.FirstPerson.GetComponent<CameraWetness>();
		if (component != null)
		{
			component.UpdateVolume(true);
		}
		this.FirstPerson.SetActive(true);
	}

	public void HidePlayer()
	{
		CameraWetness component = this.FirstPerson.GetComponent<CameraWetness>();
		if (component != null)
		{
			component.UpdateVolume(false);
		}
		if (GameFactory.AudioController != null)
		{
			GameFactory.AudioController.InGameOffVolume();
		}
		this.FirstPerson.SetActive(false);
		if (GameFactory.Player != null)
		{
			GameFactory.Player.SetViewPause(true);
		}
	}

	public void SendFps()
	{
		PhotonConnectionFactory.Instance.PinFps(FPSController.GetFps());
	}

	public GameObject PondFirstSceneForm;

	public GameObject Game3DPond;

	public GameObject PondMainMenu;

	public GameObject FirstPerson;

	public static PondControllers Instance;
}
