#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE1006 // Naming Styles

using MonoMod;
using Quintessential;

[MonoModPatch("class_250")]
class patch_GifRecorder{

	[PatchGifRecorderFrame]
	[MonoModIgnore]
	public extern void method_50(float param_4165);

	// name is used in MonoModRules
	private static void MarkOnFrame(){
		var markerPos = new Vector2(826 - 60 - 40, 647 - 61);
		var verPos = new Vector2(826 - 60 - 40 - 20, 647 - 40);
		class_135.method_272(class_238.field_1989.field_81.field_613.field_632, markerPos);
		class_135.method_290(QuintessentialLoader.VersionString, verPos, class_238.field_1990.field_2145, Color.LightGray, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), class_238.field_1989.field_71, int.MaxValue, true, true);
	}
}