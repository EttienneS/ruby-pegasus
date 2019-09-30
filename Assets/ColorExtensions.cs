﻿using System;
using UnityEngine;
using Random = UnityEngine.Random;

public static class ColorExtensions
{
    public static Color GetRandomColor(float alpha = 1.0f)
    {
        return new Color(Random.value, Random.value, Random.value, alpha);
    }

    

    public static Color ToColor(this float[] arr)
    {
        return new Color(arr[0], arr[1], arr[2], arr[3]);
    }

    public static float[] ToFloatArray(this Color color)
    {
        return new[] { color.r, color.g, color.b, color.a };
    }

    internal static Color GetColorFromHex(string hexString)
    {
        Color col;

        if (ColorUtility.TryParseHtmlString(hexString, out col))
        {
            return col;
        }

        throw new Exception("Unable to parse color");
    }

    internal static Color GetRandomHairColor()
    {
        return ColorConstants.HairArray[Random.Range(0, ColorConstants.HairArray.Length - 1)];
    }

    internal static Color GetRandomSkinColor()
    {
        return ColorConstants.SkinArray[Random.Range(0, ColorConstants.SkinArray.Length - 1)];
    }
}