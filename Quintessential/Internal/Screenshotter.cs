using System;
using System.IO;
using System.Runtime.InteropServices;

using SDL2;

namespace Quintessential.Internal;

internal class Screenshotter{

	public static void CheckScreenshot(){
		if(true)
			return; // TODO

		if(QuintessentialSettings.Instance.Screenshot.Pressed()){
			var size = Input.ScreenSize();
			SDL.SDL_Rect rect = new(){
				w = (int)Math.Ceiling(size.X),
				h = (int)Math.Ceiling(size.Y),
				x = 0,
				y = 0
			};
			var buffer = new byte[rect.x * rect.y * 4];

			unsafe{
				fixed(byte* pixels = buffer){
					IntPtr pixPtr = new IntPtr(pixels);
					//IntPtr sdlRenderer = SDL.SDL_GetRenderer(GameLogic.field_2434.field_2437.field_2192);
					//SDL.SDL_RenderReadPixels(sdlRenderer, ref rect, SDL.SDL_PIXELFORMAT_ABGR8888, pixPtr, rect.w * 4);
					IntPtr surface = SDL.SDL_CreateRGBSurfaceFrom(pixPtr, rect.w, rect.h, 32, rect.w * 4, 0xFF, 0xFF00, 0xFF0000, 0xFF000000);
					if(surface.Equals(IntPtr.Zero))
						throw new Exception($"Failed to create surface! \"{SDL.SDL_GetError()}\"");
					Logger.Log("Created surface!");
					
					var result = class_267.method_726(surface, Path.Combine(QuintessentialLoader.PathScreenshots, "screenshot-" + 1 + ".png"));
					if(result != 0)
						throw new Exception($"Failed to save screenshot! \"{SDL.SDL_GetError()}\"");
					Logger.Log("Took screenshot!");
					
					SDL.SDL_FreeSurface(surface);
				}
			}
		}
	}
}