using Lab;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;

namespace Sandbox.UI
{

	public class SelectionBox : Panel
	{
		public SelectionBox()
		{
			StyleSheet.Load( "/ui/SelectionBox.scss" );
		}

		public override void Tick()
		{
			base.Tick();

			if ( Game.LocalPawn is LabPawn labPawn )
			{
				Tick( labPawn.FrustumSelect );
			}
		}

		private void Tick( FrustumSelect frustumSelect )
		{
			SetClass( "active", frustumSelect.IsDragging );
			if ( !frustumSelect.IsDragging ) return;

			var f = frustumSelect.GetFrustum( 1000 );

			var topleft = f.GetCorner( 0 ).Value.ToScreen();
			var bottomright = f.GetCorner( 2 ).Value.ToScreen();

			Style.Left = topleft.x * Screen.Width * ScaleFromScreen;
			Style.Top = topleft.y * Screen.Height * ScaleFromScreen;
			Style.Width = (bottomright.x - topleft.x) * Screen.Width * ScaleFromScreen;
			Style.Height = (bottomright.y - topleft.y) * Screen.Height * ScaleFromScreen;
			Style.Dirty();
		}
	}
}
