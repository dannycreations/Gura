using System;
using System.Collections;
using System.Collections.Generic;
using EasyRoads3D;
using UnityEngine;

[ExecuteInEditMode]
public class MarkerScript : MonoBehaviour
{
	private void Start()
	{
		IEnumerator enumerator = base.transform.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				this.surface = transform;
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
	}

	private void OnDrawGizmos()
	{
		if (this.objectScript != null)
		{
			if (!this.objectScript.OCDCCDOOOD)
			{
				if (this.snapMarker && ODCCQOCQOD.terrain != null)
				{
					Vector3 vector = base.transform.position;
					vector.y = ODCCQOCQOD.terrain.SampleHeight(vector) + ODCCQOCQOD.terrain.transform.position.y;
					base.transform.position = vector;
				}
				Vector3 vector2 = base.transform.position - this.oldPos;
				if (this.OQOQDOCQOC && this.oldPos != Vector3.zero && vector2 != Vector3.zero)
				{
					int num = 0;
					foreach (Transform transform in this.ODQDOQCCCOs)
					{
						transform.position += vector2 * this.trperc[num];
						if (this.snapMarker && ODCCQOCQOD.terrain != null)
						{
							Vector3 vector = transform.position;
							vector.y = ODCCQOCQOD.terrain.SampleHeight(vector);
							transform.position = vector;
						}
						num++;
					}
				}
				if (this.oldPos != Vector3.zero && vector2 != Vector3.zero)
				{
					this.changed = true;
					if (this.objectScript.OCDCCDOOOD)
					{
						this.objectScript.OQQCDCOCQO.specialRoadMaterial = true;
					}
				}
				this.oldPos = base.transform.position;
			}
			else if (this.objectScript.ODODDDOO)
			{
				base.transform.position = this.oldPos;
			}
		}
	}

	private void SetObjectScript()
	{
		this.objectScript = base.transform.parent.parent.GetComponent<RoadObjectScript>();
		if (this.objectScript.OQQCDCOCQO == null)
		{
			List<ODODDQQO> list = OCOQDQCQDO.OOOCODQOOQ(false);
			this.objectScript.OQDCDCOODC(list, OCOQDQCQDO.ODOCQDDCCD(list), OCOQDQCQDO.ODCOCCOQCQ(list));
		}
	}

	private void GetMarkerCount()
	{
		int num = 0;
		IEnumerator enumerator = base.transform.parent.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				if (transform == base.transform)
				{
					this.markerInt = num;
					break;
				}
				num++;
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
	}

	public void LeftIndent(float change, float perc)
	{
		this.ri += change * perc;
		if (this.ri < this.objectScript.indent)
		{
			this.ri = this.objectScript.indent;
		}
		this.OOQOQQOO = this.ri;
	}

	public void RightIndent(float change, float perc)
	{
		this.li += change * perc;
		if (this.li < this.objectScript.indent)
		{
			this.li = this.objectScript.indent;
		}
		this.ODODQQOO = this.li;
	}

	public void LeftSurrounding(float change, float perc)
	{
		this.rs += change * perc;
		if (this.rs < this.objectScript.indent)
		{
			this.rs = this.objectScript.indent;
		}
		this.ODOQQOOO = this.rs;
	}

	public void RightSurrounding(float change, float perc)
	{
		this.ls += change * perc;
		if (this.ls < this.objectScript.indent)
		{
			this.ls = this.objectScript.indent;
		}
		this.DODOQQOO = this.ls;
	}

	public void LeftTilting(float change, float perc)
	{
		this.rt += change * perc;
		if (this.rt < 0f)
		{
			this.rt = 0f;
		}
		this.ODDQODOO = this.rt;
	}

	public void RightTilting(float change, float perc)
	{
		this.lt += change * perc;
		if (this.lt < 0f)
		{
			this.lt = 0f;
		}
		this.ODDOQOQQ = this.lt;
	}

	public void FloorDepth(float change, float perc)
	{
		this.floorDepth += change * perc;
		if (this.floorDepth > 0f)
		{
			this.floorDepth = 0f;
		}
		this.oldFloorDepth = this.floorDepth;
	}

	public bool InSelected()
	{
		for (int i = 0; i < this.objectScript.ODQDOQCCCOs.Length; i++)
		{
			if (this.objectScript.ODQDOQCCCOs[i] == base.gameObject)
			{
				return true;
			}
		}
		return false;
	}

	public float tension = 0.5f;

	public float ri;

	public float OOQOQQOO;

	public float li;

	public float ODODQQOO;

	public float rs;

	public float ODOQQOOO;

	public float ls;

	public float DODOQQOO;

	public float rt;

	public float qt;

	public float ODDQODOO;

	public float lt;

	public float ODDOQOQQ;

	public bool OQOQDOCQOC;

	public bool ODQDOQOO;

	public float OQCCQDCDOQ;

	public float ODOOQQOO;

	public Transform[] ODQDOQCCCOs;

	public float[] trperc;

	public Vector3 oldPos = Vector3.zero;

	public bool autoUpdate;

	public bool changed;

	public Transform surface;

	public bool ODDCQOCODC;

	private Vector3 position;

	private bool updated;

	private int frameCount;

	private float currentstamp;

	private float newstamp;

	private bool mousedown;

	private Vector3 lookAtPoint;

	public bool bridgeObject;

	public bool distHeights;

	public RoadObjectScript objectScript;

	public List<string> OQODQQDO = new List<string>();

	public List<bool> ODOQQQDO = new List<bool>();

	public List<bool> OQQODQQOO = new List<bool>();

	public List<float> ODDOQQOO = new List<float>();

	public string[] ODDOOQDO;

	public bool[] ODDGDOOO;

	public bool[] ODDQOOO;

	public float[] ODDQOODO;

	public float[] ODOQODOO;

	public float[] ODDOQDO;

	public int markerNum;

	public string distance = "0";

	public string OQCOCOCDCQ = "0";

	public string OQCCOCCDOD = "0";

	public bool newSegment;

	public float floorDepth = 2f;

	public float oldFloorDepth = 2f;

	public float waterLevel = 0.5f;

	public bool lockWaterLevel = true;

	public bool tunnelFlag;

	public bool sharpCorner;

	public bool snapMarker;

	public int markerInt;
}
