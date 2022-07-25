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

			GCHandle bufferHandle = GCHandle.Alloc(buffer);
			IntPtr bufferPointer = (IntPtr)bufferHandle;

			IntPtr sdlRenderer = SDL.SDL_GetRenderer(GameLogic.field_2434.field_2437.field_2192);
			SDL.SDL_RenderReadPixels(sdlRenderer, ref rect, SDL.SDL_PIXELFORMAT_ARGB8888, bufferPointer, rect.w * 4);

			var surface = SDL.SDL_CreateRGBSurfaceFrom(bufferPointer, rect.w, rect.h, 8 * 4, rect.w * 4, 0xFF0000, 0xFF00, 0xFF, 0xFF000000);
			Logger.Log("created surface!");
			SDL.SDL_SaveBMP(surface, Path.Combine(QuintessentialLoader.PathScreenshots, "screenshot-" + 1 + ".bmp"));
			Logger.Log("saved screenshot!");

			SDL.SDL_FreeSurface(surface);
			bufferHandle.Free();
		}
	}
}