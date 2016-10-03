#region license
/*The MIT License (MIT)
ICW_Window - Interface for transferring information on the main window

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
using System.Collections.Generic;
using ContractsWindow.Unity.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Interfaces
{
	public interface ICW_Window
	{
		bool HideTooltips { get; set; }

		bool BlizzyAvailable { get; }

		bool StockToolbar { get; set; }

		bool ReplaceToolbar { get; set; }

		bool LargeFont { get; set; }

		bool IgnoreScale { get; set; }

		float Scale { get; set; }

		float MasterScale { get; }

		string Version { get; }

		Canvas MainCanvas { get; }

		IList<IMissionSection> GetMissions { get; }

		IMissionSection GetCurrentMission { get; }

		void Rebuild();

		void NewMission(string title, Guid id);

		void SetWindowPosition(Rect r);
	}
}
