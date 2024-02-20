using System;
using UnityEngine;

namespace mset
{
	public class SkyDebug : MonoBehaviour
	{
		private void Start()
		{
			this.debugID = SkyDebug.debugCounter;
			SkyDebug.debugCounter++;
		}

		private void LateUpdate()
		{
			bool flag = this.printOnce || this.printConstantly;
			if (base.GetComponent<Renderer>() && flag)
			{
				this.printOnce = false;
				this.debugString = this.GetDebugString();
				if (this.printToConsole)
				{
					Debug.Log(this.debugString);
				}
			}
		}

		public string GetDebugString()
		{
			string text = "<b>SkyDebug Info - " + base.name + "</b>\n";
			Material material;
			if (Application.isPlaying)
			{
				material = base.GetComponent<Renderer>().material;
			}
			else
			{
				material = base.GetComponent<Renderer>().sharedMaterial;
			}
			text = text + material.shader.name + "\n";
			string text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"is supported: ",
				material.shader.isSupported,
				"\n"
			});
			ShaderIDs[] array = new ShaderIDs[]
			{
				new ShaderIDs(),
				new ShaderIDs()
			};
			array[0].Link();
			array[1].Link("1");
			text += "\n<b>Anchor</b>\n";
			SkyAnchor component = base.GetComponent<SkyAnchor>();
			if (component != null)
			{
				text = text + "Curr. sky: " + component.CurrentSky.name + "\n";
				text = text + "Prev. sky: " + component.PreviousSky.name + "\n";
			}
			else
			{
				text += "none\n";
			}
			text += "\n<b>Property Block</b>\n";
			if (this.block == null)
			{
				this.block = new MaterialPropertyBlock();
			}
			this.block.Clear();
			base.GetComponent<Renderer>().GetPropertyBlock(this.block);
			for (int i = 0; i < 2; i++)
			{
				text = text + "Renderer Property block - blend ID " + i;
				if (this.printDetails)
				{
					text = text + "\nexposureIBL  " + this.block.GetVector(array[i].exposureIBL);
					text = text + "\nexposureLM   " + this.block.GetVector(array[i].exposureLM);
					text = text + "\nskyMin       " + this.block.GetVector(array[i].skyMin);
					text = text + "\nskyMax       " + this.block.GetVector(array[i].skyMax);
					text += "\ndiffuse SH\n";
					for (int j = 0; j < 4; j++)
					{
						text = text + this.block.GetVector(array[i].SH[j]) + "\n";
					}
					text += "...\n";
				}
				Texture texture = this.block.GetTexture(array[i].specCubeIBL);
				Texture texture2 = this.block.GetTexture(array[i].skyCubeIBL);
				text += "\nspecCubeIBL  ";
				if (texture)
				{
					text += texture.name;
				}
				else
				{
					text += "none";
				}
				text += "\nskyCubeIBL   ";
				if (texture2)
				{
					text += texture2.name;
				}
				else
				{
					text += "none";
				}
				if (this.printDetails)
				{
					text = text + "\nskyMatrix\n" + this.block.GetMatrix(array[i].skyMatrix);
					text = text + "\ninvSkyMatrix\n" + this.block.GetMatrix(array[i].invSkyMatrix);
				}
				if (i == 0)
				{
					text = text + "\nblendWeightIBL " + this.block.GetFloat(array[i].blendWeightIBL);
				}
				text += "\n\n";
			}
			return text;
		}

		private void OnDrawGizmosSelected()
		{
			bool flag = this.printOnce || this.printConstantly;
			if (base.GetComponent<Renderer>() && this.printInEditor && this.printToConsole && flag)
			{
				this.printOnce = false;
				string text = this.GetDebugString();
				Debug.Log(text);
			}
		}

		private void OnGUI()
		{
			if (this.printToGUI)
			{
				Rect rect = Rect.MinMaxRect(3f, 3f, 360f, 1024f);
				if (Camera.main)
				{
					rect.yMax = (float)Camera.main.pixelHeight;
				}
				rect.xMin += (float)this.debugID * rect.width;
				GUI.color = Color.white;
				if (this.debugStyle == null)
				{
					this.debugStyle = new GUIStyle();
					this.debugStyle.richText = true;
				}
				string text = "<color=\"#000\">";
				string text2 = "</color>";
				GUI.TextArea(rect, text + this.debugString + text2, this.debugStyle);
				text = "<color=\"#FFF\">";
				rect.xMin -= 1f;
				rect.yMin -= 2f;
				GUI.TextArea(rect, text + this.debugString + text2, this.debugStyle);
			}
		}

		public bool printConstantly = true;

		public bool printOnce;

		public bool printToGUI = true;

		public bool printToConsole;

		public bool printInEditor = true;

		public bool printDetails;

		public string debugString = string.Empty;

		private MaterialPropertyBlock block;

		private GUIStyle debugStyle;

		private static int debugCounter;

		private int debugID;
	}
}
