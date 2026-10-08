using System;
using MonoMod;
using Quintessential;

[MonoModPatch("InstructionType")]
class patch_InstructionType{

    [MonoModInternalF]
    [Obsolete("This shouldn't be used. Use `Id` instead.")]
    public char id;

    public Identifier Id {
        get => id switch {
            'O' => "om:period_override",
            'I' => "om:idle",
            'i' => "om:idle_placeholder",
            'A' => "om:move_plus",
            'a' => "om:move_minus",
            'R' => "om:rotate_right",
            'r' => "om:rotate_left",
            'E' => "om:extend",
            'e' => "om:retract",
            'P' => "om:pivot_right",
            'p' => "om:pivot_left",
            'G' => "om:grab",
            'g' => "om:drop",
            'X' => "om:reset",
            'C' => "om:repeat",
            'B' => "om:halt",
            _ => field,
        }; set;
    }
}