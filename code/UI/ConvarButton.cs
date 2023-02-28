
using System;

namespace Sandbox.UI
{
	public class ConvarButtonGroup : Panel
	{
		public string ConvarName { get; set; }
		public string ConvarValue { get; set; }

		bool initialized;

		public ConvarButtonGroup()
		{
			AddClass( "group" );
		}

		public override void Tick()
		{
			base.Tick();

			if ( string.IsNullOrWhiteSpace( ConvarName ) )
				return;

			if ( !initialized )
				init();

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

		private void init()
		{
			initialized = true;

			foreach ( var child in Children )
			{
				if ( child.StringValue == null ) continue;

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
