using Quintessential;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

class patch_class_201
{
	// Reset QSounds when normal sounds are reset.
	public extern void orig_method_540();
	public void method_540()
	{
		orig_method_540();
		foreach (var q in QSound.AllQSounds) q.sound.field_4062 = false;
	}
}