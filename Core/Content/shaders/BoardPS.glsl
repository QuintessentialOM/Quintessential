
void main()
{
    vec2 boardOffset = fragTexCoord1;
    vec2 lightPos = LightPos().xy / fragColor.a;
    vec2 lightVec = normalize(lightPos - fragPosition.xy);
    float lightStrength = 1.0 - saturate(distance(lightPos, fragPosition.xy) / LightRadius() * fragColor.a);

    vec2 baseTexCoord = (boardOffset + fragPosition.xy) * 0.001;
    vec4 base = texture(Texture0, baseTexCoord);

    vec4 left  = texture(Texture1, fragTexCoord0);
    vec4 top   = texture(Texture2, fragTexCoord0);
    vec4 right = texture(Texture3, fragTexCoord0);
    float mask = texture(Texture4, fragTexCoord0).r;

    // Get a directionally-lit hex edge texture by interpolating between several
    // directionally-lit versions of the texture:
    vec4 edge;
    if (lightVec.x > 0.0)
    {
        edge = lerp(top, right, lightVec.x);
    }
    else
    {
        edge = lerp(top, left, -lightVec.x);
    }
    // Make the hex seams less harsh:
    edge *= 0.8;

    // Composite the hex edges onto the base texture:
    Output.rgb = base.rgb * (1.0 - edge.a) + edge.rgb;
    // Apply lighting:
    Output.rgb = Overlay(Output.rgb, lerp(0.2, 1.1, lightStrength));
    // Apply the mask:
    Output.rgb *= mask;
    Output.a = mask;
}
