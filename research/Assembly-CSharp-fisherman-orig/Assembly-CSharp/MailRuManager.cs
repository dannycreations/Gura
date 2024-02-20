using System;
using UnityEngine;

public class MailRuManager : MonoBehaviour
{
	public static MailRuManager Instance
	{
		get
		{
			return MailRuManager._instance ?? new GameObject("MailRuManager").AddComponent<MailRuManager>();
		}
	}

	public static bool Initialized
	{
		get
		{
			return MailRuManager._instance != null && MailRuManager._instance._initialized;
		}
	}

	public void Awake()
	{
		Object.Destroy(base.gameObject);
	}

	private static MailRuManager _instance;

	private bool _initialized;
}
