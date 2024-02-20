using System;
using System.Collections.Generic;
using Photon;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class InRoomChat : MonoBehaviour
{
	public void Start()
	{
		if (this.AlignBottom)
		{
			this.GuiRect.y = (float)Screen.height - this.GuiRect.height;
		}
	}

	public void Chat(string newLine, PhotonMessageInfo mi)
	{
		string text = "anonymous";
		if (mi != null && mi.sender != null)
		{
			if (!string.IsNullOrEmpty(mi.sender.name))
			{
				text = mi.sender.name;
			}
			else
			{
				text = "player " + mi.sender.ID;
			}
		}
		this.messages.Add(text + ": " + newLine);
	}

	public void AddLine(string newLine)
	{
		this.messages.Add(newLine);
	}

	public Rect GuiRect = new Rect(0f, 0f, 250f, 300f);

	public bool IsVisible = true;

	public bool AlignBottom;

	public List<string> messages = new List<string>();

	private string inputLine = string.Empty;

	private Vector2 scrollPos = Vector2.zero;

	public static readonly string ChatRPC = "Chat";
}
