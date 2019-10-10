﻿using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;
using Random = UnityEngine.Random;

public class CreatureRenderer : MonoBehaviour
{
    internal CreatureData Data = new CreatureData();
    internal SpriteRenderer Highlight;
    internal LineRenderer LineRenderer;
    internal float RemainingTextDuration;
    internal SpriteRenderer MainRenderer;
    internal SpriteRenderer HairRenderer;
    internal SpriteRenderer FaceRenderer;
    internal SpriteRenderer TopRenderer;
    internal SpriteRenderer BottomRenderer;
    internal SpriteRenderer BodyRenderer;
    internal float TempMaterialDelta;
    internal float TempMaterialDuration;
    internal TextMeshPro Text;

    public void Awake()
    {
        Highlight = transform.Find("Highlight").GetComponent<SpriteRenderer>();
        var mainSprite = transform.Find("Sprite");
        MainRenderer = mainSprite.GetComponent<SpriteRenderer>();

        FaceRenderer = mainSprite.Find("Face").GetComponent<SpriteRenderer>();
        HairRenderer = mainSprite.Find("Hair").GetComponent<SpriteRenderer>();
        TopRenderer = mainSprite.Find("Top").GetComponent<SpriteRenderer>();
        BottomRenderer = mainSprite.Find("Bottom").GetComponent<SpriteRenderer>();
        BodyRenderer = mainSprite.Find("Body").GetComponent<SpriteRenderer>();

        Text = GetComponentInChildren<TextMeshPro>();

        LineRenderer = GetComponent<LineRenderer>();

        Highlight.gameObject.SetActive(false);
        Light.gameObject.SetActive(false);
    }

    public void DrawAwareness()
    {
        var awareness = new List<Vector3> { Data.Cell.ToTopOfMapVector() };
        awareness.AddRange(Data.Awareness.Where(c => c.Neighbors.Any(n => !Data.Awareness.Contains(n)))
                                         .Select(c => c.ToTopOfMapVector()));
        awareness.Add(Data.Cell.ToTopOfMapVector());

        ShowLine(Color.white, awareness.ToArray());
    }

    public Sprite GetIcon()
    {
        return MainRenderer.sprite;
    }

    public void HideLine()
    {
        LineRenderer.positionCount = 0;
    }

    public void ShowLine(Color color, params Vector3[] points)
    {
        LineRenderer.startColor = color;
        LineRenderer.endColor = color;

        LineRenderer.SetPositions(points);
        LineRenderer.positionCount = points.Length;
    }

    public void ShowText(string text, float duration)
    {
        Text.text = text;
        Text.color = Color.white;

        RemainingTextDuration = duration;
    }

    public void Start()
    {
        // disable other sprites
        if (!Data.Sprite.Contains("_"))
        {
            FaceRenderer.sprite = null;
            BodyRenderer.sprite = null;
            TopRenderer.sprite = null;
            BottomRenderer.sprite = null;
            HairRenderer.sprite = null;
        }
        else
        {
            MainRenderer.sprite = null;
        }
    }

    public void Update()
    {
        UpdateFloatingText();

        if (Data.Update(Time.deltaTime))
        {
            //if (Highlight.gameObject.activeInHierarchy)
            //{
            //    DrawAwareness();
            //}
            //else
            //{
            //    HideLine();
            //}
        }
        UpdateMaterial();
    }

    internal void DisableHightlight()
    {
        if (Highlight != null && Highlight.gameObject != null)
        {
            Highlight.gameObject.SetActive(false);
        }
    }

    internal void DisplayChannel(ManaColor color, float duration)
    {
        var col = color.GetActualColor();
        MainRenderer.material = Game.MaterialController.GetChannelingMaterial(col);
        TempMaterialDuration = duration;
        TempMaterialDelta = 0;
    }

    internal void EnableLight()
    {
        if (Light != null)
        {
            Light.gameObject.SetActive(true);
        }
    }

    public Light2D Light;

    internal void EnableHighlight(Color color)
    {
        if (Highlight != null)
        {
            Highlight.color = color;
            Highlight.gameObject.SetActive(true);
        }
    }

    private void UpdateFloatingText()
    {
        if (RemainingTextDuration > 0)
        {
            RemainingTextDuration -= Time.deltaTime;

            if (RemainingTextDuration < 1f)
            {
                Text.color = new Color(Text.color.r, Text.color.g, Text.color.b, RemainingTextDuration);
            }
        }
        else
        {
            Text.text = "";
        }
    }

    private void UpdateMaterial()
    {
        var totalMana = Data.ManaPool.Sum(t => t.Value.Total);
        //Light.volumeOpacity = totalMana / 100;
        Light.pointLightOuterRadius = totalMana / 50.0f;
        Light.pointLightInnerRadius = Light.pointLightOuterRadius / 2.0f;
        Light.intensity = Light.pointLightOuterRadius;
        Light.color = Data.ManaPool.GetManaWithMost().GetActualColor();

        if (TempMaterialDuration <= 0)
            return;

        TempMaterialDelta += Time.deltaTime;
        if (TempMaterialDelta >= TempMaterialDuration)
        {
            TempMaterialDuration = 0;
            MainRenderer.SetBoundMaterial(Data.Cell);
        }

    }
}