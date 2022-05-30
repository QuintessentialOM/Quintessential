using MonoMod;
using Quintessential;
using Song = class_186;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
[MonoModPatch("class_235")]



class patch_SoundLoader
{
	public extern Sound orig_method_616(string path);
	public Sound method_616(string path)
	{
		path = QApi.fetchPath(path, ".wav");
		return orig_method_616(path.Substring(path.Length - 4));
	}

	// Adding this causes a "Common Language Runtime detected an invalid program" error for some reason??? fix later
	//
	//public extern Song orig_method_617(string path);
	//
	//public Song method_617(string path)
	//{
	//	//path = QApi.fetchPath(path, ".ogg");
	//	return orig_method_617(path);
	//}
}