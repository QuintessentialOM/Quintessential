using MonoMod;
using Quintessential;
using AtomTypes = class_175;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
[MonoModPatch("class_175")]
class patch_AtomTypes {

	public static extern void orig_method_248();

	public static void method_248() {
		orig_method_248();
		((patch_AtomType)(object)AtomTypes.field_1675).QuintAtomType = "salt";
		((patch_AtomType)(object)AtomTypes.field_1676).QuintAtomType = "air";
		((patch_AtomType)(object)AtomTypes.field_1677).QuintAtomType = "earth";
		((patch_AtomType)(object)AtomTypes.field_1678).QuintAtomType = "fire";
		((patch_AtomType)(object)AtomTypes.field_1679).QuintAtomType = "water";
		((patch_AtomType)(object)AtomTypes.field_1680).QuintAtomType = "quicksilver";
		((patch_AtomType)(object)AtomTypes.field_1681).QuintAtomType = "lead";
		((patch_AtomType)(object)AtomTypes.field_1682).QuintAtomType = "copper";
		((patch_AtomType)(object)AtomTypes.field_1683).QuintAtomType = "tin";
		((patch_AtomType)(object)AtomTypes.field_1684).QuintAtomType = "iron";
		((patch_AtomType)(object)AtomTypes.field_1685).QuintAtomType = "silver";
		((patch_AtomType)(object)AtomTypes.field_1686).QuintAtomType = "gold";
		((patch_AtomType)(object)AtomTypes.field_1687).QuintAtomType = "vitae";
		((patch_AtomType)(object)AtomTypes.field_1688).QuintAtomType = "mors";
		((patch_AtomType)(object)AtomTypes.field_1689).QuintAtomType = "repeat";
		((patch_AtomType)(object)AtomTypes.field_1690).QuintAtomType = "quintessence";
	}
}