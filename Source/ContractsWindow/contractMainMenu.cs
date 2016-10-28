#region license
/*The MIT License (MIT)
Contract Main Menu - Addon to initialize settings and textures

Copyright (c) 2014 DMagic

KSP Plugin Framework by TriggerAu, 2014: http://forum.kerbalspaceprogram.com/threads/66503-KSP-Plugin-Framework

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

using ContractsWindow.Toolbar;
using UnityEngine;

namespace ContractsWindow
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	class contractMainMenu: DMC_MBE
	{
		private const string filePath = "PluginData/Settings";
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

		protected override void Start()
		{
			if (toolbarIcon == null)
				toolbarIcon = GameDatabase.Instance.GetTexture("DMagicUtilities/ContractsWindow/Resources/ContractsIconApp", false);

			if (settings == null)
				settings = new contractSettings(filePath);

			if (settings != null)
				contractLoader.UpdateTooltips(settings.tooltips);

			Destroy(gameObject);
		}
	}
}
