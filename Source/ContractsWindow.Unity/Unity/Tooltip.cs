/// Credit drHogan 
/// Sourced from - http://forum.unity3d.com/threads/screenspace-camera-tooltip-controller-sweat-and-tears.293991/#post-1938929
/// updated ddreaper - refactored code to be more performant.
/// *Note - only works for Screenspace Camera canvases at present, needs updating to include Screenspace and Worldspace!

//ToolTip is written by Emiliano Pastorelli, H&R Tallinn (Estonia), http://www.hammerandravens.com
//Copyright (c) 2015 Emiliano Pastorelli, H&R - Hammer&Ravens, Tallinn, Estonia.
//All rights reserved.

//Redistribution and use in source and binary forms are permitted
//provided that the above copyright notice and this paragraph are
//duplicated in all such forms and that any documentation,
//advertising materials, and other materials related to such
//distribution and use acknowledge that the software was developed
//by H&R, Hammer&Ravens. The name of the
//H&R, Hammer&Ravens may not be used to endorse or promote products derived
//from this software without specific prior written permission.
//THIS SOFTWARE IS PROVIDED ``AS IS'' AND WITHOUT ANY EXPRESS OR
//IMPLIED WARRANTIES, INCLUDING, WITHOUT LIMITATION, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.

using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	[RequireComponent(typeof(RectTransform))]
	[AddComponentMenu("UI/Extensions/Tooltip")]
	public class ToolTip : MonoBehaviour
	{
		//text of the tooltip
		private TextHandler _handler;
		private RectTransform _rectTransform;

		//if the tooltip is inside a UI element
		private bool _inside;
		
		private float width, height;


		private float YShift, xShift;

		private RenderMode _guiMode;

		private Camera _guiCamera;

		private bool waken;

		// Use this for initialization
		public void WakeUp()
		{
			if (CW_Window.Window == null || CW_Window.Window.Interface == null || CW_Window.Window.Interface.MainCanvas == null)
				return;

			this.gameObject.SetActive(true);

			var _canvas = CW_Window.Window.Interface.MainCanvas;
			_guiCamera = _canvas.worldCamera;
			_guiMode = _canvas.renderMode;
			_rectTransform = GetComponent<RectTransform>();

			_handler = GetComponentInChildren<TextHandler>();

			_inside = false;

			xShift = 5f;
			YShift = -35f;

			waken = true;
		}

		//Call this function externally to set the text of the template and activate the tooltip
		public void SetTooltip(string text)
		{
			if (!waken)
				WakeUp();

			if (_guiMode == RenderMode.ScreenSpaceCamera)
			{
				//set the text and fit the tooltip panel to the text size
				this.gameObject.SetActive(true);

				_handler.OnTextUpdate.Invoke(text);

				_rectTransform.sizeDelta = new Vector2(_handler.PreferredSize.x + 10f, _handler.PreferredSize.y + 0f);

				OnScreenSpaceCamera();
			}
		}

		//call this function on mouse exit to deactivate the template
		public void HideTooltip()
		{
			if (_guiMode == RenderMode.ScreenSpaceCamera)
			{
				this.gameObject.SetActive(false);
				_inside = false;
			}
		}

		// Update is called once per frame
		void FixedUpdate()
		{
			if (_inside)
			{
				if (_guiMode == RenderMode.ScreenSpaceCamera)
				{
					OnScreenSpaceCamera();
				}
			}
		}

		//main tooltip edge of screen guard and movement
		public void OnScreenSpaceCamera()
		{
			Vector3 newPos = _guiCamera.ScreenToViewportPoint(Input.mousePosition - new Vector3(xShift, YShift, 0f));
			Vector3 newPosWVP = _guiCamera.ViewportToWorldPoint(newPos);

			width = _rectTransform.sizeDelta[0];
			height = _rectTransform.sizeDelta[1];

			// check and solve problems for the tooltip that goes out of the screen on the horizontal axis
			float val;

			Vector3 lowerLeft = _guiCamera.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, 0.0f));
			Vector3 upperRight = _guiCamera.ViewportToWorldPoint(new Vector3(1.0f, 1.0f, 0.0f));

			//check for right edge of screen
			val = (newPosWVP.x + width / 2);
			if (val > upperRight.x)
			{
				Vector3 shifter = new Vector3(val - upperRight.x, 0f, 0f);
				Vector3 newWorldPos = new Vector3(newPosWVP.x - shifter.x, newPos.y, 0f);
				newPos.x = _guiCamera.WorldToViewportPoint(newWorldPos).x;
			}
			//check for left edge of screen
			val = (newPosWVP.x - width / 2);
			if (val < lowerLeft.x)
			{
				Vector3 shifter = new Vector3(lowerLeft.x - val, 0f, 0f);
				Vector3 newWorldPos = new Vector3(newPosWVP.x + shifter.x, newPos.y, 0f);
				newPos.x = _guiCamera.WorldToViewportPoint(newWorldPos).x;
			}

			// check and solve problems for the tooltip that goes out of the screen on the vertical axis

			//check for upper edge of the screen
			val = (newPosWVP.y + height / 2);
			if (val > upperRight.y)
			{
				Vector3 shifter = new Vector3(0f, 35f + height / 2, 0f);
				Vector3 newWorldPos = new Vector3(newPos.x, newPosWVP.y - shifter.y, 0f);
				newPos.y = _guiCamera.WorldToViewportPoint(newWorldPos).y;
			}

			//check for lower edge of the screen (if the shifts of the tooltip are kept as in this code, no need for this as the tooltip always appears above the mouse bu default)
			val = (newPosWVP.y - height / 2);
			if (val < lowerLeft.y)
			{
				Vector3 shifter = new Vector3(0f, 35f + height / 2, 0f);
				Vector3 newWorldPos = new Vector3(newPos.x, newPosWVP.y + shifter.y, 0f);
				newPos.y = _guiCamera.WorldToViewportPoint(newWorldPos).y;
			}
			this.transform.position = new Vector3(newPosWVP.x, newPosWVP.y, 1f);
			//this.gameObject.SetActive(true);
			_inside = true;
		}
	}
}