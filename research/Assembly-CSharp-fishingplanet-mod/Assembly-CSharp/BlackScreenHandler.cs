using System;
using UnityEngine;

public class BlackScreenHandler
{
	public static void Show(bool immediately = false, float? hideFadeTime = null)
	{
		if (BlackScreenHandler.currentScreen != null)
		{
			return;
		}
		BlackScreenHandler.currentScreen = GUITools.AddChild(null, MessageBoxList.Instance.blackScreen);
		BlackScreenHandler.currentScreen.transform.SetAsLastSibling();
		AlphaFade component = BlackScreenHandler.currentScreen.GetComponent<AlphaFade>();
		if (immediately)
		{
			component.FastShowPanel();
		}
		else
		{
			component.ShowPanel();
		}
		component.HideFinished += BlackScreenHandler.AlphaFade_HideFinished;
		if (hideFadeTime != null)
		{
			component.HideFadeTime = hideFadeTime.Value;
		}
		BlackScreenHandler.currentScreen.SetActive(true);
		CursorManager.ShowCursor();
		CursorManager.BlockFPS();
		RectTransform component2 = BlackScreenHandler.currentScreen.GetComponent<RectTransform>();
		component2.anchoredPosition = new Vector3(0f, 0f, 0f);
		component2.sizeDelta = new Vector2(0f, 0f);
	}

	public static void Hide()
	{
		if (BlackScreenHandler.currentScreen != null)
		{
			BlackScreenHandler.currentScreen.GetComponent<AlphaFade>().HidePanel();
		}
	}

	public static void HideFast()
	{
		if (BlackScreenHandler.currentScreen != null)
		{
			AlphaFade component = BlackScreenHandler.currentScreen.GetComponent<AlphaFade>();
			component.FastHidePanel();
			component.FinishHideInvokes();
		}
	}

	private static void AlphaFade_HideFinished(object sender, EventArgsAlphaFade e)
	{
		if (BlackScreenHandler.currentScreen != null)
		{
			CursorManager.HideCursor();
			CursorManager.UnBlockFPS();
			Object.Destroy(BlackScreenHandler.currentScreen);
			BlackScreenHandler.currentScreen = null;
		}
	}

	private static GameObject currentScreen;
}
