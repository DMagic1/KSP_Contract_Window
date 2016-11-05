#region license
/*The MIT License (MIT)
contractLoader - Monobehaviour for loading and process UI prefabs

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

using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ContractsWindow.Unity;
using KSP.UI;

namespace ContractsWindow
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class contractLoader : MonoBehaviour
	{
		private static AssetBundle prefabs;
		private static GameObject[] loadedPrefabs;
		private static GameObject windowPrefab;
		private static Canvas canvasPrefab;
		private static int currentFontAdjustment;

		public static AssetBundle Prefabs
		{
			get { return prefabs; }
		}

		public static GameObject WindowPrefab
		{
			get { return windowPrefab; }
		}

		public static Canvas CanvasPrefab
		{
			get { return canvasPrefab; }
		}

		private void Awake()
		{
			string path = KSPUtil.ApplicationRootPath + "GameData/DMagicUtilities/ContractsWindow/Resources";

			prefabs = AssetBundle.LoadFromFile(path + "/contracts_window_prefabs");

			if (prefabs != null)
				loadedPrefabs = prefabs.LoadAllAssets<GameObject>();

			if (loadedPrefabs != null)
				processPrefabs();
		}

		public static void UpdateTooltips(bool isOn)
		{
			if (loadedPrefabs == null)
				return;

			for (int i = loadedPrefabs.Length - 1; i >= 0; i--)
			{
				GameObject obj = loadedPrefabs[i];

				if (obj == null)
					continue;

				var tooltips = obj.GetComponentsInChildren<TooltipHandler>(true);

				for (int j = tooltips.Length - 1; j >= 0; j--)
				{
					TooltipHandler t = tooltips[j];

					if (t == null)
						continue;

					t.IsActive = isOn;
				}
			}
		}

		public static void UpdateFontSize(int i)
		{
			if (loadedPrefabs == null)
				return;

			if (i == 1)
			{
				if (currentFontAdjustment == 1)
					return;

				currentFontAdjustment = 1;
			}
			else if (i == 0)
			{
				if (currentFontAdjustment == 0)
					return;

				currentFontAdjustment = 0;
			}
			else if (i == -1)
				currentFontAdjustment = 0;

			for (int j = loadedPrefabs.Length - 1; j >= 0; j--)
			{
				GameObject obj = loadedPrefabs[j];

				if (obj == null)
					continue;

				var texts = obj.GetComponentsInChildren<TextHandler>(true);

				for (int k = texts.Length - 1; k >= 0; k--)
				{
					TextHandler t = texts[k];

					if (t == null)
						continue;

					t.OnFontChange.Invoke(i);
				}
			}
		}

		private void processPrefabs()
		{
			for (int i = loadedPrefabs.Length - 1; i >= 0; i--)
			{
				GameObject o = loadedPrefabs[i];

				if (o.name == "CW_Plus")
					windowPrefab = o;
				else if (o.name == "CW_Canvas_Prefab")
				{
					Canvas c = o.GetComponent<Canvas>();

					if (c != null)
						canvasPrefab = c;
				}

				if (o != null)
					processPrefab(o);
			}
		}

		private void processPrefab(GameObject obj)
		{
			if (obj == null)
				return;

			TextHandler[] handlers = obj.GetComponentsInChildren<TextHandler>(true);

			if (handlers == null)
				return;

			for (int i = 0; i < handlers.Length; i++)
				TMProFromText(handlers[i]);
		}

		private void TMProFromText(TextHandler handler)
		{
			if (handler == null)
				return;

			Text text = handler.GetComponent<Text>();

			if (text == null)
				return;

			string t = text.text;
			Color c = text.color;
			int i = text.fontSize;
			bool r = text.raycastTarget;
			FontStyles sty = getStyle(text.fontStyle);
			TextAlignmentOptions align = getAnchor(text.alignment);
			float spacing = text.lineSpacing;
			GameObject obj = text.gameObject;

			MonoBehaviour.DestroyImmediate(text);

			CWTextMeshProHolder tmp = obj.AddComponent<CWTextMeshProHolder>();

			tmp.text = t;
			tmp.color = c;
			tmp.fontSize = i;
			tmp.raycastTarget = r;
			tmp.alignment = align;
			tmp.fontStyle = sty;
			tmp.lineSpacing = spacing;

			tmp.font = Resources.Load("Fonts/Calibri SDF", typeof(TMP_FontAsset)) as TMP_FontAsset;
			tmp.fontSharedMaterial = Resources.Load("Fonts/Materials/Calibri Dropshadow", typeof(Material)) as Material;

			tmp.enableWordWrapping = true;
			tmp.isOverlay = false;
			tmp.richText = true;

			tmp.Setup(handler);
		}

		private FontStyles getStyle(FontStyle style)
		{
			switch (style)
			{
				case FontStyle.Normal:
					return FontStyles.Normal;
				case FontStyle.Bold:
					return FontStyles.Bold;
				case FontStyle.Italic:
					return FontStyles.Italic;
				case FontStyle.BoldAndItalic:
					return FontStyles.Bold;
				default:
					return FontStyles.Normal;
			}
		}

		private TextAlignmentOptions getAnchor(TextAnchor anchor)
		{
			switch (anchor)
			{
				case TextAnchor.UpperLeft:
					return TextAlignmentOptions.TopLeft;
				case TextAnchor.UpperCenter:
					return TextAlignmentOptions.Top;
				case TextAnchor.UpperRight:
					return TextAlignmentOptions.TopRight;
				case TextAnchor.MiddleLeft:
					return TextAlignmentOptions.MidlineLeft;
				case TextAnchor.MiddleCenter:
					return TextAlignmentOptions.Midline;
				case TextAnchor.MiddleRight:
					return TextAlignmentOptions.MidlineRight;
				case TextAnchor.LowerLeft:
					return TextAlignmentOptions.BottomLeft;
				case TextAnchor.LowerCenter:
					return TextAlignmentOptions.Bottom;
				case TextAnchor.LowerRight:
					return TextAlignmentOptions.BottomRight;
				default:
					return TextAlignmentOptions.Center;
			}
		}
	}
}
