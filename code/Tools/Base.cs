using Sandbox;
using Sandbox.UI;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab.Tools
{
	public class Base : LibraryClass
	{
		public Entity Owner { get; set; }

		public virtual void OnClick( TraceResult tr, IList<Entity> selected )
		{

		}

		public virtual void Tick( TraceResult tr, IList<Entity> selected )
		{
			
		}
	}
}
