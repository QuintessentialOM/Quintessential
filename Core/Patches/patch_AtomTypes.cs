#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Quintessential;
using System.Linq;

class patch_AtomTypes{

	public static AtomType GetByID(Identifier id) {
		return AtomTypes.atoms.First(atom => ((patch_AtomType)(object)atom).Id == id);
	}

	public static extern void orig_Init();
	public static void Init(){
        orig_Init();
		((patch_AtomType)(object)AtomTypes.salt).Id = "om:salt";
		((patch_AtomType)(object)AtomTypes.air).Id = "om:air";
		((patch_AtomType)(object)AtomTypes.earth).Id = "om:earth";
		((patch_AtomType)(object)AtomTypes.fire).Id = "om:fire";
		((patch_AtomType)(object)AtomTypes.water).Id = "om:water";
		((patch_AtomType)(object)AtomTypes.quicksilver).Id = "om:quicksilver";
		((patch_AtomType)(object)AtomTypes.lead).Id = "om:lead";
		((patch_AtomType)(object)AtomTypes.copper).Id = "om:copper";
		((patch_AtomType)(object)AtomTypes.tin).Id = "om:tin";
		((patch_AtomType)(object)AtomTypes.iron).Id = "om:iron";
		((patch_AtomType)(object)AtomTypes.silver).Id = "om:silver";
		((patch_AtomType)(object)AtomTypes.gold).Id = "om:gold";
		((patch_AtomType)(object)AtomTypes.vitae).Id = "om:vitae";
		((patch_AtomType)(object)AtomTypes.mors).Id = "om:mors";
		((patch_AtomType)(object)AtomTypes.repeat).Id = "om:repeat";
		((patch_AtomType)(object)AtomTypes.quintessence).Id = "om:quintessence";
	}
}