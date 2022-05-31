using System.IO;
using MonoMod;
using Quintessential;
using Song = class_186;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
[MonoModPatch("class_235")]

class patch_SoundLoader
{
	public extern static Sound orig_method_616(string path);
	public static Sound method_616(string path)
	{
		string filePath = QApi.fetchPath(path, ".wav");
		return new Sound()
		{
			field_4060 = Path.GetFileNameWithoutExtension(path),
			field_4061 = class_158.method_375(filePath)
		};
	}

	public extern static Song orig_method_617(string path);
	
	public static Song method_617(string path)
	{
		string filePath = QApi.fetchPath(path, ".ogg");
		return new Song()
		{
			field_1739 = class_158.method_375(filePath)
		};
	}
}