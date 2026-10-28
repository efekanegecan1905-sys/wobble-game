#ifndef WOBBLE_CONCEPT_SKY_INCLUDED
#define WOBBLE_CONCEPT_SKY_INCLUDED

// Shared painterly sky used by the skybox and by every distant stone surface.
// One function keeps the horizon perfectly continuous between sky and ruins.
float WobbleHash(float2 p)
{
    p = frac(p * float2(127.31, 311.71));
    p += dot(p, p + 34.23);
    return frac(p.x * p.y);
}

float WobbleCloud(float2 p, float2 centre)
{
    float2 q = (p - centre) / float2(.34, .085);
    float base = 1 - smoothstep(.55, 1, dot(q, q));
    float2 a = (p - centre - float2(-.11, .05)) / float2(.15, .095);
    float2 b = (p - centre - float2(.08, .075)) / float2(.19, .12);
    return max(base, max(1 - smoothstep(.5, 1, dot(a, a)), 1 - smoothstep(.5, 1, dot(b, b))));
}

float3 WobbleSky(float3 d, float3 top, float3 horizon, float3 bottom, float3 accent,
                 float strength, float3 sunDir, float3 sunColor, float sunIntensity,
                 float3 cloudTint, float cloudStrength, float stars)
{
    d = normalize(d);
    float y = d.y;

    // Three-stop gradient tuned for a downward gameplay camera: the colour seen
    // through most of the frame is the "bottom" stop, while the pale horizon only
    // occupies a thin band at the top of the screen.
    float3 sky = lerp(bottom, horizon, smoothstep(-.34, .10, y));
    sky = lerp(sky, top, smoothstep(.06, .78, y));
    float band = exp(-pow((y + .02) * 7.0, 2));
    sky = lerp(sky, horizon, band * .30);

    // Sun disc plus a wide, gentle halo.
    float sun = saturate(dot(d, normalize(sunDir)));
    float disc = smoothstep(.9988, .9995, sun);
    float halo = pow(sun, 48) * .42 + pow(sun, 7) * .12;
    sky += sunColor * ((disc * 1.35 + halo) * sunIntensity);

    // Two cloud decks drifting at different speeds keep the sky alive.
    float2 p = float2(atan2(d.z, d.x), y);
    float upper = max(WobbleCloud(p, float2(-2.1, -.22)), max(WobbleCloud(p, float2(-.8, -.46)), WobbleCloud(p, float2(1.5, -.28))));
    float2 q = p * 1.7 + float2(11.3 + _Time.y * .004, 2.7);
    float lower = max(WobbleCloud(q, float2(-2.0, -.30)), max(WobbleCloud(q, float2(-.6, -.52)), WobbleCloud(q, float2(1.7, -.34))));
    float cloud = saturate(upper * .86 + lower * .58);
    // A distant bank hugging the horizon; the play plane already has its cloud sea.
    float deck = smoothstep(-.46, -.18, y) * smoothstep(.10, -.02, y);
    float cloudMask = cloud * deck * cloudStrength;
    // Warm rim on the sun side of the clouds.
    float3 litCloud = lerp(cloudTint, sunColor, pow(sun, 4) * .35);
    sky = lerp(sky, litCloud, cloudMask * .46);

    // Accent veil just above the horizon: the signature concept-art glow.
    float veil = smoothstep(.55, .95, sin(d.x * 3 + d.z * 2 + _Time.y * .025) * .5 + .5) * exp(-pow((d.y + .34) * 5, 2));
    sky = lerp(sky, accent, veil * strength);

    // Stars for the cosmic destinations. Cheap grid hash, no texture reads.
    if (stars > .001)
    {
        float2 sp = float2(atan2(d.z, d.x) * 26.0, y * 54.0);
        float2 cell = floor(sp);
        float seed = WobbleHash(cell);
        float sparkle = step(.938, seed) * smoothstep(.15, 0.0, length(frac(sp) - .5));
        float twinkle = .65 + .35 * sin(_Time.y * 2.2 + seed * 41.0);
        sky += sunColor * sparkle * twinkle * stars * 1.60 * smoothstep(-.55, -.05, y);
    }

    return sky;
}
#endif
