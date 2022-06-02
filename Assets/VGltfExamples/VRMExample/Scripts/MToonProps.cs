using System.Collections.Generic;
using System.Linq;

namespace VGltfExamples.VRMExample
{
    static class MToonProps
    {
        public enum PropKind
        {
            Float,
            Color,
            Tex,
        }

        public readonly struct KV
        {
            public KV(string k, PropKind v)
            {
                Key = k;
                Value = v;
            }

            public readonly string Key;
            public readonly PropKind Value;
        }

        public static readonly KV[] Props = new KV[]
        {
            new KV("_Cutoff", PropKind.Float),
            new KV("_Color", PropKind.Color),
            new KV("_ShadeColor", PropKind.Color),
            new KV("_MainTex", PropKind.Tex),
            new KV("_ShadeTexture", PropKind.Tex),
            new KV("_BumpScale", PropKind.Float),
            new KV("_BumpMap", PropKind.Tex), // normal
            new KV("_ReceiveShadowRate", PropKind.Float),
            new KV("_ReceiveShadowTexture", PropKind.Tex),
            new KV("_ShadingGradeRate", PropKind.Float),
            new KV("_ShadingGradeTexture", PropKind.Tex),
            new KV("_ShadeShift", PropKind.Float),
            new KV("_ShadeToony", PropKind.Float),
            new KV("_LightColorAttenuation", PropKind.Float),
            new KV("_IndirectLightIntensity", PropKind.Float),

            new KV("_RimColor", PropKind.Color),
            new KV("_RimTexture", PropKind.Tex),
            new KV("_RimLightingMix", PropKind.Float),
            new KV("_RimFresnelPower", PropKind.Float),
            new KV("_RimLift", PropKind.Float),

            new KV("_SphereAdd", PropKind.Tex),
            new KV("_EmissionColor", PropKind.Color),
            new KV("_EmissionMap", PropKind.Tex),
            new KV("_OutlineWidthTexture", PropKind.Tex),
            new KV("_OutlineWidth", PropKind.Float),
            new KV("_OutlineScaledMaxDistance", PropKind.Float),
            new KV("_OutlineColor", PropKind.Color),
            new KV("_OutlineLightingMix", PropKind.Float),

            new KV("_UvAnimMaskTexture", PropKind.Tex),
            new KV("_UvAnimScrollX", PropKind.Float),
            new KV("_UvAnimScrollY", PropKind.Float),
            new KV("_UvAnimRotation", PropKind.Float),

            new KV("_MToonVersion", PropKind.Float),
            new KV("_DebugMode", PropKind.Float),
            new KV("_BlendMode", PropKind.Float),
            new KV("_OutlineWidthMode", PropKind.Float),
            new KV("_OutlineColorMode", PropKind.Float),
            new KV("_CullMode", PropKind.Float),
            new KV("_OutlineCullMode", PropKind.Float),
            new KV("_SrcBlend", PropKind.Float),
            new KV("_DstBlend", PropKind.Float),
            new KV("_ZWrite", PropKind.Float),
            new KV("_AlphaToMask", PropKind.Float),
        };

        static readonly Dictionary<string, PropKind> PropsMap = Props.ToDictionary(kv => kv.Key, kv => kv.Value);

        public static bool TryGetPropKind(string key, out PropKind value)
        {
            return PropsMap.TryGetValue(key, out value);
        }

        // https://docs.unity3d.com/ja/2019.4/Manual/SL-SubShaderTags.html
        public static readonly List<string> Tags = new List<string>
        {
            "RenderType",
        };
    }
}
