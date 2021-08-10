using MonoMod;
using Quintessential;
using System;
using System.IO;

class patch_Renderer {

	public static extern bool orig_method_1343(class_66 param_5118);

	public static bool method_1343(class_66 textureInfo) {
		if(textureInfo.field_810.method_1089() /*Exists*/ && textureInfo.field_810.method_1091()/*Get*/.StartsWith("Content")) {
			Maybe<string> origPath = textureInfo.field_810;
			foreach(var dir in QuintessentialLoader.ModContentDirectories) {
				try {
					textureInfo.field_810 = Path.Combine(dir, origPath.method_1091());
					return orig_method_1343(textureInfo);
				} catch(Exception e) {
					if(!e.Message.StartsWith("Texture file not found!"))
						throw;
				} finally {
					textureInfo.field_810 = origPath;
				}
			}
			textureInfo.field_810 = origPath;
		}
		return orig_method_1343(textureInfo);
	}
}