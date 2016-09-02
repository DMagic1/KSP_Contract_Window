using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ContractsWindow.Unity.Interfaces;

namespace ContractsWindow.Unity.Unity
{
	public class CW_ParameterSection : MonoBehaviour
	{
		[SerializeField]
		private Text ParameterText = null;
		[SerializeField]
		private Text ParameterRewardText = null;
		[SerializeField]
		private Text ParameterPenaltyText = null;
		[SerializeField]
		private Toggle NoteToggle = null;
		[SerializeField]
		private Image ParameterNoteImage = null;
		[SerializeField]
		private Sprite NoteOn = null;
		[SerializeField]
		private Sprite NoteOff = null;
		[SerializeField]
		private GameObject NotePrefab = null;
		[SerializeField]
		private Transform NoteTransform = null;

		private Color textColor = new Color();
		private Color successColor = new Color();
		private Color failColor = new Color();
		private IParameterSection parameterInterface;
		private CW_Note note;

		public void setParameter(IParameterSection section)
		{
			if (section == null)
				return;

			if (ParameterText == null || ParameterRewardText == null || ParameterPenaltyText == null)
				return;

			parameterInterface = section;

			ParameterText.text = parameterInterface.TitleText;

			ParameterRewardText.text = parameterInterface.RewardText;

			ParameterPenaltyText.text = parameterInterface.PenaltyText;

			if (parameterInterface.HasNote)
				setNote();
			else if (NoteToggle != null)
				NoteToggle.gameObject.SetActive(false);
		}

		private void Update()
		{
			if (parameterInterface == null)
				return;

			if (!parameterInterface.IsVisible)
				return;

			if (ParameterText == null)
				return;

			parameterInterface.Update();

			ParameterText.color = stateColor(parameterInterface.ParameterState);
		}

		public void ToggleNote(bool isOn)
		{
			if (parameterInterface == null)
				return;

			if (ParameterNoteImage == null || NoteOn == null || NoteOff == null)
				return;

			if (note == null)
				return;

			note.gameObject.SetActive(isOn);

			ParameterNoteImage.sprite = isOn ? NoteOff : NoteOn;
		}

		private void setNote()
		{
			if (parameterInterface == null)
				return;

			if (NotePrefab == null || NoteTransform == null)
				return;

			GameObject obj = Instantiate(NotePrefab);

			if (obj == null)
				return;

			parameterInterface.ProcessStyle(obj);

			obj.transform.SetParent(NoteTransform, false);

			note = obj.GetComponent<CW_Note>();

			if (note == null)
				return;

			note.setNote(parameterInterface.GetNote);

			note.gameObject.SetActive(false);
		}

		private Color stateColor(ContractState state)
		{
			switch (state)
			{
				case ContractState.Active:
					return textColor;
				case ContractState.Complete:
					return successColor;
				case ContractState.Fail:
					return failColor;
				default:
					return textColor;
			}
		}
	}
}
