using System;
using System.Collections;
using UnityEngine;

public class rippleSharp : MonoBehaviour
{
	private void Start()
	{
		MeshFilter meshFilter = (MeshFilter)base.GetComponent(typeof(MeshFilter));
		this.mesh = meshFilter.mesh;
		GameFactory.Water.SetDynWaterPosition(54f, 14f);
	}

	private void Update()
	{
		this.checkInput();
	}

	private GameObject CreateSplash2(Vector3 coord, string prefabName, float speed, bool autodestruct = true)
	{
		GameObject gameObject = (GameObject)Resources.Load(prefabName, typeof(GameObject));
		if (gameObject == null)
		{
			throw new PrefabException(string.Format("splash: {0} prefab can't instantiate", prefabName));
		}
		GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject, coord, Quaternion.identity);
		if (this.CameraTransform != null)
		{
			gameObject2.transform.LookAt(this.CameraTransform);
		}
		ParticleSystem[] componentsInChildren = gameObject2.GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			particleSystem.playbackSpeed = speed;
			if (particleSystem.name == "horizontal")
			{
				particleSystem.startRotation = (gameObject2.transform.rotation.eulerAngles.y - 90f) * 0.017453292f;
			}
			particleSystem.Play();
			particleSystem.startSize = 2f;
		}
		if (autodestruct)
		{
			Object.Destroy(gameObject2, 1.5f);
		}
		return gameObject2;
	}

	private IEnumerator StartSplash(RaycastHit hit, Bounds bounds, float splashSize)
	{
		float xcoord = (hit.textureCoord.x - 0.5f) * bounds.size.x * -20f;
		float zcoord = (hit.textureCoord.y - 0.5f) * bounds.size.z * -20f;
		DynWaterParticlesController.CreateSplash(this.CameraTransform, new Vector3(xcoord, 0f, zcoord), "2D/Splashes/pSplash_universal", splashSize, 1f, true, true, 1);
		yield return 0;
		yield break;
	}

	private void checkInput()
	{
		RaycastHit raycastHit;
		if (Input.GetMouseButton(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), ref raycastHit))
		{
			Bounds bounds = this.mesh.bounds;
		}
		RaycastHit raycastHit2;
		if (Input.GetMouseButton(1) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), ref raycastHit2))
		{
			Bounds bounds2 = this.mesh.bounds;
		}
		RaycastHit raycastHit3;
		if (Input.GetButtonDown("Fire1") && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), ref raycastHit3))
		{
			Bounds bounds3 = this.mesh.bounds;
		}
		RaycastHit raycastHit4;
		if (Input.GetButtonDown("Fire2") && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), ref raycastHit4))
		{
			Bounds bounds4 = this.mesh.bounds;
		}
	}

	public Transform CameraTransform;

	private Mesh mesh;
}
