using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	public class CW_Note : MonoBehaviour
	{
		[SerializeField]
		private Text NoteText = null;

		public void setNote(string note)
		{
			if (NoteText == null)
				return;

			NoteText.text = note;
		}
	}
}
