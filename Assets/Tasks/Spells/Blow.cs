﻿using System;

public class Blow : SpellBase
{
    public Blow()
    {
    }

    public override bool DoSpell()
    {
        Pipe fromPipe;
        ManaPool toPool;

        var fromCell = Structure.Cell.GetNeighbor(Structure.Rotation);

        if (fromCell?.Structure?.IsPipe() == true)
        {
            var linkedPipe = fromCell.Structure as Pipe;

            if (linkedPipe.Attunement.HasValue)
            {
                fromPipe = linkedPipe;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }

        var toCell = Structure.Cell.GetNeighbor(Structure.Rotation.Opposite());
        if (toCell?.Structure?.IsBluePrint == false)
        {
            toPool = toCell.Structure.ManaPool;
        }
        else
        {
            return false;
        }

        fromPipe.ManaPool.Transfer(toPool, fromPipe.Attunement.Value, 1);

        if (fromPipe.ManaPool[fromPipe.Attunement.Value].Total <= 0)
        {
            fromPipe.Attunement = null;
        }

        return true;
    }
}