using Quintessential;
using System;
using System.IO;

class patch_Renderer {

	public static extern bool orig_method_1343(class_256 param_5118);

	public static bool method_1343(class_256 textureInfo) {
		if(textureInfo.field_2062.method_1085() /*Exists*/ && textureInfo.field_2062.method_1087()/*Get*/.StartsWith("Content")) {
			Maybe<string> origPath = textureInfo.field_2062;
			foreach(var dir in QuintessentialLoader.ModContentDirectories) {
				try {
					textureInfo.field_2062 = Path.Combine(dir, origPath.method_1087());
					return orig_method_1343(textureInfo);
				} catch(Exception e) {
					if(!e.Message.StartsWith("Texture file not found!"))
						throw;
				} finally {
					textureInfo.field_2062 = origPath;
				}
			}
			textureInfo.field_2062 = origPath;
		}
		return orig_method_1343(textureInfo);
	}
}