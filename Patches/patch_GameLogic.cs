using MonoMod;
using Quintessential;

class patch_GameLogic {

	[PatchGameLogicInit]
	public extern void orig_method_946();

	public void method_946() {
		QuintessentialLoader.PreInit();
		orig_method_946();
		QuintessentialLoader.PostLoad();
	}
}