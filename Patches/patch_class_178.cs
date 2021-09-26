using Quintessential;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class patch_class_178 {

	public extern void orig_method_50(float param_3782);

	public void method_50(float param_3782) {
		if(GameLogic.field_2434.method_938() is ModsScreen)
			return;
		orig_method_50(param_3782);
		float num = 65f;
		Vector2 vector2_1 = new Vector2(570f, 440f);
		Vector2 vector2_2 = (class_115.field_1433 / 2 - vector2_1 / 2).Rounded();
		Vector2 vector2_3 = new Vector2(161f, 256f);
		Vector2 vector2_4 = vector2_3 + new Vector2(0.0f, -num);
		Vector2 vector2_5 = vector2_4 + new Vector2(0.0f, -num);
		Vector2 vector2_6 = vector2_5 + new Vector2(0.0f, -num * 2);
		if(class_140.method_314(class_134.method_253("Mods", string.Empty), vector2_2 + vector2_6).method_824(true, true)) {
			// show mod options
			GameLogic.field_2434.method_945(new ModsScreen(), struct_18.field_1431, struct_18.field_1431);
		}
	}
}

