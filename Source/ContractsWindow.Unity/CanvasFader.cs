#region license
/*The MIT License (MIT)
CanvasFader - Monobehaviour for making smooth fade in and fade out for UI windows

Copyright (c) 2016 DMagic

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#endregion

using System;
using System.Collections;
using UnityEngine;

namespace ContractsWindow.Unity
{
	[RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
	public class CanvasFader : MonoBehaviour
	{
		[SerializeField]
		private float SlowRate = 0.9f;
		[SerializeField]
		private float FastRate = 0.3f;

		private CanvasGroup canvas;
		private IEnumerator fader;
		private bool allowInterrupt = true;

		protected virtual void Awake()
		{
			canvas = GetComponent<CanvasGroup>();
		}

		public bool Fading
		{
			get { return fader != null; }
		}

		protected void Fade(float to, bool fast, Action call = null, bool interrupt = true, bool overrule = false)
		{
			if (canvas == null)
				return;

			Fade(canvas.alpha, to, fast ? FastRate : SlowRate, call, interrupt, overrule);
		}

		protected void Alpha(float to)
		{
			if (canvas == null)
				return;

			to = Mathf.Clamp01(to);
			canvas.alpha = to;
		}

		private void Fade(float from, float to, float duration, Action call, bool interrupt, bool overrule)
		{
			if (!allowInterrupt && !overrule)
				return;

			if (fader != null)
				StopCoroutine(fader);

			fader = FadeRoutine(from, to, duration, call, interrupt);
			StartCoroutine(fader);
		}

		private IEnumerator FadeRoutine(float from, float to, float duration, Action call, bool interrupt)
		{
			allowInterrupt = interrupt;

			yield return new WaitForEndOfFrame();

			float f = 0;

			while (f <= 1)
			{
				f += Time.deltaTime / duration;
				Alpha(Mathf.Lerp(from, to, f));
				yield return null;
			}

			if (call != null)
				call.Invoke();

			allowInterrupt = true;

			fader = null;
		}

	}
}
