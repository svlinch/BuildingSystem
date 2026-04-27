float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
}

float noise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);

    return lerp(
        lerp(hash(i + float2(0, 0)), hash(i + float2(1, 0)), f.x),
        lerp(hash(i + float2(0, 1)), hash(i + float2(1, 1)), f.x),
        f.y
    );
}

void GetInstanceData_float(float InID, float Time, float3 WorldPos, out float3 Color, out float AlphaOut)
{
    Color = float3(1, 1, 1);
    AlphaOut = 1.0;

#if defined(UNITY_INSTANCING_ENABLED)

    unity_InstanceID = uint(InID); 
    float4x4 mat = GetObjectToWorldMatrix();

    float deathTime  = mat._m30;
    float instant    = mat._m31;
    float createTime = mat._m32;

    float fadeInDuration  = 0.5;
    float fadeOutDuration = 0.5;

    // появление
    float appear = saturate((Time - createTime) / fadeInDuration);

    // исчезновение
    float disappear = 1.0;

    if (deathTime > 0.01)
    {
        float t = Time - deathTime;
        disappear = 0.65 - saturate(t / fadeOutDuration);
    }

    float visibility = appear * disappear;

    // шум
    float n = noise(WorldPos.xz * 10.0 + WorldPos.y * 10.0);

    // финальная альфа
    AlphaOut = saturate(visibility + n * 0.5);

    // если нужно мгновенное удаление
    if (instant > 0.5 && Time > deathTime)
    {
        AlphaOut = 0;
    }

#endif
}