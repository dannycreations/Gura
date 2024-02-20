using System;
using UnityEngine;

public class CustomPlayerPrefs : MonoBehaviour
{
	public static void Init(MonoBehaviour gameObject)
	{
	}

	public static void SetKeyPrefix(string prefix)
	{
		CustomPlayerPrefs.keyPrefix = prefix;
		Debug.Log("Prefs: Key prefix set!");
	}

	private static void OnPlayerPrefsInitialized()
	{
		PlayerPrefs.Save();
		CustomPlayerPrefs.havePrimedPlayerPrefs = true;
	}

	public static bool HasKey(string _key)
	{
		return PlayerPrefs.HasKey(CustomPlayerPrefs.keyPrefix + _key);
	}

	public static void DeleteKey(string _key)
	{
		PlayerPrefs.DeleteKey(CustomPlayerPrefs.keyPrefix + _key);
	}

	public static void Save()
	{
		PlayerPrefs.Save();
	}

	public static void SetInt(string _key, int value)
	{
		PlayerPrefs.SetInt(CustomPlayerPrefs.keyPrefix + _key, value);
		PlayerPrefs.Save();
	}

	public static void SetFloat(string _key, float _value)
	{
		PlayerPrefs.SetFloat(CustomPlayerPrefs.keyPrefix + _key, _value);
		PlayerPrefs.Save();
	}

	public static void SetString(string _key, string _value)
	{
		PlayerPrefs.SetString(CustomPlayerPrefs.keyPrefix + _key, _value);
		PlayerPrefs.Save();
	}

	public static int GetInt(string _key)
	{
		return PlayerPrefs.GetInt(CustomPlayerPrefs.keyPrefix + _key);
	}

	public static int GetInt(string _key, int _defaultValue)
	{
		return PlayerPrefs.GetInt(CustomPlayerPrefs.keyPrefix + _key, _defaultValue);
	}

	public static float GetFloat(string _key)
	{
		return PlayerPrefs.GetFloat(CustomPlayerPrefs.keyPrefix + _key);
	}

	public static float GetFloat(string _key, float _defaultValue)
	{
		return PlayerPrefs.GetFloat(CustomPlayerPrefs.keyPrefix + _key, _defaultValue);
	}

	public static string GetString(string _key)
	{
		return PlayerPrefs.GetString(CustomPlayerPrefs.keyPrefix + _key);
	}

	public static string GetString(string _key, string _defaultValue)
	{
		return PlayerPrefs.GetString(CustomPlayerPrefs.keyPrefix + _key, _defaultValue);
	}

	private static bool havePrimedPlayerPrefs = false;

	private static string keyPrefix = string.Empty;
}
