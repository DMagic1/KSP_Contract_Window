#region license
/*The MIT License (MIT)
ToggleSpriteHandler - Script to handle switching sprites on button press

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

using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity
{
	[RequireComponent(typeof(Image))]
	public class ToggleSpriteHandler : MonoBehaviour
	{
		[SerializeField]
		private Sprite OnSprite = null;
		[SerializeField]
		private Sprite OffSprite = null;
		[SerializeField]
		private Sprite Alternate = null;
		[SerializeField]
		private Toggle ParentToggle = null;

		private Image image;
		private bool alternateSet;

		private void Awake()
		{
			if (ParentToggle == null || OnSprite == null || OffSprite == null)
				return;

			ParentToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>(onToggle));

			image = GetComponent<Image>();
		}

		public void onToggle(bool isOn)
		{
			if (image == null || OnSprite == null || OffSprite == null)
				return;

			if (alternateSet)
				return;

			image.sprite = isOn ? OnSprite : OffSprite;
		}

		public void SetAlternate()
		{
			if (Alternate == null || image == null)
				return;

			alternateSet = true;

			image.sprite = Alternate;
		}
	}
}
