using Sandbox.UI;

namespace Lab
{
	public partial class HudEntity : Sandbox.HudEntity<RootPanel>
	{
		public HudEntity()
		{
			if ( IsClient )
			{
				RootPanel.SetTemplate( "/lab/labhud.html" );
			}
		}
	}

}
