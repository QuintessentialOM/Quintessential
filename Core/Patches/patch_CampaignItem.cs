#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

public class patch_CampaignItem{

	public Texture Icon, IconSmall;

	public extern Texture orig_GetIcon();
	public Texture GetIcon() => Icon ?? orig_GetIcon();
	
	public extern Texture orig_GetIconSmall();
	public Texture GetIconSmall() => IconSmall ?? orig_GetIconSmall();
}