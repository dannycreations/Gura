using System;
using System.Collections.Generic;
using Phy;
using UnityEngine;

public static class BoatShaderParametersController
{
	static BoatShaderParametersController()
	{
		for (int i = 0; i < 10; i++)
		{
			BoatShaderParametersController._vectorsStr[i] = string.Format("_BoatSettings{0}", i);
			BoatShaderParametersController._texStr[i] = string.Format("_BoatMask{0}", i);
		}
		BoatShaderParametersController._masks = new Dictionary<BoatShaderParametersController.BoatMaskType, Texture2D>();
		BoatShaderParametersController._masks[BoatShaderParametersController.BoatMaskType.Kayak] = Resources.Load<Texture2D>("Boats/Masks/maskKayak");
		BoatShaderParametersController._masks[BoatShaderParametersController.BoatMaskType.Zodiac] = Resources.Load<Texture2D>("Boats/Masks/maskInflatableMotorboat");
		BoatShaderParametersController._masks[BoatShaderParametersController.BoatMaskType.Metal] = Resources.Load<Texture2D>("Boats/Masks/maskMotorboat");
		BoatShaderParametersController._masks[BoatShaderParametersController.BoatMaskType.Bassboat1] = Resources.Load<Texture2D>("Boats/Masks/maskBassboatMK1");
		BoatShaderParametersController._masks[BoatShaderParametersController.BoatMaskType.Bassboat2] = Resources.Load<Texture2D>("Boats/Masks/maskBassboatMK2");
	}

	public static void SetBoatParameters(BoatShaderParametersController.BoatMaskType maskType, FloatingSimplexComposite phyboat)
	{
		if (BoatShaderParametersController._boatIndex < 10)
		{
			byte boatIndex = BoatShaderParametersController._boatIndex;
			BoatShaderParametersController._boatIndex = boatIndex + 1;
			int num = (int)boatIndex;
			Shader.SetGlobalVector(BoatShaderParametersController._vectorsStr[num], new Vector4(phyboat.Position.x, phyboat.Position.z, 0.017453292f * phyboat.Rotation.eulerAngles.y, 8.7f));
			Shader.SetGlobalTexture(BoatShaderParametersController._texStr[num], BoatShaderParametersController._masks[maskType]);
		}
	}

	public static void SetBoatParameters(BoatShaderParametersController.BoatMaskType maskType, Transform boat)
	{
		if (BoatShaderParametersController._boatIndex < 10)
		{
			byte boatIndex = BoatShaderParametersController._boatIndex;
			BoatShaderParametersController._boatIndex = boatIndex + 1;
			int num = (int)boatIndex;
			Shader.SetGlobalVector(BoatShaderParametersController._vectorsStr[num], new Vector4(boat.position.x, boat.position.z, 0.017453292f * boat.rotation.eulerAngles.y, 8.7f));
			Shader.SetGlobalTexture(BoatShaderParametersController._texStr[num], BoatShaderParametersController._masks[maskType]);
		}
	}

	public static void ResetFrame()
	{
		BoatShaderParametersController._boatIndex = 0;
		for (int i = 0; i < 10; i++)
		{
			Shader.SetGlobalVector(BoatShaderParametersController._vectorsStr[i], new Vector4(0f, 0f, 0f, 0f));
		}
	}

	private const byte MaxBoats = 10;

	private const float MaskScale = 8.7f;

	private static string[] _vectorsStr = new string[10];

	private static string[] _texStr = new string[10];

	private static byte _boatIndex = 0;

	private static Dictionary<BoatShaderParametersController.BoatMaskType, Texture2D> _masks;

	public enum BoatMaskType
	{
		Kayak,
		Zodiac,
		Metal,
		Bassboat1,
		Bassboat2
	}
}
