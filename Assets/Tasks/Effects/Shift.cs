﻿using System.Collections.Generic;
using UnityEngine;

public class PipeConstants
{
    public const string Nothing = "Nothing";
    public const string Content = "Content";
    public const string Pressure = "Pressure";
    public const string Suckable = "Suckable";
}

public class Shift : EffectBase
{
    private Pipe _pipe;

    public Shift()
    {
    }

    public new int Range = -1;

    public override bool DoEffect()
    {
        if (_pipe == null)
        {
            _pipe = AssignedEntity as Pipe;
        }

        if (_pipe == null)
        {
            return true;
        }

        if (Random.value > 0.9)
        {
            return true;
        }

        _pipe.Flow();
        return true;
    }

    
}