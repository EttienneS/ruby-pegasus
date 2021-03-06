﻿using Assets.ServiceLocator;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class VisualEffect : MonoBehaviour
{
    public VisualEffectData Data;

    public Light Light;

    public ParticleSystem ParticleSystem;

    public SpriteRenderer Sprite;

    public void Start()
    {
        var lightObject = transform.Find("Light").gameObject;
        var spriteObject = transform.Find("Sprite").gameObject;
        var particleObject = transform.Find("Particle").gameObject;

        if ((Data.EffectType & EffectType.Light) == EffectType.Light)
        {
            Light = lightObject.GetComponent<Light>();
        }
        else
        {
            lightObject.SetActive(false);
        }

        if ((Data.EffectType & EffectType.Sprite) == EffectType.Sprite)
        {
            Sprite = spriteObject.GetComponent<SpriteRenderer>();
        }
        else
        {
            spriteObject.SetActive(false);
        }

        if ((Data.EffectType & EffectType.Particle) == EffectType.Particle)
        {
            ParticleSystem = particleObject.GetComponent<ParticleSystem>();
        }
        else
        {
            particleObject.SetActive(false);
        }

        if (Data.Properties.ContainsKey("Sprite"))
        {
            Sprite.sprite = Loc.GetSpriteStore().GetSprite(Data.GetProperty("Sprite"));
        }
        if (Data.Properties.ContainsKey("Color"))
        {
            Sprite.color = Data.GetProperty("Color").GetColorFromHex();
        }

        var x = float.Parse(Data.GetProperty("X"));
        var y = float.Parse(Data.GetProperty("Y"));
        var z = float.Parse(Data.GetProperty("Z"));
        transform.position = new Vector3(x, y, z);
    }

    internal void DestroySelf()
    {
        Data.Destroyed = true;
        Destroy(gameObject);
    }

    internal void Fades(bool fadeOut = false)
    {
        Data.TimeAlive = 0;
        Data.Fade = true;
        Data.FadeOut = fadeOut;
    }

    internal void Kill()
    {
        Data.LifeSpan = 0;
    }

    internal void Regular()
    {
        transform.localScale = new Vector3(1, 1, 1);
    }

    internal void Tiny()
    {
        transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
    }

    private void Update()
    {
        if (Loc.GetTimeManager().Paused)
            return;

        if (Data.FullSpan < 0)
        {
            Data.FullSpan = Data.LifeSpan;
        }

        if (Data.StartIntensity < 0)
        {
            Data.StartIntensity = Data.Intensity;
        }
        Data.TimeAlive += Time.deltaTime;
        Data.LifeSpan -= Time.deltaTime;
        if (Data.LifeSpan <= 0)
        {
            DestroySelf();
            return;
        }

        float t = Data.TimeAlive / Data.LifeSpan;
        var step = Data.FadeOut ? Mathf.SmoothStep(0, Data.FullSpan, t) : Mathf.SmoothStep(Data.FullSpan, 0, t);

        if (Sprite != null)
        {
            if (Data.Fade)
            {
                Sprite.color = new Color(Sprite.color.r,
                                         Sprite.color.g,
                                         Sprite.color.b,
                                         step);
            }
        }

        if (Light != null)
        {
            if (Data.Fade)
            {
                Light.intensity = Data.Intensity * (Data.LifeSpan / Data.FullSpan);
            }
        }
    }
}

public class VisualEffectData
{
    public bool FadeOut;

    public float FullSpan = -1;
    public float LifeSpan;
    public float TimeAlive;
    private VisualEffect _linkedGameObject;
    public bool Destroyed { get; set; }
    public EffectType EffectType { get; set; }
    public bool Fade { get; set; }

    public float Intensity { get; set; } = -1;

    [JsonIgnore]
    public VisualEffect LinkedGameObject
    {
        get
        {
            if (_linkedGameObject == null)
            {
                _linkedGameObject = Loc.GetVisualEffectController().SpawnEffect(this);
            }
            return _linkedGameObject;
        }
        set
        {
            _linkedGameObject = value;
        }
    }

    public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    public float StartIntensity { get; set; } = -1;

    internal string GetProperty(string key)
    {
        return Properties[key];
    }

    internal void SetProperty(string key, string value)
    {
        if (Properties.ContainsKey(key))
        {
            Properties[key] = value;
        }
        else
        {
            Properties.Add(key, value);
        }
    }
}