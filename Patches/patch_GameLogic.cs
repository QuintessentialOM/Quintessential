using MonoMod;
using Quintessential;

class patch_GameLogic {

	// calls mod loading
	// also removes WorkshopManager calls

	[PatchGameLogicInit]
	public extern void orig_method_942();

	public void method_942() {
		QuintessentialLoader.PreInit();
		orig_method_942();
		QuintessentialLoader.PostLoad();
	}
}