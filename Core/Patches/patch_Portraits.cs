using MonoMod;
using Quintessential;
using System.Collections.Generic;
using System.IO;

internal class patch_Portraits {
    [MonoModIgnore] public static Dictionary<string, patch_Portrait> portraits;
    public static void CreateRescaledVersions() {
        portraits["Mannequin"].smallTexture ??=  ScaleDown(Assets.textures.portraits.anataeus_alt_large);
        portraits["Armand Van Tassen"].smallTexture ??= AssetLoaderHelper.LoadTexture("textures/portraits/armand_small"); //ScaleDown(Assets.textures.portraits.armand_large);
        portraits["Verrin Ravari"].smallTexture ??= AssetLoaderHelper.LoadTexture("textures/portraits/verrin_small"); //ScaleDown(Assets.textures.portraits.verrin_large);
        portraits["Verrin Ravari (Shabby)"].smallTexture ??= AssetLoaderHelper.LoadTexture("textures/portraits/verrin_shabby_small"); //ScaleDown(Assets.textures.portraits.verrin_shabby_large);
        portraits["Taros Colvan"].smallTexture ??= AssetLoaderHelper.LoadTexture("textures/portraits/taros_small"); //ScaleDown(Assets.textures.portraits.taros_large);
    }

    private static Texture ScaleDown(Texture texture) {
        Texture created = AssetLoaderHelper.LoadTexture(Path.GetRelativePath("Content", texture.sourceFile.GetValue()));
        UI.AssignScale(created, Assets.textures.portraits.anataeus_small.size.ToVector2() / 2);
        return created;
    }
}
