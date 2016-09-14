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
		private Text Title = null;
		[SerializeField]
		private Text Reward = null;
		[SerializeField]
		private Toggle NoteToggle = null;
		[SerializeField]
		private GameObject NotePrefab = null;
		[SerializeField]
		private Transform NoteTransform = null;

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
				Title.text = node.NodeText;
			
			if (Reward != null)
				Reward.text = node.RewardText;

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
				Title.text = standardInterface.NodeText;

			if (Reward != null)
				Reward.text = standardInterface.RewardText;
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
		}
	}
}
