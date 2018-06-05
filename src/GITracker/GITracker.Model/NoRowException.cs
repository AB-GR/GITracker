using System;
using System.Collections.Generic;
using System.Text;

namespace GITracker.Model
{
	public class NoRowException : Exception
	{
		public NoRowException(Type tableType) : base($"{tableType.Name}: First")
		{ }
		public NoRowException(Type tableType, string message) : base($"{tableType.Name}: {message}")
		{ }
	}
}
