using Quintessential;
using System;
using System.IO;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

class patch_Renderer {

	// checks mods for textures before vanilla

	public static extern bool orig_method_1339(class_256 param_5118);

	public static bool method_1339(class_256 textureInfo) {
		if(textureInfo.field_2062.method_1085() /*Exists*/ && textureInfo.field_2062.method_1087()/*Get*/.StartsWith("Content")) {
			Maybe<string> origPath = textureInfo.field_2062;
			for(int i = QuintessentialLoader.ModContentDirectories.Count - 1; i >= 0; i--) {
				string dir = QuintessentialLoader.ModContentDirectories[i];
				try {
					textureInfo.field_2062 = Path.Combine(dir, origPath.method_1087());
					return orig_method_1339(textureInfo);
				} catch(Exception e) {
					if(!e.Message.StartsWith("Texture file not found!"))
						throw;
				} finally {
					textureInfo.field_2062 = origPath;
				}
			}
			textureInfo.field_2062 = origPath;
		}
		return orig_method_1339(textureInfo);
	}
}