using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	public class CW_Rebuild : CW_Popup
	{
		private CW_Window parent;

		public void setInterface(CW_Window p)
		{
			if (p == null)
				return;

			parent = p;
		}

		public void Rebuild()
		{
			if (parent == null)
				return;

			if (parent.Interface == null)
				return;

			parent.Interface.Rebuild();

			parent.DestroyChild(gameObject);
		}
	}
}
