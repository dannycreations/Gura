using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EnhancedScrollerDemos.SnappingDemo
{
	public class PlayWin : MonoBehaviour
	{
		private void Awake()
		{
			this._transform = base.transform;
			this._transform.localScale = Vector3.zero;
		}

		public void Play(int score)
		{
			this.scoreText.text = string.Format("{0:n0}", score);
			base.transform.localScale = Vector3.zero;
			this._timeLeft = this.zoomTime;
			base.StartCoroutine(this.PlayZoom());
		}

		private IEnumerator PlayZoom()
		{
			while (this._timeLeft > 0f)
			{
				this._timeLeft -= Time.deltaTime;
				this._transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, (this.zoomTime - this._timeLeft) / this.zoomTime);
				yield return null;
			}
			base.transform.localScale = Vector3.one;
			this._timeLeft = this.holdTime;
			base.StartCoroutine(this.PlayHold());
			yield break;
		}

		private IEnumerator PlayHold()
		{
			while (this._timeLeft > 0f)
			{
				this._timeLeft -= Time.deltaTime;
				yield return null;
			}
			this._timeLeft = this.unZoomTime;
			base.StartCoroutine(this.PlayUnZoom());
			yield break;
		}

		private IEnumerator PlayUnZoom()
		{
			while (this._timeLeft > 0f)
			{
				this._timeLeft -= Time.deltaTime;
				this._transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, (this.unZoomTime - this._timeLeft) / this.unZoomTime);
				yield return null;
			}
			base.transform.localScale = Vector3.zero;
			yield break;
		}

		private Transform _transform;

		private float _timeLeft;

		public Text scoreText;

		public float zoomTime;

		public float holdTime;

		public float unZoomTime;
	}
}
