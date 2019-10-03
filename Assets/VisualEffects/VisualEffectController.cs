﻿using UnityEngine;

public class VisualEffectController : MonoBehaviour
{
    public VisualEffect EffectPrefab;

    public Badge BadgePrefab;

    public VisualEffect SpriteEffectPrefab;

    public void SpawnEffect(Cell cell, float lifeSpan)
    {
        var effect = Instantiate(EffectPrefab, transform);
        effect.LifeSpan = lifeSpan;
        effect.transform.position = cell.ToTopOfMapVector();
    }

    public VisualEffect SpawnSpriteEffect(Cell cell, string sprite, float lifeSpan)
    {
        var effect = Instantiate(SpriteEffectPrefab, transform);
        effect.Sprite.sprite = Game.SpriteStore.GetSprite(sprite);
        effect.LifeSpan = lifeSpan;
        effect.transform.position = cell.ToTopOfMapVector();

        return effect;
    }

    public Badge Spawn()
    {
        return Instantiate(BadgePrefab, transform);
    }

    internal Badge AddBadge(IEntity entity, string iconName)
    {
        var badge = Spawn();
        badge.SetSprite(iconName);
        badge.Follow(entity);
        return badge;
    }

    internal Badge AddBadge(Cell cell, string iconName)
    {
        var badge = Spawn();
        badge.SetSprite(iconName);
        badge.transform.position = cell.ToTopOfMapVector();

        return badge;
    }
}