using System;
using System.Collections.Generic;
using MonoMod;
using Quintessential;

[MonoModPatch("PartType")]
class patch_PartType{
	
	// When non-null, the predicate is run on the puzzle's set of custom permissions to check that the part is allowed
	public Predicate<HashSet<Identifier>> CustomPermissionCheck;

	// When true, this part type can't be cloned or removed from the board, Akin to a conduit.
	public bool IsForced = false;

    [MonoModInternalF]
    [Obsolete("This shouldn't be used. Use `Id` instead.")]
    public string id;

    public Identifier Id {
        get => id switch {
            "input" => "om:input",
            "out-std" => "om:out_std",
            "out-rep" => "om:out_rep",
            "pipe" => "om:conduit",
            "arm1" => "om:arm",
            "arm2" => "om:bi_arm",
            "arm3" => "om:tri_arm",
            "arm6" => "om:hex_arm",
            "piston" => "om:piston",
            "claw-pivot" => "om:claw_pivot",
            "track" => "om:track",
            "baron" => "om:berlos_wheel",
            "ravari" => "om:ravaris_wheel",
            "bonder" => "om:bonder",
            "unbonder" => "om:unbonder",
            "bonder-speed" => "om:multi_bonder",
            "bonder-prisma" => "om:triplex_bonder",
            "glyph-calcification" => "om:calcification",
            "glyph-duplication" => "om:duplication",
            "glyph-projection" => "om:projection",
            "glyph-rejection" => "om:rejection",
            "glyph-purification" => "om:purification",
            "glyph-division" => "om:division",
            "glyph-proliferation" => "om:proliferation",
            "glyph-life-and-death" => "om:animismus",
            "glyph-disposal" => "om:disposal",
            "glyph-marker" => "om:equilibrium",
            "glyph-unification" => "om:unification",
            "glyph-dispersion" => "om:dispersion",
            _ => field,
        }; set;
    }
}