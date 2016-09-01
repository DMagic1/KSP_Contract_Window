using System;
using System.Collections.Generic;

namespace ContractsWindow.Unity.Interfaces
{
	public interface INote
	{
		bool IsVisible { get; }

		string NoteText { get; }

		void Update();
	}
}
