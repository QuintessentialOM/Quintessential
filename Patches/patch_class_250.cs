using MonoMod;
using Quintessential;

class patch_class_250 {

	[PatchGifRecorderFrame]
	[MonoModIgnore]
	public extern void method_50(float param_4165);

	private static void MarkOnFrame() {
		var markerPos = new Vector2(20, 647 - 60);
		class_135.method_272(class_238.field_1989.field_81.field_613.field_632, markerPos);
		class_135.method_290(QuintessentialLoader.VersionString, markerPos + new Vector2(30, -2), class_238.field_1990.field_2145, Color.LightGray, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), class_238.field_1989.field_71, int.MaxValue, true, true);
	}
}