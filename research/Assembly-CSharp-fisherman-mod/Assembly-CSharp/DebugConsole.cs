using System;
using System.Collections;
using UnityEngine;

public class DebugConsole : MonoBehaviour
{
	public static bool isVisible
	{
		get
		{
			return DebugConsole.instance.visible;
		}
		set
		{
			DebugConsole.instance.visible = value;
			if (value)
			{
				DebugConsole.instance.Display();
			}
			else if (!value)
			{
				DebugConsole.instance.ClearScreen();
			}
		}
	}

	public static bool isDraggable
	{
		get
		{
			return DebugConsole.instance.draggable;
		}
		set
		{
			DebugConsole.instance.draggable = value;
		}
	}

	public static DebugConsole instance
	{
		get
		{
			if (DebugConsole.s_Instance == null)
			{
				DebugConsole.s_Instance = Object.FindObjectOfType(typeof(DebugConsole)) as DebugConsole;
				if (DebugConsole.s_Instance == null)
				{
					GameObject gameObject = new GameObject();
					gameObject.AddComponent<DebugConsole>();
					gameObject.name = "DebugConsoleController";
					DebugConsole.s_Instance = Object.FindObjectOfType(typeof(DebugConsole)) as DebugConsole;
					DebugConsole.instance.InitGuis();
				}
			}
			return DebugConsole.s_Instance;
		}
	}

	private void Awake()
	{
		DebugConsole.s_Instance = this;
		this.InitGuis();
	}

	public void InitGuis()
	{
		float num = this.lineSpacing;
		this.screenHeight = (float)Screen.height;
		if (this.pixelCorrect)
		{
			num = 1f / this.screenHeight * num;
		}
		if (!this.guisCreated)
		{
			if (this.DebugGui == null)
			{
				this.DebugGui = new GameObject();
				this.DebugGui.AddComponent<GUIText>();
				this.DebugGui.name = "DebugGUI(0)";
				this.DebugGui.transform.position = this.defaultGuiPosition;
				this.DebugGui.transform.localScale = this.defaultGuiScale;
			}
			Vector3 vector = this.DebugGui.transform.position;
			this.guis.Add(this.DebugGui);
			for (int i = 1; i < this.maxMessages; i++)
			{
				vector.y -= num;
				GameObject gameObject = Object.Instantiate<GameObject>(this.DebugGui, vector, base.transform.rotation);
				gameObject.name = string.Format("DebugGUI({0})", i);
				this.guis.Add(gameObject);
				vector = gameObject.transform.position;
			}
			for (int i = 0; i < this.guis.Count; i++)
			{
				GameObject gameObject2 = (GameObject)this.guis[i];
				gameObject2.transform.parent = this.DebugGui.transform;
			}
			this.guisCreated = true;
		}
		else
		{
			Vector3 position = this.DebugGui.transform.position;
			for (int j = 0; j < this.guis.Count; j++)
			{
				position.y -= num;
				GameObject gameObject3 = (GameObject)this.guis[j];
				gameObject3.transform.position = position;
			}
		}
	}

	private void Update()
	{
		if (this.visible && this.screenHeight != (float)Screen.height)
		{
			this.InitGuis();
		}
		if (this.draggable)
		{
			if (Input.GetMouseButtonDown(0))
			{
				if (!this.connectedToMouse && this.DebugGui.GetComponent<GUIText>().HitTest(Input.mousePosition))
				{
					this.connectedToMouse = true;
				}
				else if (this.connectedToMouse)
				{
					this.connectedToMouse = false;
				}
			}
			if (this.connectedToMouse)
			{
				float num = this.DebugGui.transform.position.x;
				float num2 = this.DebugGui.transform.position.y;
				num = Input.mousePosition.x / (float)Screen.width;
				num2 = Input.mousePosition.y / (float)Screen.height;
				this.DebugGui.transform.position = new Vector3(num, num2, 0f);
			}
		}
	}

	public static void Log(string message, string color)
	{
		DebugConsole.instance.AddMessage(message, color);
	}

	public static void Log(string message)
	{
		DebugConsole.instance.AddMessage(message);
	}

	public static void Clear()
	{
		DebugConsole.instance.ClearMessages();
	}

	public void AddMessage(string message, string color)
	{
		this.messages.Add(message);
		this.colors.Add(color);
		this.Display();
	}

	public void AddMessage(string message)
	{
		this.messages.Add(message);
		this.colors.Add("normal");
		this.Display();
	}

	public void ClearMessages()
	{
		this.messages.Clear();
		this.colors.Clear();
		this.ClearScreen();
	}

	private void ClearScreen()
	{
		if (this.guis.Count >= this.maxMessages)
		{
			for (int i = 0; i < this.guis.Count; i++)
			{
				GameObject gameObject = (GameObject)this.guis[i];
				gameObject.GetComponent<GUIText>().text = string.Empty;
			}
		}
	}

	private void Prune()
	{
		if (this.messages.Count > this.maxMessages)
		{
			int num;
			if (this.messages.Count <= 0)
			{
				num = 0;
			}
			else
			{
				num = this.messages.Count - this.maxMessages;
			}
			this.messages.RemoveRange(0, num);
			this.colors.RemoveRange(0, num);
		}
	}

	private void Display()
	{
		if (!this.visible)
		{
			this.ClearScreen();
		}
		else if (this.visible)
		{
			if (this.messages.Count > this.maxMessages)
			{
				this.Prune();
			}
			int i = 0;
			if (this.guis.Count >= this.maxMessages)
			{
				while (i < this.messages.Count)
				{
					GameObject gameObject = (GameObject)this.guis[i];
					string text = (string)this.colors[i];
					if (text != null)
					{
						if (!(text == "normal"))
						{
							if (!(text == "warning"))
							{
								if (text == "error")
								{
									gameObject.GetComponent<GUIText>().material.color = this.error;
								}
							}
							else
							{
								gameObject.GetComponent<GUIText>().material.color = this.warning;
							}
						}
						else
						{
							gameObject.GetComponent<GUIText>().material.color = this.normal;
						}
					}
					gameObject.GetComponent<GUIText>().text = (string)this.messages[i];
					i++;
				}
			}
		}
	}

	public GameObject DebugGui;

	public Vector3 defaultGuiPosition = new Vector3(0.01f, 0.98f, 0f);

	public Vector3 defaultGuiScale = new Vector3(0.5f, 0.5f, 1f);

	public Color normal = Color.green;

	public Color warning = Color.yellow;

	public Color error = Color.red;

	public int maxMessages = 30;

	public float lineSpacing = 0.02f;

	public ArrayList messages = new ArrayList();

	public ArrayList guis = new ArrayList();

	public ArrayList colors = new ArrayList();

	public bool draggable = true;

	public bool visible = true;

	public bool pixelCorrect;

	private static DebugConsole s_Instance;

	protected bool guisCreated;

	protected float screenHeight = -1f;

	private bool connectedToMouse;
}
