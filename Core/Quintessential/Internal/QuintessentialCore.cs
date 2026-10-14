using System;

namespace Quintessential.Internal;

internal class QuintessentialCore : QuintessentialMod {
    public static QuintessentialCore Instance { get; }
    public override string ModId => "quintessential_core";
    public override Type SettingsType => typeof(QuintessentialCoreSettings);

    public override void Load() {}

    public override void LoadContent() {
        QApi.AddSolutionPayloadHandler("Quintessential:place", (solution, data) => {
            string[] parameters = data.Split(',');
            if (parameters.Length != 4) {
                return;
            }
            HexIndex position = new(int.Parse(parameters[0]), int.Parse(parameters[1]));
            HexRotation rotation = new(int.Parse(parameters[2]));

            if (!patch_PartTypes.GetById((Identifier)parameters[3]).GetOrDefault(out PartType partType)) {
                partType = PartTypes.equilibriumGlyph;
            }

            Part part = new(partType, false);
            solution.RepositionPart(part, position);
            part.RotateBy(solution, rotation);
        });
    }
    public override void LoadCompatContent() { }
    public override void FinaliseContent() { }

    public override void PostLoad() { }
	public override void Unload() { }
}
