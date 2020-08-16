﻿using Assets.Item;
using Assets.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    private Dictionary<string, string> _itemTypeFileMap;
    private Dictionary<string, ItemData> _itemDataReference;

    internal Dictionary<string, ItemData> ItemDataReference
    {
        get
        {
            ItemTypeFileMap.First();
            return _itemDataReference;
        }
    }

    internal List<FilterViewOption> GetAllItemOptions()
    {
        var options = new List<FilterViewOption>();
        foreach (var item in ItemDataReference.Values)
        {
            options.Add(new FilterViewOption(item.Name, Game.Instance.SpriteStore.GetSprite(item.Icon), item.Categories));
        }
        return options;
    }

    internal Dictionary<string, string> ItemTypeFileMap
    {
        get
        {
            if (_itemTypeFileMap == null)
            {
                _itemTypeFileMap = new Dictionary<string, string>();
                _itemDataReference = new Dictionary<string, ItemData>();
                foreach (var itemFile in Game.Instance.FileController.ItemFiles)
                {
                    try
                    {
                        var data = ItemData.GetFromJson(itemFile.text);
                        _itemTypeFileMap.Add(data.Name, itemFile.text);
                        _itemDataReference.Add(data.Name, data);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Unable to load item {itemFile}: {ex.Message}");
                    }
                }
            }
            return _itemTypeFileMap;
        }
    }

    public ItemData SpawnItem(string name, Cell cell, int amount = 1)
    {
        if (!ItemTypeFileMap.ContainsKey(name))
        {
            Debug.LogError($"Item not found: {name}");
        }

        var itemToMerge = cell.Items.FirstOrDefault(i => i.Name == name);
        if (itemToMerge != null)
        {
            itemToMerge.Amount += amount;
            return itemToMerge;
        }
        else
        {
            var data = ItemData.GetFromJson(ItemTypeFileMap[name]);
            data.Cell = cell;
            data.Amount = amount;

            SpawnItem(data);
            return data;
        }
    }

    internal void DestroyItem(ItemData item)
    {
        if (item != null)
        {
            Game.Instance.IdService.RemoveEntity(item);
            Game.Instance.AddItemToDestroy(item.Renderer.gameObject);
        }
    }

    private void IndexItem(ItemData item)
    {
        Game.Instance.IdService.EnrollEntity(item);
    }

    internal void SpawnItem(ItemData data)
    {
        var mesh = Game.Instance.MeshRendererFactory
                                .GetItemMesh(data.Mesh);

        var renderer = Instantiate(mesh, transform).gameObject
                                                   .AddComponent<ItemRenderer>();

        renderer.Data = data;
        data.Renderer = renderer;

        IndexItem(data);

        data.Cell = Map.Instance.GetCellAtCoordinate(new Vector3(data.Coords.X, 0, data.Coords.Z));
        renderer.UpdatePosition();
    }
}