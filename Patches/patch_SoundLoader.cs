using System.IO;
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
		return orig_method_616(QApi.fetchPath(path,".wav"));
	}

	public extern Song orig_method_617(string path);

	public Song method_617(string path)
	{
		return orig_method_617(QApi.fetchPath(path,".ogg"));
	}
}