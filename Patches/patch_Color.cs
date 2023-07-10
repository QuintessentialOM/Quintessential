public class patch_Color{

	public static Color FromInts(int r, int g, int b, float alpha) => new Color(r / 255f, g / 255f, b / 255f, alpha);
}