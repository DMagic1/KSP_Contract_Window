#region license
/*The MIT License (MIT)
Contract contractSettings - Persistent settings file

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

using System;
using System.IO;
using System.Reflection;

namespace ContractsWindow
{
	public class contractSettings
	{
		[Persistent]
		public bool tooltips = true;
		[Persistent]
		public bool pixelPerfect = false;
		[Persistent]
		public bool largeFont = false;
		[Persistent]
		public bool ignoreKSPScale = false;
		[Persistent]
		public float windowScale = 1;
		[Persistent]
		public bool useStockToolbar = true;
		[Persistent]
		public bool replaceStockApp = false;

		private const string fileName = "PluginData/Settings.cfg";
		private string fullPath;

		public contractSettings()
		{
			fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName).Replace("\\", "/");

			if (!Load())
				DMC_MBE.LogFormatted("Failed to load settings file");
		}

		public bool Load()
		{
			try
			{
				if (File.Exists(fullPath))
				{
					ConfigNode node = ConfigNode.Load(fullPath);
					ConfigNode unwrapped = node.GetNode(GetType().Name);
					ConfigNode.LoadObjectFromConfig(this, unwrapped);
					return true;
				}
				else
				{
				 	DMC_MBE.LogFormatted("Settings file could not be found [{0}]", fullPath);
					return false;
				}
			}
			catch (Exception e)
			{
				DMC_MBE.LogFormatted("Error while loading settings file from [{0}]\n{1}", fullPath, e);
				return false;
			}
		}

		public bool Save()
		{
			try
			{
				ConfigNode node = AsConfigNode();
				ConfigNode wrapper = new ConfigNode(GetType().Name);
				wrapper.AddNode(node);
				wrapper.Save(fullPath);
				return true;
			}
			catch (Exception e)
			{
				DMC_MBE.LogFormatted("Error while saving settings file at [{0}]\n{1}", fullPath, e);
				return false;
			}
		}

		private ConfigNode AsConfigNode()
		{
			try
			{
				ConfigNode node = new ConfigNode(GetType().Name);

				node = ConfigNode.CreateConfigFromObject(this, node);
				return node;
			}
			catch (Exception e)
			{
				DMC_MBE.LogFormatted("Failed to generate settings file node...\n{0}", e);
				return new ConfigNode(GetType().Name);
			}
		}
	}
}
