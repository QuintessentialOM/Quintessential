using Quintessential;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

class patch_Sim {

	// add our parts to the panel

	public extern void orig_method_1832(bool first);
	public void method_1832(bool first) {
		orig_method_1832(first);
		foreach(var action in QApi.ToRunAfterCycle)
			action((Sim)(object)this, first);
	}
}