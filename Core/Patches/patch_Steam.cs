using Quintessential;
using Quintessential.Internal;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
#pragma warning disable IDE1006 // Naming Styles

internal class patch_Steam{

	public static extern void orig_DiscardStaleEventsAndRunCallbacks();
	public static void DiscardStaleEventsAndRunCallbacks(){
		orig_DiscardStaleEventsAndRunCallbacks(); // is this necessary?
		QuintessentialLoader.CheckCampaignReload();
	}
}
