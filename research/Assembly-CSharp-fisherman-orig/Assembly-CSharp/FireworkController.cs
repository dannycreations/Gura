using System;
using Assets.Scripts.Common.Managers.Helpers;
using ObjectModel;
using UnityEngine;

public class FireworkController : MonoBehaviour
{
	public static FireworkController Instance
	{
		get
		{
			return FireworkController._instance;
		}
	}

	private void Start()
	{
		FireworkController._instance = this;
		PhotonConnectionFactory.Instance.OnLocalEvent += this.OnLocalEvent;
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnLocalEvent -= this.OnLocalEvent;
	}

	public void OnLocalEvent(LocalEvent localEvent)
	{
	}

	public void RunFirework(Firework firework, Vector3 position)
	{
		GameObject gameObject = (GameObject)Resources.Load(firework.LaunchAsset, typeof(GameObject));
		if (gameObject == null)
		{
			throw new PrefabException(string.Format("firework: {0} prefab can't instantiate", firework.LaunchAsset));
		}
		GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject, position, Quaternion.identity);
		gameObject2.transform.parent = null;
	}

	private static FireworkController _instance = null;

	private static PondHelpers _pondHelpers = new PondHelpers();
}
