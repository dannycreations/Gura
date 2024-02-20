using System;
using UnityEngine;

public class GUIEarth : MonoBehaviour
{
	private void Awake()
	{
		this.displayedPlanet = Object.Instantiate<GameObject>(this.planets[0], base.transform.position, this.planets[0].transform.rotation);
		this.displayedPlanet.transform.parent = base.transform;
		this.displayedPlanet.SetActive(false);
	}

	public Transform DisplayedPlanet
	{
		get
		{
			return this.displayedPlanet.transform;
		}
	}

	public Transform DisplayedPlanetParent
	{
		get
		{
			return this.displayedPlanet.transform.parent;
		}
	}

	public GameObject[] planets;

	private GameObject displayedPlanet;
}
