using System;
using UnityEngine;

public class UnluckFlagGUI : MonoBehaviour
{
	public void Start()
	{
		this.Swap();
		if (this.txt == null)
		{
			this.txt = base.transform.Find("txt").GetComponent<TextMesh>();
		}
		if (this.nextButton == null)
		{
			this.nextButton = base.transform.Find("nextButton").GetComponent<GameObject>();
		}
		if (this.prevButton == null)
		{
			this.prevButton = base.transform.Find("prevButton").GetComponent<GameObject>();
		}
		if (this.bgrButton == null)
		{
			this.bgrButton = base.transform.Find("bgrButton").GetComponent<GameObject>();
		}
		if (this.lightButton == null)
		{
			this.lightButton = base.transform.Find("lightButton").gameObject;
		}
		if (this.texturePreview == null)
		{
			this.texturePreview = base.transform.Find("texturePreview").GetComponent<GameObject>();
		}
		if (this.debug == null)
		{
			this.debug = base.transform.Find("debug").GetComponent<TextMesh>();
		}
	}

	public void Update()
	{
		if (Input.GetMouseButtonUp(0))
		{
			this.ButtonUp();
		}
		if (Input.GetKeyUp("right"))
		{
			this.Next();
		}
		if (Input.GetKeyUp("left"))
		{
			this.Prev();
		}
		if (Input.GetKeyUp("space"))
		{
			this.nextButton.SetActive(!this.nextButton.activeInHierarchy);
			this.prevButton.SetActive(this.nextButton.activeInHierarchy);
			this.bgrButton.SetActive(this.nextButton.activeInHierarchy);
			this.texturePreview.SetActive(this.nextButton.activeInHierarchy);
			this.txt.gameObject.SetActive(this.nextButton.activeInHierarchy);
			this.lightButton.gameObject.SetActive(this.nextButton.activeInHierarchy);
		}
	}

	public void ButtonUp()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit raycastHit = default(RaycastHit);
		if (Physics.Raycast(ray, ref raycastHit))
		{
			if (raycastHit.transform.gameObject == this.nextButton)
			{
				this.Next();
			}
			else if (raycastHit.transform.gameObject == this.prevButton)
			{
				this.Prev();
			}
			else if (raycastHit.transform.gameObject == this.bgrButton)
			{
				this.NextBgr();
			}
			else if (raycastHit.transform.gameObject == this.lightButton)
			{
				this.LightChange();
			}
		}
	}

	public void LightChange()
	{
		if (this.lights.Length > 0)
		{
			this.lights[this.lCounter].enabled = false;
			this.lCounter++;
			if (this.lCounter >= this.lights.Length)
			{
				this.lCounter = 0;
			}
			this.lights[this.lCounter].enabled = true;
		}
	}

	public void NextBgr()
	{
		if (this.bgrs.Length > 0)
		{
			this.bCounter++;
			if (this.bCounter >= this.bgrs.Length)
			{
				this.bCounter = 0;
			}
			RenderSettings.skybox = this.bgrs[this.bCounter];
		}
	}

	public void Next()
	{
		this.counter++;
		if (this.counter > this.prefabs.Length - 1)
		{
			this.counter = 0;
		}
		this.Swap();
	}

	public void Prev()
	{
		this.counter--;
		if (this.counter < 0)
		{
			this.counter = this.prefabs.Length - 1;
		}
		this.Swap();
	}

	public void Swap()
	{
		if (this.prefabs.Length > 0)
		{
			Object.Destroy(this.activeObj);
			GameObject gameObject = Object.Instantiate<GameObject>(this.prefabs[this.counter]);
			this.activeObj = gameObject;
			if (this.txt != null)
			{
				this.txt.text = this.activeObj.name;
				this.txt.text = this.txt.text.Replace("(Clone)", string.Empty);
				this.txt.text = this.txt.text + " " + this.activeObj.GetComponent<UnluckAnimatedMesh>().meshContainerFBX.name;
				this.txt.text = this.txt.text.Replace("_", " ");
				this.txt.text = this.txt.text.Replace("Flag ", string.Empty);
			}
			if (this.texturePreview != null)
			{
				this.texturePreview.GetComponent<Renderer>().sharedMaterial = this.activeObj.GetComponent<Renderer>().sharedMaterial;
			}
		}
	}

	public GameObject[] prefabs;

	public Material[] bgrs;

	public Light[] lights;

	public GameObject nextButton;

	public GameObject prevButton;

	public GameObject bgrButton;

	public GameObject lightButton;

	public GameObject texturePreview;

	private GameObject activeObj;

	private int counter;

	private int bCounter;

	private int lCounter;

	public TextMesh txt;

	public TextMesh debug;
}
