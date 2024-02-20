using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class CapturedBackground : MonoBehaviour
{
	public static Texture2D CaptureImage(Camera camera, int width, int height, Quaternion? rotation = null, Vector3? position = null)
	{
		CapturedBackground capturedBackground = new CapturedBackground();
		Camera camera2 = capturedBackground.CreateBackgroundCamera(camera);
		Texture2D texture2D = new Texture2D(width, height, 3, false);
		if (rotation != null)
		{
			camera2.transform.rotation = rotation.Value;
		}
		if (position != null)
		{
			camera2.transform.position = position.Value;
		}
		camera2.Render();
		RenderTexture.active = camera2.targetTexture;
		Debug.Log(string.Concat(new object[] { "Width: ", width, "  Heigth: ", height }));
		texture2D.ReadPixels(new Rect(0f, 0f, (float)(width - 1), (float)(height - 1)), 0, 0);
		texture2D.Apply();
		RenderTexture.active = null;
		Object.Destroy(camera2.GetComponent<Blur>());
		Object.Destroy(camera2);
		return texture2D;
	}

	private Camera CreateBackgroundCamera(Camera cam)
	{
		string text = "Blur" + cam.name;
		GameObject gameObject = GameObject.Find(text);
		if (!gameObject)
		{
			gameObject = new GameObject(text, new Type[] { typeof(Camera) });
		}
		if (!gameObject.GetComponent(typeof(Camera)))
		{
			gameObject.AddComponent(typeof(Camera));
		}
		Camera component = gameObject.GetComponent<Camera>();
		component.CopyFrom(cam);
		Blur blur = component.gameObject.AddComponent<Blur>();
		blur.blurShader = Shader.Find("Hidden/FastBlur");
		return component;
	}
}
