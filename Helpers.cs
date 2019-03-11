﻿using System.Collections.Generic;
using System.Globalization;
using Random = UnityEngine.Random;

public static class Helpers
{
    public static List<string> ParseItemString(string itemString)
    {
        //"10:Wood"
        //"1-3:Food"
        var items = new List<string>();
        var parts = itemString.Split(':');

        var type = parts[1];
        var countString = parts[0].Split('-');

        int count = 0;
        if (countString.Length > 1)
        {
            var min = int.Parse(countString[0]);
            var max = int.Parse(countString[1]);

            count = Random.Range(min, max);
        }
        else
        {
            count = int.Parse(countString[0]);
        }

        for (var i = 0; i < count; i++)
        {
            items.Add(type);
        }

        return items;
    }

    public static float GetValueFromFloatRange(string input)
    {
        var scaleStringParts = GlobalizeFloatString(input).Split('~');

        var scale = 1f;
        if (scaleStringParts.Length > 1)
        {
            var min = float.Parse(scaleStringParts[0]);
            var max = float.Parse(scaleStringParts[1]);

            scale = Random.Range(min, max);
        }

        return scale;
    }

    public static string GlobalizeFloatString(string input)
    {
        return input.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
    }
}