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
using ContractsWindow.Unity.Unity;
using KSP.UI;
using KSP.Localization;

namespace ContractsWindow
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class contractLoader : MonoBehaviour
	{
		private const string prefabAssetName = "/contracts_window_prefabs";
		private const string unitySkinAssetName = "/unity_skin";

		private static AssetBundle prefabs;
		private static GameObject[] loadedPrefabs;
		private static GameObject _windowPrefab;
		private static GameObject _tooltipPrefab;
		private static Canvas canvasPrefab;
		private static int currentFontAdjustment;
		private static string path;

		private static bool loaded;
		private static bool spritesLoaded;
		private static bool clearLoaded;
		private static bool toggleLoaded;
		private static bool skinLoaded;
		private static bool prefabsLoaded;
		private static bool tmpProcessed;
		private static bool tooltipsProcessed;
		private static bool prefabsProcessed;

		private static UISkinDef _unitySkinDef;

		private static Sprite _kspTooltipBackground;
		private static Sprite _unityTooltipBackground;

		private static Sprite _clearSprite;
		private static Sprite _toggleNormal;
		private static Sprite _toggleHover;
		private static Sprite _toggleActive;
		private static Sprite _toggleOn;

		private static Texture2D toolbarIcon;

		private static contractSettings settings;

		public static Texture2D ToolbarIcon
		{
			get { return toolbarIcon; }
		}

		public static contractSettings Settings
		{
			get { return settings; }
		}

		public static AssetBundle Prefabs
		{
			get { return prefabs; }
		}

		public static GameObject WindowPrefab
		{
			get { return _windowPrefab; }
		}

		public static Canvas CanvasPrefab
		{
			get { return canvasPrefab; }
		}

		public static void ToggleTooltips(bool isOn)
		{
			for (int i = loadedPrefabs.Length - 1; i >= 0; i--)
			{
				GameObject o = loadedPrefabs[i];

				TooltipHandler[] handlers = o.GetComponentsInChildren<TooltipHandler>(true);

				if (handlers == null)
					return;

				for (int j = 0; j < handlers.Length; j++)
					toggleTooltip(handlers[j], isOn);
			}
		}

		public static void ResetUIStyle()
		{
			if (loadedPrefabs != null)
				processUIPrefabs();
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

		private void Awake()
		{
			if (loaded)
			{
				Destroy(gameObject);
				return;
			}

			if (settings == null)
				settings = new contractSettings();

			path = KSPUtil.ApplicationRootPath + "GameData/DMagicUtilities/ContractsWindow/Resources";

			if (!spritesLoaded)
				loadTextures();

			if (!skinLoaded)
				LoadSkin();

			if (!prefabsLoaded)
				LoadPrefabs();

			if (prefabsLoaded && skinLoaded)
				loaded = true;

			if (toolbarIcon == null)
				toolbarIcon = GameDatabase.Instance.GetTexture("DMagicUtilities/ContractsWindow/Resources/ContractsIconApp", false);

			if (settings != null)
				ToggleTooltips(settings.tooltips);
		}

		private void loadTextures()
		{
			if (!clearLoaded)
			{
				Texture2D clear = new Texture2D(1, 1);
				clear.SetPixel(1, 1, Color.clear);
				clear.Apply();

				_clearSprite = Sprite.Create(clear, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

				clearLoaded = true;
			}

			if (!toggleLoaded)
			{
				GUISkin skin = HighLogic.Skin;

				if (skin == null)
					return;

				_toggleNormal = Sprite.Create(skin.toggle.normal.background, new Rect(16, 16, skin.toggle.normal.background.width - 32, skin.toggle.normal.background.height - 32), new Vector2(0.5f, 0.5f));
				_toggleHover = Sprite.Create(skin.toggle.hover.background, new Rect(16, 16, skin.toggle.hover.background.width - 32, skin.toggle.hover.background.height - 32), new Vector2(0.5f, 0.5f));
				_toggleActive = Sprite.Create(skin.toggle.active.background, new Rect(16, 16, skin.toggle.active.background.width - 32, skin.toggle.active.background.height - 32), new Vector2(0.5f, 0.5f));
				_toggleOn = Sprite.Create(skin.toggle.onNormal.background, new Rect(16, 16, skin.toggle.onNormal.background.width - 32, skin.toggle.onNormal.background.height - 32), new Vector2(0.5f, 0.5f));
				
				toggleLoaded = true;
			}

			if (clearLoaded && toggleLoaded)
				spritesLoaded = true;
		}

		private void LoadSkin()
		{
			_unitySkinDef = new UISkinDef();

			SkinInit(_unitySkinDef);

			AssetBundle skin = AssetBundle.LoadFromFile(path + unitySkinAssetName);

			if (skin == null)
				return;

			Sprite[] skinSprites = skin.LoadAllAssets<Sprite>();

			if (skinSprites == null)
				return;

			for (int i = skinSprites.Length - 1; i >= 0; i--)
			{
				Sprite s = skinSprites[i];

				if (s.name == "window")
					_unitySkinDef.window.normal.background = s;
				else if (s.name == "box")
					_unitySkinDef.box.normal.background = s;
				else if (s.name == "button")
					_unitySkinDef.button.normal.background = s;
				else if (s.name == "button hover")
					_unitySkinDef.button.highlight.background = s;
				else if (s.name == "button on")
					_unitySkinDef.button.active.background = s;
				else if (s.name == "toggle")
					_unitySkinDef.toggle.normal.background = s;
				else if (s.name == "toggle hover")
					_unitySkinDef.toggle.highlight.background = s;
				else if (s.name == "toggle active")
					_unitySkinDef.toggle.active.background = s;
				else if (s.name == "toggle on")
					_unitySkinDef.toggle.disabled.background = s;
				else if (s.name == "textfield")
					_unitySkinDef.textField.normal.background = s;
				else if (s.name == "textfield hover")
					_unitySkinDef.textField.highlight.background = s;
				else if (s.name == "textfield on")
					_unitySkinDef.textField.active.background = s;				
				else if (s.name == "vertical scrollbar")
					_unitySkinDef.verticalScrollbar.normal.background = s;
				else if (s.name == "vertical scrollbar thumb")
					_unitySkinDef.verticalScrollbarThumb.normal.background = s;
				else if (s.name == "horizontalslider")
					_unitySkinDef.horizontalSlider.normal.background = s;
				else if (s.name == "slider thumb")
					_unitySkinDef.horizontalSliderThumb.normal.background = s;
				else if (s.name == "slider thumb hover")
					_unitySkinDef.horizontalSliderThumb.highlight.background = s;
				else if (s.name == "slider thumb active")
					_unitySkinDef.horizontalSliderThumb.active.background = s;
				else if (s.name == "tooltip")
					_unityTooltipBackground = s;
				else if (s.name == "KSP_Tooltip")
					_kspTooltipBackground = s;
			}

			skinLoaded = true;
		}

		private void SkinInit(UISkinDef skin)
		{
			skin.window = new UIStyle();
			skin.box = new UIStyle();
			skin.button = new UIStyle();
			skin.toggle = new UIStyle();
			skin.textField = new UIStyle();
			skin.verticalScrollbar = new UIStyle();
			skin.verticalScrollbarThumb = new UIStyle();
			skin.horizontalSlider = new UIStyle();
			skin.horizontalSliderThumb = new UIStyle();

			skin.window.normal = new UIStyleState();
			skin.box.normal = new UIStyleState();
			skin.button.normal = new UIStyleState();
			skin.button.highlight = new UIStyleState();
			skin.button.active = new UIStyleState();
			skin.toggle.normal = new UIStyleState();
			skin.toggle.highlight = new UIStyleState();
			skin.toggle.active = new UIStyleState();
			skin.toggle.disabled = new UIStyleState();
			skin.textField.normal = new UIStyleState();
			skin.textField.highlight = new UIStyleState();
			skin.textField.active = new UIStyleState();
			skin.verticalScrollbar.normal = new UIStyleState();
			skin.verticalScrollbarThumb.normal = new UIStyleState();
			skin.horizontalSlider.normal = new UIStyleState();
			skin.horizontalSliderThumb.normal = new UIStyleState();
			skin.horizontalSliderThumb.highlight = new UIStyleState();
			skin.horizontalSliderThumb.active = new UIStyleState();
		}

		private void LoadPrefabs()
		{
			prefabs = AssetBundle.LoadFromFile(path + prefabAssetName);

			if (prefabs != null)
				loadedPrefabs = prefabs.LoadAllAssets<GameObject>();

			if (loadedPrefabs == null)
				return;

			if (!tmpProcessed)
				processTMPPrefabs();

			if (!tooltipsProcessed && _tooltipPrefab != null)
				processTooltips();

			if (!prefabsProcessed)
				processUIPrefabs();

			if (tmpProcessed && tooltipsProcessed && prefabsProcessed)
				DMC_MBE.LogFormatted("UI prefab bundle loaded and processed");
			else
				DMC_MBE.LogFormatted("Error in processing UI prefab bundle\nSome UI elements may be affected or non-functional");

			prefabsLoaded = true;
		}

		private void processTMPPrefabs()
		{
			for (int i = loadedPrefabs.Length - 1; i >= 0; i--)
			{
				GameObject o = loadedPrefabs[i];

				if (o == null)
					continue;

				if (o.name == "CW_Plus")
					_windowPrefab = o;
				else if (o.name == "CW_Canvas_Prefab")
				{
					Canvas c = o.GetComponent<Canvas>();

					if (c != null)
						canvasPrefab = c;
				}
				else if (o.name == "CW_Tooltip")
					_tooltipPrefab = o;

				processTMP(o);

				processInputFields(o);
			}

			tmpProcessed = true;
		}

		private void processTMP(GameObject obj)
		{
			if (obj == null)
				return;

			TextHandler[] handlers = obj.GetComponentsInChildren<TextHandler>(true);

			if (handlers == null)
				return;

			for (int i = 0; i < handlers.Length; i++)
			{
				TextHandler handler = handlers[i];

				if (handler.LocalizedText)
					handler.SetLocalText(GetStringWithName(handler.LocalizedName));
	
				TMProFromText(handler);
			}
		}

		private static string GetStringWithName(string loc)
		{
			return Localizer.Format("#autoLOC_" + loc);
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
			FontStyles sty = TMPProUtil.FontStyle(text.fontStyle);
			TextAlignmentOptions align = TMPProUtil.TextAlignment(text.alignment);
			float spacing = text.lineSpacing;
			GameObject obj = text.gameObject;

			MonoBehaviour.DestroyImmediate(text);

			CWTextMeshPro tmp = obj.AddComponent<CWTextMeshPro>();

			tmp.text = t;
			tmp.color = c;
			tmp.fontSize = i;
			tmp.raycastTarget = r;
			tmp.alignment = align;
			tmp.fontStyle = sty;
			tmp.lineSpacing = spacing;

			tmp.font = UISkinManager.TMPFont;
			tmp.fontSharedMaterial = Resources.Load("Fonts/Materials/Calibri Dropshadow", typeof(Material)) as Material;

			tmp.enableWordWrapping = true;
			tmp.isOverlay = false;
			tmp.richText = true;

			tmp.Setup(handler);
		}

		private void processInputFields(GameObject obj)
		{
			InputHandler[] handlers = obj.GetComponentsInChildren<InputHandler>(true);

			if (handlers == null)
				return;

			for (int i = 0; i < handlers.Length; i++)
				TMPInputFromInput(handlers[i]);
		}

		private void TMPInputFromInput(InputHandler handler)
		{
			if (handler == null)
				return;

			InputField input = handler.GetComponent<InputField>();

			if (input == null)
				return;

			int limit = input.characterLimit;
			TMP_InputField.ContentType content = GetTMPContentType(input.contentType);
			TMP_InputField.LineType line = TMPProUtil.LineType(input.lineType);
			float caretBlinkRate = input.caretBlinkRate;
			int caretWidth = input.caretWidth;
			Color selectionColor = input.selectionColor;
			GameObject obj = input.gameObject;

			RectTransform viewport = handler.GetComponentInChildren<RectMask2D>().rectTransform;
			CWTextMeshPro placholder = handler.GetComponentsInChildren<CWTextMeshPro>()[0];
			CWTextMeshPro textComponent = handler.GetComponentsInChildren<CWTextMeshPro>()[1];

			if (viewport == null || placholder == null || textComponent == null)
				return;

			MonoBehaviour.DestroyImmediate(input);

			CWTextMeshProInput tmp = obj.AddComponent<CWTextMeshProInput>();

			tmp.textViewport = viewport;
			tmp.placeholder = placholder;
			tmp.textComponent = textComponent;

			tmp.characterLimit = limit;
			tmp.contentType = content;
			tmp.lineType = line;
			tmp.caretBlinkRate = caretBlinkRate;
			tmp.caretWidth = caretWidth;
			tmp.selectionColor = selectionColor;

			tmp.readOnly = false;
			tmp.shouldHideMobileInput = false;

			tmp.fontAsset = UISkinManager.TMPFont;
		}

		private TMP_InputField.ContentType GetTMPContentType(InputField.ContentType type)
		{
			switch (type)
			{
				case InputField.ContentType.Alphanumeric:
					return TMP_InputField.ContentType.Alphanumeric;
				case InputField.ContentType.Autocorrected:
					return TMP_InputField.ContentType.Autocorrected;
				case InputField.ContentType.Custom:
					return TMP_InputField.ContentType.Custom;
				case InputField.ContentType.DecimalNumber:
					return TMP_InputField.ContentType.DecimalNumber;
				case InputField.ContentType.EmailAddress:
					return TMP_InputField.ContentType.EmailAddress;
				case InputField.ContentType.IntegerNumber:
					return TMP_InputField.ContentType.IntegerNumber;
				case InputField.ContentType.Name:
					return TMP_InputField.ContentType.Name;
				case InputField.ContentType.Password:
					return TMP_InputField.ContentType.Password;
				case InputField.ContentType.Pin:
					return TMP_InputField.ContentType.Pin;
				case InputField.ContentType.Standard:
					return TMP_InputField.ContentType.Standard;
				default:
					return TMP_InputField.ContentType.Standard;
			}
		}

		private void processTooltips()
		{
			for (int i = loadedPrefabs.Length - 1; i >= 0; i--)
			{
				GameObject o = loadedPrefabs[i];

				TooltipHandler[] handlers = o.GetComponentsInChildren<TooltipHandler>(true);

				if (handlers == null)
					return;

				for (int j = 0; j < handlers.Length; j++)
					processTooltip(handlers[j]);
			}

			tooltipsProcessed = true;
		}

		private void processTooltip(TooltipHandler handler)
		{
			if (handler == null)
				return;

			handler.Prefab = _tooltipPrefab;

			int count = handler.TooltipCount;

			string[] text = new string[count];

			for (int i = count - 1; i >= 0; i--)
				text[i] = GetStringWithName(handler.TooltipNames(i));

			handler.TooltipText = text;

			toggleTooltip(handler, contractLoader.Settings.tooltips);
		}

		private static void toggleTooltip(TooltipHandler handler, bool isOn)
		{
			if (handler == null)
				return;

			handler.IsActive = isOn;
		}

		private static void processUIPrefabs()
		{
			for (int i = loadedPrefabs.Length - 1; i >= 0; i--)
			{
				GameObject o = loadedPrefabs[i];

				if (o != null)
					processUIComponents(o);
			}

			prefabsProcessed = true;
		}

		private static void processUIComponents(GameObject obj)
		{
			CW_Style[] styles = obj.GetComponentsInChildren<CW_Style>(true);

			if (styles == null)
				return;

			for (int i = 0; i < styles.Length; i++)
				processComponents(styles[i]);
		}

		private static void processComponents(CW_Style style)
		{
			if (style == null)
				return;

			UISkinDef skin = UISkinManager.defaultSkin;

			bool stock = contractLoader.Settings.stockUIStyle || _unitySkinDef == null;

			if (!stock)
				skin = _unitySkinDef;

			if (skin == null)
				return;

			switch (style.StlyeType)
			{
				case CW_Style.StyleTypes.Window:
					style.setImage(skin.window.normal.background);
					break;
				case CW_Style.StyleTypes.Box:
					style.setImage(skin.box.normal.background);
					break;
				case CW_Style.StyleTypes.Button:
					style.setButton(skin.button.normal.background, skin.button.highlight.background, skin.button.active.background, skin.button.active.background);
					break;
				case CW_Style.StyleTypes.HiddenButton:
					style.setButton(_clearSprite, skin.button.highlight.background, skin.button.active.background, skin.button.active.background);
					break;
				case CW_Style.StyleTypes.Toggle:
					if (stock)
						style.setToggle(_toggleNormal, _toggleHover, _toggleActive, _toggleActive, _toggleOn);
					else
						style.setToggle(skin.toggle.normal.background, skin.toggle.highlight.background, skin.toggle.active.background, skin.toggle.active.background, skin.toggle.disabled.background);
					break;
				case CW_Style.StyleTypes.ToggleButton:
					style.setButton(skin.button.normal.background, skin.button.highlight.background, skin.button.active.background, skin.button.active.background);
					break;
				case CW_Style.StyleTypes.HorizontalSlider:
					style.setSlider(skin.horizontalSlider.normal.background, skin.horizontalSliderThumb.normal.background, skin.horizontalSliderThumb.highlight.background, skin.horizontalSliderThumb.active.background, skin.horizontalSliderThumb.active.background);
					break;
				case CW_Style.StyleTypes.VerticalScrollbar:
					style.setScrollbar(skin.verticalScrollbar.normal.background, skin.verticalScrollbarThumb.normal.background);
					break;
				case CW_Style.StyleTypes.Tooltip:
					style.setImage(stock ? _kspTooltipBackground : _unityTooltipBackground);
					break;
				case CW_Style.StyleTypes.Popup:
					style.setImage(stock ? _kspTooltipBackground : _unityTooltipBackground);
					break;
				case CW_Style.StyleTypes.HiddenToggleButton:
					style.setButton(_clearSprite, skin.button.highlight.background, skin.button.active.background, skin.button.active.background);
					break;
				default:
					break;
			}
		}
	}
}
