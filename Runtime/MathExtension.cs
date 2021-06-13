using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathExtension
{
    public static Vector3 NewX(this Vector3 source, float value)
    {
        source.x = value;
        return source;
    }

    public static Vector3 NewY(this Vector3 source, float value)
    {
        source.y = value;
        return source;
    }

    public static Vector3 NewZ(this Vector3 source, float value)
    {
        source.z = value;
        return source;
    }
    
    /// <summary> float 확장. 이 float이 range에 포함되어있으면 true. (include) </summary>
    public static bool IsInRange(this float source, in Vector2 range)
    {
        return range.x <= source && source <= range.y;
    }

    /// <summary> value가 beforeRange에서 갖던 비율 만큼 targetRange 범위에서 정의되는 값을 반환함.</summary>
    public static float Remap(this float value, in Vector2 beforeRange, in Vector2 targetRange, bool useClamp = true)
    {
        var ratio = (value - beforeRange.x) / (beforeRange.y - beforeRange.x);
        var result = (targetRange.y - targetRange.x) * ratio + targetRange.x;
        return useClamp ? Mathf.Clamp(result, targetRange.x, targetRange.y) : result;
    }
    
    /// <summary> value가 beforeRange에서 갖던 비율 만큼 targetRange 범위에서 정의되는 값을 반환함.</summary>
    public static float Remap(this float value, float beforeRangeMin, float beforeRangeMax, float targetRangeMin, float targetRangeMax, bool useClamp = true)
    {
        var ratio = (value - beforeRangeMin) / (beforeRangeMax - beforeRangeMin);
        var result = (targetRangeMax - targetRangeMin) * ratio + targetRangeMin;
        return useClamp ? Mathf.Clamp(result, targetRangeMin, targetRangeMax) : result;
    }
        
    /// <summary> value가 beforeRange에서 갖던 비율 만큼 0..1 범위에서 정의되는 값을 반환함.</summary>
    public static float RemapTo01(this float value, in Vector2 beforeRange, bool useClamp = true)
    {
        return Remap(value, beforeRange, Vector2.up, useClamp);
    }
        
    /// <summary> 0..1에서 정의된 value를 targetRange의 범위로 정의함</summary>
    public static float RemapFrom01(this float value, in Vector2 targetRange, bool useClamp = true)
    {
        return Remap(value, Vector2.up, targetRange, useClamp);
    }
}