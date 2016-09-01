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

		private INote noteInterface;

		public void setNote(INote note)
		{
			if (note == null)
				return;

			noteInterface = null;

			if (NoteText == null)
				return;

			NoteText.text = note.NoteText;
		}

		public void Update()
		{
			if (noteInterface == null)
				return;

			if (!noteInterface.IsVisible)
				return;

			if (NoteText == null)
				return;

			noteInterface.Update();

			NoteText.text = noteInterface.NoteText;
		}
	}
}
