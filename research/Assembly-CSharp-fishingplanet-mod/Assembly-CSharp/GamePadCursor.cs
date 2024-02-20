using System;
using UnityEngine;

public class GamePadCursor : MonoBehaviour
{
	public static Vector3 position
	{
		get
		{
			return GamePadCursor.getInstance()._position;
		}
	}

	public static Vector3 scrollDelta
	{
		get
		{
			return GamePadCursor.getInstance()._scrollDelta;
		}
	}

	public static CursorLockMode lockState
	{
		get
		{
			return GamePadCursor.getInstance()._lockState;
		}
		set
		{
			GamePadCursor.getInstance()._lockState = value;
		}
	}

	public static bool visible
	{
		get
		{
			return GamePadCursor.getInstance()._visible;
		}
		set
		{
			GamePadCursor.getInstance()._visible = value;
		}
	}

	public static void SetCursor(Texture cursorTexture, Vector2 hotSpot)
	{
		GamePadCursor.getInstance().setCursor(cursorTexture, hotSpot);
	}

	public static void UpdatePositionFromMouseCursor()
	{
		if (Input.mousePosition.x > 0f && Input.mousePosition.x < (float)Screen.width && Input.mousePosition.y > 0f && Input.mousePosition.y < (float)Screen.height && GamePadCursor.getInstance()._position.x == (float)(Screen.width / 2) && GamePadCursor.getInstance()._position.y == (float)(Screen.height / 2))
		{
			GamePadCursor.getInstance()._position = Input.mousePosition;
		}
	}

	private static GamePadCursor getInstance()
	{
		if (GamePadCursor._instance == null)
		{
			GameObject gameObject = new GameObject("GamePadCursor");
			gameObject.AddComponent<GamePadCursor>();
		}
		return GamePadCursor._instance;
	}

	private void setCursor(Texture cursorTexture, Vector2 hotSpot)
	{
		this._cursorTexture = cursorTexture;
		this._hotSpot = hotSpot;
	}

	private void Awake()
	{
		if (GamePadCursor._instance != null)
		{
			Object.Destroy(this);
			return;
		}
		GamePadCursor._instance = this;
		Object.DontDestroyOnLoad(this);
		this._position = new Vector3((float)(Screen.width / 2), (float)(Screen.height / 2), 0f);
	}

	private static GamePadCursor _instance;

	private Vector3 _position = Vector3.zero;

	private Vector2 _scrollDelta = Vector2.zero;

	private Texture _cursorTexture;

	private bool _visible = true;

	private Vector2 _hotSpot = Vector2.zero;

	private CursorLockMode _lockState;
}
