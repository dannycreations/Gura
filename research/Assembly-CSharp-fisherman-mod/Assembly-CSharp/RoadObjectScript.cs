using System;
using System.Collections;
using System.Collections.Generic;
using EasyRoads3D;
using UnityEngine;

public class RoadObjectScript : MonoBehaviour
{
	public void OQDCDCOODC(List<ODODDQQO> arr, string[] DOODQOQO, string[] OODDQOQO)
	{
		this.OCODQDOCOC(base.transform, arr, DOODQOQO, OODDQOQO);
	}

	public void OCOODDOOQQ(MarkerScript markerScript)
	{
		this.ODQDOQCCCO = markerScript.transform;
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < this.ODQDOQCCCOs.Length; i++)
		{
			if (this.ODQDOQCCCOs[i] != markerScript.gameObject)
			{
				list.Add(this.ODQDOQCCCOs[i]);
			}
		}
		list.Add(markerScript.gameObject);
		this.ODQDOQCCCOs = list.ToArray();
		this.ODQDOQCCCO = markerScript.transform;
		this.OQQCDCOCQO.ODQCDOOQQQ(this.ODQDOQCCCO, this.ODQDOQCCCOs, markerScript.OQOQDOCQOC, markerScript.OQCCQDCDOQ, this.ODOQCQDDCQ, ref markerScript.ODQDOQCCCOs, ref markerScript.trperc, this.ODQDOQCCCOs);
		this.OCQDCCQCOD = -1;
	}

	public void OCOQCOQQDO(MarkerScript markerScript)
	{
		if (markerScript.OQCCQDCDOQ != markerScript.ODOOQQOO || markerScript.OQCCQDCDOQ != markerScript.ODOOQQOO)
		{
			this.OQQCDCOCQO.ODQCDOOQQQ(this.ODQDOQCCCO, this.ODQDOQCCCOs, markerScript.OQOQDOCQOC, markerScript.OQCCQDCDOQ, this.ODOQCQDDCQ, ref markerScript.ODQDOQCCCOs, ref markerScript.trperc, this.ODQDOQCCCOs);
			markerScript.ODQDOQOO = markerScript.OQOQDOCQOC;
			markerScript.ODOOQQOO = markerScript.OQCCQDCDOQ;
		}
		if (this.OOOOOCQOCO.autoUpdate)
		{
			this.ODCQOCDDDC(this.OOOOOCQOCO.geoResolution, false, false);
		}
	}

	public void ResetMaterials(MarkerScript markerScript)
	{
		if (this.OQQCDCOCQO != null)
		{
			this.OQQCDCOCQO.ODQCDOOQQQ(this.ODQDOQCCCO, this.ODQDOQCCCOs, markerScript.OQOQDOCQOC, markerScript.OQCCQDCDOQ, this.ODOQCQDDCQ, ref markerScript.ODQDOQCCCOs, ref markerScript.trperc, this.ODQDOQCCCOs);
		}
	}

	public void OQCQDQCDDQ(MarkerScript markerScript)
	{
		if (markerScript.OQCCQDCDOQ != markerScript.ODOOQQOO)
		{
			this.OQQCDCOCQO.ODQCDOOQQQ(this.ODQDOQCCCO, this.ODQDOQCCCOs, markerScript.OQOQDOCQOC, markerScript.OQCCQDCDOQ, this.ODOQCQDDCQ, ref markerScript.ODQDOQCCCOs, ref markerScript.trperc, this.ODQDOQCCCOs);
			markerScript.ODOOQQOO = markerScript.OQCCQDCDOQ;
		}
		this.ODCQOCDDDC(this.OOOOOCQOCO.geoResolution, false, false);
	}

	private void OOOCDDDQQO(string ctrl, MarkerScript markerScript)
	{
		int num = 0;
		foreach (Transform transform in markerScript.ODQDOQCCCOs)
		{
			MarkerScript component = transform.GetComponent<MarkerScript>();
			if (ctrl == "rs")
			{
				component.LeftSurrounding(markerScript.rs - markerScript.ODOQQOOO, markerScript.trperc[num]);
			}
			else if (ctrl == "ls")
			{
				component.RightSurrounding(markerScript.ls - markerScript.DODOQQOO, markerScript.trperc[num]);
			}
			else if (ctrl == "ri")
			{
				component.LeftIndent(markerScript.ri - markerScript.OOQOQQOO, markerScript.trperc[num]);
			}
			else if (ctrl == "li")
			{
				component.RightIndent(markerScript.li - markerScript.ODODQQOO, markerScript.trperc[num]);
			}
			else if (ctrl == "rt")
			{
				component.LeftTilting(markerScript.rt - markerScript.ODDQODOO, markerScript.trperc[num]);
			}
			else if (ctrl == "lt")
			{
				component.RightTilting(markerScript.lt - markerScript.ODDOQOQQ, markerScript.trperc[num]);
			}
			else if (ctrl == "floorDepth")
			{
				component.FloorDepth(markerScript.floorDepth - markerScript.oldFloorDepth, markerScript.trperc[num]);
			}
			num++;
		}
	}

	public void OQCCDQDCDQ()
	{
		if (this.markers > 1)
		{
			this.ODCQOCDDDC(this.OOOOOCQOCO.geoResolution, false, false);
		}
	}

	public void OCODQDOCOC(Transform tr, List<ODODDQQO> arr, string[] DOODQOQO, string[] OODDQOQO)
	{
		RoadObjectScript.version = "2.5.6";
		RoadObjectScript.ODCOOCDQQC = (GUISkin)Resources.Load("ER3DSkin", typeof(GUISkin));
		RoadObjectScript.ODCCDDCCDO = (Texture2D)Resources.Load("ER3DLogo", typeof(Texture2D));
		if (RoadObjectScript.objectStrings == null)
		{
			RoadObjectScript.objectStrings = new string[3];
			RoadObjectScript.objectStrings[0] = "Road Object";
			RoadObjectScript.objectStrings[1] = "River Object";
			RoadObjectScript.objectStrings[2] = "Procedural Mesh Object";
		}
		this.obj = tr;
		this.OQQCDCOCQO = new OOCOOCQQDQ();
		this.OOOOOCQOCO = this.obj.GetComponent<RoadObjectScript>();
		IEnumerator enumerator = this.obj.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				if (transform.name == "Markers")
				{
					this.ODOQCQDDCQ = transform;
				}
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}
		RoadObjectScript[] array = (RoadObjectScript[])Object.FindObjectsOfType(typeof(RoadObjectScript));
		OOCOOCQQDQ.terrainList.Clear();
		Terrain[] array2 = (Terrain[])Object.FindObjectsOfType(typeof(Terrain));
		foreach (Terrain terrain in array2)
		{
			Terrains terrains = new Terrains();
			terrains.terrain = terrain;
			if (!terrain.gameObject.GetComponent<EasyRoads3DTerrainID>())
			{
				EasyRoads3DTerrainID easyRoads3DTerrainID = terrain.gameObject.AddComponent<EasyRoads3DTerrainID>();
				string text = Random.Range(100000000, 999999999).ToString();
				easyRoads3DTerrainID.terrainid = text;
				terrains.id = text;
			}
			else
			{
				terrains.id = terrain.gameObject.GetComponent<EasyRoads3DTerrainID>().terrainid;
			}
			OOCOOCQQDQ.OODOQDQODQ(terrains);
		}
		ODCCQOCQOD.OODOQDQODQ();
		if (this.roadMaterialEdit == null)
		{
			this.roadMaterialEdit = (Material)Resources.Load("materials/roadMaterialEdit", typeof(Material));
		}
		if (this.objectType == 0 && GameObject.Find(base.gameObject.name + "/road") == null)
		{
			GameObject gameObject = new GameObject("road");
			gameObject.transform.parent = base.transform;
		}
		this.OQQCDCOCQO.OOCOCQOOQC(this.obj, RoadObjectScript.ODOCCCQQDD, this.OOOOOCQOCO.roadWidth, this.surfaceOpacity, ref this.ODDCQOCODC, ref this.indent, this.applyAnimation, this.waveSize, this.waveHeight);
		this.OQQCDCOCQO.ODCCQCDCDQ = this.ODCCQCDCDQ;
		this.OQQCDCOCQO.ODCQCQQQCO = this.ODCQCQQQCO;
		this.OQQCDCOCQO.OdQODQOD = this.OdQODQOD + 1;
		this.OQQCDCOCQO.OOQQQDOD = this.OOQQQDOD;
		this.OQQCDCOCQO.OOQQQDODOffset = this.OOQQQDODOffset;
		this.OQQCDCOCQO.OOQQQDODLength = this.OOQQQDODLength;
		this.OQQCDCOCQO.objectType = this.objectType;
		this.OQQCDCOCQO.snapY = this.snapY;
		this.OQQCDCOCQO.terrainRendered = this.OCDCCDOOOD;
		this.OQQCDCOCQO.handleVegetation = this.handleVegetation;
		this.OQQCDCOCQO.raise = this.raise;
		this.OQQCDCOCQO.roadResolution = this.roadResolution;
		this.OQQCDCOCQO.multipleTerrains = this.multipleTerrains;
		this.OQQCDCOCQO.editRestore = this.editRestore;
		this.OQQCDCOCQO.roadMaterialEdit = this.roadMaterialEdit;
		this.OQQCDCOCQO.renderRoad = this.renderRoad;
		this.OQQCDCOCQO.rscrpts = array.Length;
		this.OQQCDCOCQO.blendFlag = this.blendFlag;
		this.OQQCDCOCQO.startBlendDistance = this.startBlendDistance;
		this.OQQCDCOCQO.endBlendDistance = this.endBlendDistance;
		this.OQQCDCOCQO.iOS = this.iOS;
		if (RoadObjectScript.backupLocation == 0)
		{
			OCQQOCDCCD.backupFolder = "/EasyRoads3D";
		}
		else
		{
			OCQQOCDCCD.backupFolder = OCQQOCDCCD.extensionPath + "/Backups";
		}
		this.ODODQOQO = this.OQQCDCOCQO.OOOCCQCQOO();
		this.ODODQOQOInt = this.OQQCDCOCQO.OOOODCDDOQ();
		if (this.OCDCCDOOOD)
		{
			this.doRestore = true;
		}
		this.OOCOOQCQQD();
		if (arr != null || RoadObjectScript.ODODQOOQ == null)
		{
			this.OQQODDQCQD(arr, DOODQOQO, OODDQOQO);
		}
		if (this.doRestore)
		{
			return;
		}
	}

	public void UpdateBackupFolder()
	{
	}

	public void ODCCDOOOQO()
	{
		if ((!this.ODODDDOO || this.objectType == 2) && this.OCCDQODQCD != null)
		{
			for (int i = 0; i < this.OCCDQODQCD.Length; i++)
			{
				this.OCCDQODQCD[i] = false;
				this.OQQCQQDOCQ[i] = false;
			}
		}
	}

	public void OCOCDDCDDO(Vector3 pos)
	{
		if (!this.displayRoad)
		{
			this.displayRoad = true;
			this.OQQCDCOCQO.OQOOCOOCQC(this.displayRoad, this.ODOQCQDDCQ);
		}
		pos.y += this.OOOOOCQOCO.raiseMarkers;
		if (this.forceY && this.ODOQDQOO != null)
		{
			float num = Vector3.Distance(pos, this.ODOQDQOO.transform.position);
			pos.y = this.ODOQDQOO.transform.position.y + this.yChange * (num / 100f);
		}
		else if (this.forceY && this.markers == 0)
		{
			this.lastY = pos.y;
		}
		GameObject gameObject;
		if (this.ODOQDQOO != null)
		{
			gameObject = Object.Instantiate<GameObject>(this.ODOQDQOO);
		}
		else
		{
			gameObject = (GameObject)Object.Instantiate(Resources.Load("marker", typeof(GameObject)));
		}
		Transform transform = gameObject.transform;
		transform.position = pos;
		transform.parent = this.ODOQCQDDCQ;
		this.markers++;
		string text;
		if (this.markers < 10)
		{
			text = "Marker000" + this.markers.ToString();
		}
		else if (this.markers < 100)
		{
			text = "Marker00" + this.markers.ToString();
		}
		else
		{
			text = "Marker0" + this.markers.ToString();
		}
		transform.gameObject.name = text;
		MarkerScript component = transform.GetComponent<MarkerScript>();
		component.ODDCQOCODC = false;
		component.objectScript = this.obj.GetComponent<RoadObjectScript>();
		if (this.ODOQDQOO == null)
		{
			component.waterLevel = this.OOOOOCQOCO.waterLevel;
			component.floorDepth = this.OOOOOCQOCO.floorDepth;
			component.ri = this.OOOOOCQOCO.indent;
			component.li = this.OOOOOCQOCO.indent;
			component.rs = this.OOOOOCQOCO.surrounding;
			component.ls = this.OOOOOCQOCO.surrounding;
			component.tension = 0.5f;
			if (this.objectType == 1)
			{
				pos.y -= this.waterLevel;
				transform.position = pos;
			}
		}
		if (this.objectType == 2 && component.surface != null)
		{
			component.surface.gameObject.SetActive(false);
		}
		this.ODOQDQOO = transform.gameObject;
		if (this.markers > 1)
		{
			this.ODCQOCDDDC(this.OOOOOCQOCO.geoResolution, false, false);
			if (this.materialType == 0)
			{
				this.OQQCDCOCQO.OCDDQQCCCO(this.materialType);
			}
		}
	}

	public void ODCQOCDDDC(float geo, bool renderMode, bool camMode)
	{
		this.OQQCDCOCQO.ODDODQOCQO.Clear();
		int num = 0;
		IEnumerator enumerator = this.obj.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				if (transform.name == "Markers")
				{
					IEnumerator enumerator2 = transform.GetEnumerator();
					try
					{
						while (enumerator2.MoveNext())
						{
							object obj2 = enumerator2.Current;
							Transform transform2 = (Transform)obj2;
							MarkerScript component = transform2.GetComponent<MarkerScript>();
							component.objectScript = this.obj.GetComponent<RoadObjectScript>();
							if (!component.ODDCQOCODC)
							{
								component.ODDCQOCODC = this.OQQCDCOCQO.OOCDQOCOQQ(transform2);
							}
							OCQODDOCQD ocqoddocqd = new OCQODDOCQD();
							ocqoddocqd.position = transform2.position;
							ocqoddocqd.num = this.OQQCDCOCQO.ODDODQOCQO.Count;
							ocqoddocqd.object1 = transform2;
							ocqoddocqd.object2 = component.surface;
							ocqoddocqd.tension = component.tension;
							ocqoddocqd.ri = component.ri;
							if (ocqoddocqd.ri < 1f)
							{
								ocqoddocqd.ri = 1f;
							}
							ocqoddocqd.li = component.li;
							if (ocqoddocqd.li < 1f)
							{
								ocqoddocqd.li = 1f;
							}
							ocqoddocqd.rt = component.rt;
							ocqoddocqd.lt = component.lt;
							ocqoddocqd.rs = component.rs;
							if (ocqoddocqd.rs < 1f)
							{
								ocqoddocqd.rs = 1f;
							}
							ocqoddocqd.ODQQQQQOQC = component.rs;
							ocqoddocqd.ls = component.ls;
							if (ocqoddocqd.ls < 1f)
							{
								ocqoddocqd.ls = 1f;
							}
							ocqoddocqd.OOQDCCOOOO = component.ls;
							ocqoddocqd.renderFlag = component.bridgeObject;
							ocqoddocqd.OOCOODCCQC = component.distHeights;
							ocqoddocqd.newSegment = component.newSegment;
							ocqoddocqd.tunnelFlag = component.tunnelFlag;
							ocqoddocqd.floorDepth = component.floorDepth;
							ocqoddocqd.waterLevel = this.waterLevel;
							ocqoddocqd.lockWaterLevel = component.lockWaterLevel;
							ocqoddocqd.sharpCorner = component.sharpCorner;
							ocqoddocqd.OCDOCOQOCD = this.OQQCDCOCQO;
							component.markerNum = num;
							component.distance = "-1";
							component.OQCCOCCDOD = "-1";
							this.OQQCDCOCQO.ODDODQOCQO.Add(ocqoddocqd);
							num++;
						}
					}
					finally
					{
						IDisposable disposable;
						if ((disposable = enumerator2 as IDisposable) != null)
						{
							disposable.Dispose();
						}
					}
				}
			}
		}
		finally
		{
			IDisposable disposable2;
			if ((disposable2 = enumerator as IDisposable) != null)
			{
				disposable2.Dispose();
			}
		}
		this.distance = "-1";
		this.OQQCDCOCQO.ODDQDCOCQC = this.OOOOOCQOCO.roadWidth;
		this.OQQCDCOCQO.OCOOQCCQCQ(geo, this.obj, this.OOOOOCQOCO.OOQDOOQQ, renderMode, camMode, this.objectType);
		if (this.OQQCDCOCQO.leftVecs.Count > 0)
		{
			this.leftVecs = this.OQQCDCOCQO.leftVecs.ToArray();
			this.rightVecs = this.OQQCDCOCQO.rightVecs.ToArray();
		}
	}

	public void StartCam()
	{
		this.ODCQOCDDDC(0.5f, false, true);
	}

	public void OOCOOQCQQD()
	{
		int num = 0;
		IEnumerator enumerator = this.obj.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				if (transform.name == "Markers")
				{
					num = 1;
					IEnumerator enumerator2 = transform.GetEnumerator();
					try
					{
						while (enumerator2.MoveNext())
						{
							object obj2 = enumerator2.Current;
							Transform transform2 = (Transform)obj2;
							string text;
							if (num < 10)
							{
								text = "Marker000" + num.ToString();
							}
							else if (num < 100)
							{
								text = "Marker00" + num.ToString();
							}
							else
							{
								text = "Marker0" + num.ToString();
							}
							transform2.name = text;
							this.ODOQDQOO = transform2.gameObject;
							num++;
						}
					}
					finally
					{
						IDisposable disposable;
						if ((disposable = enumerator2 as IDisposable) != null)
						{
							disposable.Dispose();
						}
					}
				}
			}
		}
		finally
		{
			IDisposable disposable2;
			if ((disposable2 = enumerator as IDisposable) != null)
			{
				disposable2.Dispose();
			}
		}
		this.markers = num - 1;
		this.ODCQOCDDDC(this.OOOOOCQOCO.geoResolution, false, false);
	}

	public List<Transform> RebuildObjs()
	{
		RoadObjectScript[] array = (RoadObjectScript[])Object.FindObjectsOfType(typeof(RoadObjectScript));
		List<Transform> list = new List<Transform>();
		foreach (RoadObjectScript roadObjectScript in array)
		{
			if (roadObjectScript.transform != base.transform)
			{
				list.Add(roadObjectScript.transform);
			}
		}
		return list;
	}

	public void RestoreTerrain1()
	{
		this.ODCQOCDDDC(this.OOOOOCQOCO.geoResolution, false, false);
		if (this.OQQCDCOCQO != null)
		{
			this.OQQCDCOCQO.OCODCCCOCD();
		}
		this.ODODDDOO = false;
	}

	public void ODCOQQDDDQ()
	{
		this.OQQCDCOCQO.ODCOQQDDDQ(this.OOOOOCQOCO.applySplatmap, this.OOOOOCQOCO.splatmapSmoothLevel, this.OOOOOCQOCO.renderRoad, this.OOOOOCQOCO.tuw, this.OOOOOCQOCO.roadResolution, this.OOOOOCQOCO.raise, this.OOOOOCQOCO.opacity, this.OOOOOCQOCO.expand, this.OOOOOCQOCO.offsetX, this.OOOOOCQOCO.offsetY, this.OOOOOCQOCO.beveledRoad, this.OOOOOCQOCO.splatmapLayer, this.OOOOOCQOCO.OdQODQOD, this.OOQQQDOD, this.OOQQQDODOffset, this.OOQQQDODLength);
	}

	public void OQDDODOCDO()
	{
		this.OQQCDCOCQO.OQDDODOCDO(this.OOOOOCQOCO.renderRoad, this.OOOOOCQOCO.tuw, this.OOOOOCQOCO.roadResolution, this.OOOOOCQOCO.raise, this.OOOOOCQOCO.beveledRoad, this.OOOOOCQOCO.OdQODQOD, this.OOQQQDOD, this.OOQQQDODOffset, this.OOQQQDODLength);
	}

	public void OCQQDCOCCQ(Vector3 pos, bool doInsert)
	{
		if (!this.displayRoad)
		{
			this.displayRoad = true;
			this.OQQCDCOCQO.OQOOCOOCQC(this.displayRoad, this.ODOQCQDDCQ);
		}
		int num = -1;
		int num2 = -1;
		float num3 = 10000f;
		float num4 = 10000f;
		Vector3 vector = pos;
		OCQODDOCQD ocqoddocqd = this.OQQCDCOCQO.ODDODQOCQO[0];
		OCQODDOCQD ocqoddocqd2 = this.OQQCDCOCQO.ODDODQOCQO[1];
		if (doInsert)
		{
			Debug.Log("Start Insert" + doInsert);
		}
		this.OQQCDCOCQO.ODOOQDCDQQ(pos, ref num, ref num2, ref num3, ref num4, ref ocqoddocqd, ref ocqoddocqd2, ref vector, doInsert);
		if (doInsert)
		{
			Debug.Log("marker 1: " + num);
			Debug.Log("marker 2: " + num2);
		}
		pos = vector;
		if (doInsert && num >= 0 && num2 >= 0)
		{
			if (this.OOOOOCQOCO.OOQDOOQQ && num2 == this.OQQCDCOCQO.ODDODQOCQO.Count - 1)
			{
				this.OCOCDDCDDO(pos);
			}
			else
			{
				OCQODDOCQD ocqoddocqd3 = this.OQQCDCOCQO.ODDODQOCQO[num2];
				string name = ocqoddocqd3.object1.name;
				int num5 = num2 + 2;
				for (int i = num2; i < this.OQQCDCOCQO.ODDODQOCQO.Count - 1; i++)
				{
					ocqoddocqd3 = this.OQQCDCOCQO.ODDODQOCQO[i];
					string text;
					if (num5 < 10)
					{
						text = "Marker000" + num5.ToString();
					}
					else if (num5 < 100)
					{
						text = "Marker00" + num5.ToString();
					}
					else
					{
						text = "Marker0" + num5.ToString();
					}
					ocqoddocqd3.object1.name = text;
					num5++;
				}
				ocqoddocqd3 = this.OQQCDCOCQO.ODDODQOCQO[num];
				Transform transform = Object.Instantiate<Transform>(ocqoddocqd3.object1.transform, pos, ocqoddocqd3.object1.rotation);
				transform.gameObject.name = name;
				transform.parent = this.ODOQCQDDCQ;
				MarkerScript component = transform.GetComponent<MarkerScript>();
				component.ODDCQOCODC = false;
				float num6 = num3 + num4;
				float num7 = num3 / num6;
				float num8 = ocqoddocqd.ri - ocqoddocqd2.ri;
				component.ri = ocqoddocqd.ri - num8 * num7;
				num8 = ocqoddocqd.li - ocqoddocqd2.li;
				component.li = ocqoddocqd.li - num8 * num7;
				num8 = ocqoddocqd.rt - ocqoddocqd2.rt;
				component.rt = ocqoddocqd.rt - num8 * num7;
				num8 = ocqoddocqd.lt - ocqoddocqd2.lt;
				component.lt = ocqoddocqd.lt - num8 * num7;
				num8 = ocqoddocqd.rs - ocqoddocqd2.rs;
				component.rs = ocqoddocqd.rs - num8 * num7;
				num8 = ocqoddocqd.ls - ocqoddocqd2.ls;
				component.ls = ocqoddocqd.ls - num8 * num7;
				this.ODCQOCDDDC(this.OOOOOCQOCO.geoResolution, false, false);
				if (this.materialType == 0)
				{
					this.OQQCDCOCQO.OCDDQQCCCO(this.materialType);
				}
				if (this.objectType == 2)
				{
					component.surface.gameObject.SetActive(false);
				}
			}
		}
		this.OOCOOQCQQD();
	}

	public void OCQQCODCCC()
	{
		Object.DestroyImmediate(this.OOOOOCQOCO.ODQDOQCCCO.gameObject);
		this.ODQDOQCCCO = null;
		this.OOCOOQCQQD();
	}

	public void OQOQCQODQQ()
	{
		this.OQQCDCOCQO.OCCDCODDCC(12);
	}

	public List<SideObjectParams> OQOOQQDCQC()
	{
		List<SideObjectParams> list = new List<SideObjectParams>();
		IEnumerator enumerator = this.obj.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				if (transform.name == "Markers")
				{
					IEnumerator enumerator2 = transform.GetEnumerator();
					try
					{
						while (enumerator2.MoveNext())
						{
							object obj2 = enumerator2.Current;
							Transform transform2 = (Transform)obj2;
							MarkerScript component = transform2.GetComponent<MarkerScript>();
							list.Add(new SideObjectParams
							{
								ODDGDOOO = component.ODDGDOOO,
								ODDQOODO = component.ODDQOODO,
								ODDQOOO = component.ODDQOOO
							});
						}
					}
					finally
					{
						IDisposable disposable;
						if ((disposable = enumerator2 as IDisposable) != null)
						{
							disposable.Dispose();
						}
					}
				}
			}
		}
		finally
		{
			IDisposable disposable2;
			if ((disposable2 = enumerator as IDisposable) != null)
			{
				disposable2.Dispose();
			}
		}
		return list;
	}

	public void OOQQCOCCCQ()
	{
		List<string> list = new List<string>();
		List<int> list2 = new List<int>();
		List<string> list3 = new List<string>();
		for (int i = 0; i < RoadObjectScript.ODODOQQO.Length; i++)
		{
			if (this.ODODQQOD[i])
			{
				list.Add(RoadObjectScript.ODODQOOQ[i]);
				list3.Add(RoadObjectScript.ODODOQQO[i]);
				list2.Add(i);
			}
		}
		this.ODODDQOO = list.ToArray();
		this.OOQQQOQO = list2.ToArray();
	}

	public void OQQODDQCQD(List<ODODDQQO> arr, string[] DOODQOQO, string[] OODDQOQO)
	{
		bool flag = false;
		RoadObjectScript.ODODOQQO = DOODQOQO;
		RoadObjectScript.ODODQOOQ = OODDQOQO;
		List<MarkerScript> list = new List<MarkerScript>();
		if (this.obj == null)
		{
			this.OCODQDOCOC(base.transform, null, null, null);
		}
		IEnumerator enumerator = this.obj.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				if (transform.name == "Markers")
				{
					IEnumerator enumerator2 = transform.GetEnumerator();
					try
					{
						while (enumerator2.MoveNext())
						{
							object obj2 = enumerator2.Current;
							Transform transform2 = (Transform)obj2;
							MarkerScript component = transform2.GetComponent<MarkerScript>();
							component.OQODQQDO.Clear();
							component.ODOQQQDO.Clear();
							component.OQQODQQOO.Clear();
							component.ODDOQQOO.Clear();
							list.Add(component);
						}
					}
					finally
					{
						IDisposable disposable;
						if ((disposable = enumerator2 as IDisposable) != null)
						{
							disposable.Dispose();
						}
					}
				}
			}
		}
		finally
		{
			IDisposable disposable2;
			if ((disposable2 = enumerator as IDisposable) != null)
			{
				disposable2.Dispose();
			}
		}
		this.mSc = list.ToArray();
		List<bool> list2 = new List<bool>();
		int num = 0;
		int num2 = 0;
		if (this.ODQQQQQO != null)
		{
			if (arr.Count == 0)
			{
				return;
			}
			for (int i = 0; i < RoadObjectScript.ODODOQQO.Length; i++)
			{
				ODODDQQO ododdqqo = arr[i];
				for (int j = 0; j < this.ODQQQQQO.Length; j++)
				{
					if (RoadObjectScript.ODODOQQO[i] == this.ODQQQQQO[j])
					{
						num++;
						if (this.ODODQQOD.Length > j)
						{
							list2.Add(this.ODODQQOD[j]);
						}
						else
						{
							list2.Add(false);
						}
						foreach (MarkerScript markerScript in this.mSc)
						{
							int num3 = -1;
							for (int l = 0; l < markerScript.ODDOOQDO.Length; l++)
							{
								if (ododdqqo.id == markerScript.ODDOOQDO[l])
								{
									num3 = l;
									break;
								}
							}
							if (num3 >= 0)
							{
								markerScript.OQODQQDO.Add(markerScript.ODDOOQDO[num3]);
								markerScript.ODOQQQDO.Add(markerScript.ODDGDOOO[num3]);
								markerScript.OQQODQQOO.Add(markerScript.ODDQOOO[num3]);
								if (ododdqqo.sidewaysDistanceUpdate == 0 || (ododdqqo.sidewaysDistanceUpdate == 2 && markerScript.ODDQOODO[num3] != ododdqqo.oldSidwaysDistance))
								{
									markerScript.ODDOQQOO.Add(markerScript.ODDQOODO[num3]);
								}
								else
								{
									markerScript.ODDOQQOO.Add(ododdqqo.splinePosition);
								}
							}
							else
							{
								markerScript.OQODQQDO.Add(ododdqqo.id);
								markerScript.ODOQQQDO.Add(ododdqqo.markerActive);
								markerScript.OQQODQQOO.Add(true);
								markerScript.ODDOQQOO.Add(ododdqqo.splinePosition);
							}
						}
					}
				}
				if (ododdqqo.sidewaysDistanceUpdate != 0)
				{
				}
				flag = false;
			}
		}
		for (int m = 0; m < RoadObjectScript.ODODOQQO.Length; m++)
		{
			ODODDQQO ododdqqo2 = arr[m];
			bool flag2 = false;
			for (int n = 0; n < this.ODQQQQQO.Length; n++)
			{
				if (RoadObjectScript.ODODOQQO[m] == this.ODQQQQQO[n])
				{
					flag2 = true;
				}
			}
			if (!flag2)
			{
				num2++;
				list2.Add(false);
				foreach (MarkerScript markerScript2 in this.mSc)
				{
					markerScript2.OQODQQDO.Add(ododdqqo2.id);
					markerScript2.ODOQQQDO.Add(ododdqqo2.markerActive);
					markerScript2.OQQODQQOO.Add(true);
					markerScript2.ODDOQQOO.Add(ododdqqo2.splinePosition);
				}
			}
		}
		this.ODODQQOD = list2.ToArray();
		this.ODQQQQQO = new string[RoadObjectScript.ODODOQQO.Length];
		RoadObjectScript.ODODOQQO.CopyTo(this.ODQQQQQO, 0);
		List<int> list3 = new List<int>();
		for (int num5 = 0; num5 < this.ODODQQOD.Length; num5++)
		{
			if (this.ODODQQOD[num5])
			{
				list3.Add(num5);
			}
		}
		this.OOQQQOQO = list3.ToArray();
		foreach (MarkerScript markerScript3 in this.mSc)
		{
			markerScript3.ODDOOQDO = markerScript3.OQODQQDO.ToArray();
			markerScript3.ODDGDOOO = markerScript3.ODOQQQDO.ToArray();
			markerScript3.ODDQOOO = markerScript3.OQQODQQOO.ToArray();
			markerScript3.ODDQOODO = markerScript3.ODDOQQOO.ToArray();
		}
		if (flag)
		{
		}
	}

	public void SetMultipleTerrains(bool flag)
	{
		RoadObjectScript[] array = (RoadObjectScript[])Object.FindObjectsOfType(typeof(RoadObjectScript));
		foreach (RoadObjectScript roadObjectScript in array)
		{
			roadObjectScript.multipleTerrains = flag;
			if (roadObjectScript.OQQCDCOCQO != null)
			{
				roadObjectScript.OQQCDCOCQO.multipleTerrains = flag;
			}
		}
	}

	public bool CheckWaterHeights()
	{
		if (ODCCQOCQOD.terrain == null)
		{
			return false;
		}
		bool flag = true;
		float y = ODCCQOCQOD.terrain.transform.position.y;
		IEnumerator enumerator = this.obj.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				if (transform.name == "Markers")
				{
					IEnumerator enumerator2 = transform.GetEnumerator();
					try
					{
						while (enumerator2.MoveNext())
						{
							object obj2 = enumerator2.Current;
							Transform transform2 = (Transform)obj2;
							if (transform2.position.y - y <= 0.1f)
							{
								flag = false;
							}
						}
					}
					finally
					{
						IDisposable disposable;
						if ((disposable = enumerator2 as IDisposable) != null)
						{
							disposable.Dispose();
						}
					}
				}
			}
		}
		finally
		{
			IDisposable disposable2;
			if ((disposable2 = enumerator as IDisposable) != null)
			{
				disposable2.Dispose();
			}
		}
		return flag;
	}

	public static string version = string.Empty;

	public int objectType;

	public bool displayRoad = true;

	public float roadWidth = 5f;

	public float indent = 3f;

	public float surrounding = 5f;

	public float raise = 1f;

	public float raiseMarkers = 0.5f;

	public bool OOQDOOQQ;

	public bool renderRoad = true;

	public bool beveledRoad;

	public bool applySplatmap;

	public int splatmapLayer = 4;

	public bool autoUpdate = true;

	public float geoResolution = 5f;

	public int roadResolution = 1;

	public float tuw = 15f;

	public int splatmapSmoothLevel;

	public float opacity = 1f;

	public int expand;

	public int offsetX;

	public int offsetY;

	private Material surfaceMaterial;

	public float surfaceOpacity = 1f;

	public float smoothDistance = 1f;

	public float smoothSurDistance = 3f;

	private bool handleInsertFlag;

	public bool handleVegetation = true;

	public float ODCQCQQQCO = 2f;

	public float ODCCQCDCDQ = 1f;

	public int materialType;

	private string[] materialStrings;

	public string uname;

	public string email;

	private MarkerScript[] mSc;

	private bool ODQDOQCDQC;

	private bool[] OCCDQODQCD;

	private bool[] OQQCQQDOCQ;

	public string[] OQDCCDQDOD;

	public string[] ODODQOQO;

	public int[] ODODQOQOInt;

	public int ODCDCCOODQ = -1;

	public int OCQDCCQCOD = -1;

	public static GUISkin ODCOOCDQQC;

	public static GUISkin OCOCCCDQCO;

	public bool OODODOCCCC;

	private Vector3 cPos;

	private Vector3 ePos;

	public bool ODDCQOCODC;

	public static Texture2D ODCCDDCCDO;

	public int markers = 1;

	public OOCOOCQQDQ OQQCDCOCQO;

	private GameObject ODOQDQOO;

	public bool OCDCCDOOOD;

	public bool doTerrain;

	private Transform ODQDOQCCCO;

	public GameObject[] ODQDOQCCCOs;

	private static string ODOCCCQQDD;

	public Transform obj;

	private string ODDODDOQDO;

	public static string erInit = string.Empty;

	public static Transform OQQDCQCDCQ;

	private RoadObjectScript OOOOOCQOCO;

	public bool flyby;

	private Vector3 pos;

	private float fl;

	private float oldfl;

	private bool OQDOOQCOCO;

	private bool ODCCOOCOQD;

	private bool OCCQDDOQDQ;

	public Transform ODOQCQDDCQ;

	public int OdQODQOD = 1;

	public float OOQQQDOD;

	public float OOQQQDODOffset;

	public float OOQQQDODLength;

	public bool ODODDDOO;

	public static string[] ODOQDOQO;

	public static string[] ODODOQQO;

	public static string[] ODODQOOQ;

	public int ODQDOOQO;

	public string[] ODQQQQQO;

	public string[] ODODDQOO;

	public bool[] ODODQQOD;

	public int[] OOQQQOQO;

	public int ODOQOOQO;

	public bool forceY;

	public float yChange;

	public float floorDepth = 2f;

	public float waterLevel = 1.5f;

	public bool lockWaterLevel = true;

	public float lastY;

	public string distance = "0";

	public string markerDisplayStr = "Hide Markers";

	public static string[] objectStrings;

	public string objectText = "Road";

	public bool applyAnimation;

	public float waveSize = 1.5f;

	public float waveHeight = 0.15f;

	public bool snapY = true;

	private TextAnchor origAnchor;

	public bool autoODODDQQO;

	public Texture2D roadTexture;

	public Texture2D roadMaterial;

	public string[] ODCQOQQDCQ;

	public string[] ODCDODQODC;

	public int selectedWaterMaterial;

	public int selectedWaterScript;

	private bool doRestore;

	public bool doFlyOver;

	public static GameObject tracer;

	public Camera goCam;

	public float speed = 1f;

	public float offset;

	public bool camInit;

	public GameObject customMesh;

	public static bool disableFreeAlerts = true;

	public bool multipleTerrains;

	public bool editRestore = true;

	public Material roadMaterialEdit;

	public static int backupLocation;

	public string[] backupStrings = new string[] { "Outside Assets folder path", "Inside Assets folder path" };

	public Vector3[] leftVecs = new Vector3[0];

	public Vector3[] rightVecs = new Vector3[0];

	public bool applyTangents;

	public bool sosBuild;

	public float splinePos;

	public float camHeight = 3f;

	public Vector3 splinePosV3 = Vector3.zero;

	public bool blendFlag;

	public float startBlendDistance = 5f;

	public float endBlendDistance = 5f;

	public bool iOS;

	public static string extensionPath = string.Empty;
}
