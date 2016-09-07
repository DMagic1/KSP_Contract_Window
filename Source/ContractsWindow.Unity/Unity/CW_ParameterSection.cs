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
		private GameObject NotePrefab = null;
		[SerializeField]
		private Transform NoteTransform = null;
		[SerializeField]
		private GameObject SubParamPrefab = null;
		[SerializeField]
		private Transform SubParamTransform = null;
		[SerializeField]
		private LayoutElement Spacer = null;
		[SerializeField]
		private LayoutElement ParameterLayout = null;

		private Color textColor = new Color(0.9411765f, 0.5137255f, 0.227451f, 1f);
		private Color successColor = new Color(0.4117647f, 0.8470588f, 0.3098039f, 1f);
		private Color failColor = new Color(0.8980392f, 0f, 0f, 1f);
		private Color subColor = new Color(0.8470588f, 0.8627451f, 0.8392157f, 1f);
		private ContractState oldState;
		private IParameterSection parameterInterface;
		private CW_Note note;
		private List<CW_ParameterSection> parameters = new List<CW_ParameterSection>();

		public void setParameter(IParameterSection section)
		{
			if (section == null)
				return;

			if (ParameterText == null || ParameterRewardText == null || ParameterPenaltyText == null)
				return;

			if (Spacer == null || ParameterLayout == null)
				return;

			parameterInterface = section;

			Spacer.minWidth = parameterInterface.ParamLayer * 5;

			ParameterLayout.minWidth -= parameterInterface.ParamLayer * 5;

			ParameterText.text = parameterInterface.TitleText;

			ParameterRewardText.text = parameterInterface.RewardText;

			ParameterPenaltyText.text = parameterInterface.PenaltyText;

			oldState = parameterInterface.ParameterState;

			if (!string.IsNullOrEmpty(parameterInterface.GetNote))
				setNote();
			else if (NoteToggle != null)
				NoteToggle.gameObject.SetActive(false);

			if (parameterInterface.ParamLayer < 4)
				CreateSubParameters(parameterInterface.GetSubParams);
		}

		public void ToggleSubParams(bool isOn)
		{
			if (parameterInterface == null)
				return;

			if (isOn && parameterInterface.ParameterState == ContractState.Complete)
				return;

			for (int i = parameters.Count - 1; i >= 0; i--)
			{
				CW_ParameterSection parameter = parameters[i];

				if (parameter == null)
					continue;

				parameter.ToggleSubParams(isOn);

				parameter.gameObject.SetActive(isOn);
			}
		}

		private void Update()
		{
			if (parameterInterface == null)
				return;

			if (oldState != parameterInterface.ParameterState)
			{
				oldState = parameterInterface.ParameterState;

				if (oldState != ContractState.Active)
					ToggleSubParams(false);
				else
					ToggleSubParams(true);
			}
			
			if (ParameterText == null)
				return;

			ParameterText.text = parameterInterface.TitleText;

			ParameterText.color = stateColor(oldState);
		}

		public void ToggleNote(bool isOn)
		{
			if (parameterInterface == null)
				return;

			if (note == null)
				return;

			note.gameObject.SetActive(isOn);
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
					if (parameterInterface.ParamLayer == 0)
						return textColor;
					else
						return subColor;
				case ContractState.Complete:
					return successColor;
				case ContractState.Fail:
					return failColor;
				default:
					return textColor;
			}
		}

		private void CreateSubParameters(IList<IParameterSection> sections)
		{
			if (sections == null)
				return;

			if (parameterInterface == null)
				return;

			if (SubParamPrefab == null || SubParamTransform == null)
				return;

			for (int i = sections.Count - 1; i >= 0; i--)
			{
				IParameterSection section = sections[i];

				if (section == null)
					continue;

				CreateSubParameter(section);
			}
		}

		private void CreateSubParameter(IParameterSection section)
		{
			GameObject obj = Instantiate(SubParamPrefab);

			if (obj == null)
				return;

			obj.transform.SetParent(SubParamTransform, false);

			CW_ParameterSection paramObject = obj.GetComponent<CW_ParameterSection>();

			if (paramObject == null)
				return;

			paramObject.setParameter(section);

			parameters.Add(paramObject);

			paramObject.gameObject.SetActive(parameterInterface.ParameterState != ContractState.Complete);
		}
	}
}
