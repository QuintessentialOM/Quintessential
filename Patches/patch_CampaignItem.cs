#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

public class patch_CampaignItem{

	public class_256 Icon, IconSmall;

	public extern class_256 orig_method_826();
	public class_256 method_826() => Icon ?? orig_method_826();
	
	public extern class_256 orig_method_827();
	public class_256 method_827() => IconSmall ?? orig_method_827();
}