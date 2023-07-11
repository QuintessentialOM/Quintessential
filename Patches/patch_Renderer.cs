using Quintessential;
using System;
using System.IO;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

class patch_Renderer {

	// checks mods for textures before vanilla

	public static extern bool orig_method_1339(class_256 param_5118);

	public static bool method_1339(class_256 textureInfo){
		string origPath = null;
		if(textureInfo.field_2062.method_1085() /*Exists*/){
			origPath = textureInfo.field_2062.method_1087();
			if(textureInfo.field_2062.method_1087() /*Get*/.StartsWith("Content")) {
				for(int i = QuintessentialLoader.ModContentDirectories.Count - 1; i >= 0; i--){
					string dir = QuintessentialLoader.ModContentDirectories[i];
					try{
						textureInfo.field_2062 = Path.Combine(dir, origPath);
						return orig_method_1339(textureInfo);
					}catch(Exception e){
						HandleException(e);
					}finally {
						textureInfo.field_2062 = origPath;
					}
				}
			}
		}
		try{
			return orig_method_1339(textureInfo);
		}catch(Exception e) {
			HandleException(e);
		}
		// none match -> use missing texture
		try{
			Logger.Log($"Texture {origPath} does not exist, using fallback texture");
			textureInfo.field_2062 = Path.Combine(QuintessentialLoader.ModContentDirectories[0], "Content", "Quintessential", "missing");
			return orig_method_1339(textureInfo);
		}finally{
			textureInfo.field_2062 = origPath;
		}
	}

	private static void HandleException(Exception e){
		if(e.Message.StartsWith("Texture file not found!"))
			return;
		if(e is class_266 && e.Message.StartsWith("Failed to load PNG file:"))
			throw new Exception($"SDL failed to load a PNG: \"{SDL2.SDL.SDL_GetError()}\"", e);
		throw e;
	}
}