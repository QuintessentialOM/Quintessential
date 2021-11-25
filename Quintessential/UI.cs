namespace Quintessential {

	using OMDraw = class_135;
	using OMUI = class_140;
	using OMAssets = class_238;

	public static class UI {

		// Not sure what a few of these do

		#region Drawing methods

		public static void DrawRepeatingTexture(class_256 texture, Vector2 pos, Vector2 size) {
			OMDraw.method_268(texture, Color.White, pos, Bounds2.WithSize(pos, size));
		}

		public static void DrawResizableTexture(class_256 texture, Vector2 pos, Vector2 size) {
			OMDraw.method_276(texture, Color.White, pos, size);
		}

		public static void DrawHeader(string text, Vector2 pos, int width, bool a, bool b) {
			OMUI.method_317(class_134.method_253(text, string.Empty), pos, width, true, true);
		}

		public static bool DrawAndCheckCloseButton(Vector2 framePos, Vector2 frameSize, Vector2 closeButtonOffset) {
			return OMUI.method_323(framePos, frameSize, frameSize - closeButtonOffset);
		}

		public static bool DrawAndCheckSolutionButton(string text, string subtext, Vector2 pos, int width, bool selected) {
			return OMUI.method_315(text, subtext == null ? struct_18.field_1431 : Maybe<string>.method_1089(subtext), pos, width, selected).method_824(true, true);
		}

		#endregion

		#region Screen stack

		public static void HandleCloseButton() {
			CloseScreen();
			// Play close sound
			OMAssets.field_1991.field_1873.method_28(1f);
		}

		public static void CloseScreen() {
			GameLogic.field_2434.field_2464 = false;
			GameLogic.field_2434.method_949();
		}

		public static void InstantCloseScreen() {
			GameLogic.field_2434.method_950(1);
		}

		public static void OpenScreen(IScreen toOpen) {
			GameLogic.field_2434.method_945(toOpen, struct_18.field_1431, struct_18.field_1431);
		}

		#endregion

		#region UI helpers

		public static void DrawUiBackground(Vector2 pos, Vector2 size) {
			DrawRepeatingTexture(OMAssets.field_1989.field_102.field_810, pos, size);
		}

		public static void DrawUiFrame(Vector2 pos, Vector2 size) {
			DrawResizableTexture(OMAssets.field_1989.field_102.field_817, pos, size);
		}

		#endregion
	}
}