﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainBlock : MonoBehaviour
{
    public SpriteRenderer Renderer;
    public BoxCollider2D Collider;

    void Awake()
    {
        Renderer = GetComponent<SpriteRenderer>();
        Collider = GetComponent<BoxCollider2D>();

        Collider.size = new Vector2(Constants.CellsPerTerrainBlock, Constants.CellsPerTerrainBlock);
        Collider.offset = new Vector2(Constants.CellsPerTerrainBlock / 2, Constants.CellsPerTerrainBlock / 2);
    }


}
