#region license
/*The MIT License (MIT)
CW_StandardNode - Controls progress node UI elements

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
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	public class CW_StandardNode : MonoBehaviour
	{
		[SerializeField]
		private TextHandler Title = null;
		[SerializeField]
		private TextHandler Reward = null;
		[SerializeField]
		private Toggle NoteToggle = null;
		[SerializeField]
		private GameObject NotePrefab = null;
		[SerializeField]
		private Transform NoteTransform = null;
		[SerializeField]
		private TooltipHandler NoteTooltip = null;

		private IStandardNode standardInterface;
		private CW_Note note;

		public IStandardNode StandardInterface
		{
			get { return standardInterface; }
		}

		public void setNode(IStandardNode node)
		{
			if (node == null)
				return;

			standardInterface = node;

			if (Title != null)
				Title.OnTextUpdate.Invoke(node.NodeText);
			
			if (Reward != null)
				Reward.OnTextUpdate.Invoke(node.RewardText);

			if (!string.IsNullOrEmpty(node.GetNote))
				setNote();
			else if (NoteToggle != null)
				NoteToggle.gameObject.SetActive(false);
		}

		public void UpdateText()
		{
			if (standardInterface == null)
				return;

			if (Title != null)
				Title.OnTextUpdate.Invoke(standardInterface.NodeText);

			if (Reward != null)
				Reward.OnTextUpdate.Invoke(standardInterface.RewardText);
		}

		private void setNote()
		{
			if (standardInterface == null)
				return;

			if (NotePrefab == null || NoteTransform == null)
				return;

			GameObject obj = Instantiate(NotePrefab);

			if (obj == null)
				return;

			obj.transform.SetParent(NoteTransform, false);

			note = obj.GetComponent<CW_Note>();

			if (note == null)
				return;

			note.setNote(standardInterface.GetNote);

			note.gameObject.SetActive(false);
		}

		public void NoteOn(bool isOn)
		{
			if (standardInterface == null)
				return;
			
			if (note == null)
				return;

			if (isOn)
				note.setNote(standardInterface.GetNote);

			note.gameObject.SetActive(isOn);

			if (NoteTooltip != null)
				NoteTooltip.SetNewText(isOn ? "Hide Note" : "Show Note");
		}
	}
}
