using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;

namespace Sandbox.UI
{
	[Library( "ConvarButtonGroup" )]
	public class ConvarButtonGroup : Panel
	{
		public string ConvarName { get; set; }
		public string ConvarValue { get; set; }

		public ConvarButtonGroup()
		{
			AddClass( "group" );
		}

		public override void SetProperty( string name, string value )
		{
			if ( name == "convar" )
			{
				ConvarName = value;
				UpdateFromConVar();
			}

			base.SetProperty( name, value );
		}

		public override void Tick()
		{
			base.Tick();

			if ( string.IsNullOrWhiteSpace( ConvarName ) )
				return;

			UpdateFromConVar();
		}

		public virtual void UpdateFromConVar()
		{
			UpdateValue( ConsoleSystem.GetValue( ConvarName, ConvarValue ) );
		}

		void UpdateValue( string value )
		{
			if ( value == ConvarValue ) return;

			ConvarValue = value;

			foreach( var child in Children )
			{
				child.SetClass( "active", string.Equals( child.StringValue, ConvarValue, StringComparison.OrdinalIgnoreCase ) );
			}
		}

		void SetValue( string value )
		{
			ConsoleSystem.Run( ConvarName, value );
		}

		protected override void OnChildAdded( Panel child )
		{
			base.OnChildAdded( child );

			if ( child.StringValue != null )
			{
				child.AddEventListener( "onmousedown", () => SetValue( child.StringValue ) );
				child.SetClass( "active", string.Equals( child.StringValue, ConvarValue, StringComparison.OrdinalIgnoreCase ) );
			}
		}

		Panel _selected;

		public Panel SelectedButton
		{
			get => _selected;
			set
			{
				if ( _selected == value )
					return;

				_selected?.RemoveClass( "active" );
				_selected?.CreateEvent( "stopactive" );

				_selected = value;

				_selected?.AddClass( "active" );
				_selected?.CreateEvent( "startactive" );
			}
		}
	}
}
