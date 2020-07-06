﻿using Assets.Creature.Behaviour;
using Assets.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FileController : MonoBehaviour
{
    internal TextAsset[] StructureJson;
    internal TextAsset[] CreatureFiles;
    internal TextAsset[] BiomeFiles;
    internal TextAsset[] ItemFiles;

    public MeshRenderer[] Meshes;
    public Material[] Materials;

    public Dictionary<string, TextAsset> ItemLookup;

    internal Dictionary<string, MeshRenderer> MeshLookup = new Dictionary<string, MeshRenderer>();
    internal Dictionary<string, Material> MaterialLookup = new Dictionary<string, Material>();

    public string StructureFolder = "Structures";
    public string ConstructFolder = "Constructs";
    public string CreatureFolder = "Creatures";
    public string BiomeFolder = "Biomes";
    public string ItemFolder = "Items";

    private Material _blueprintMaterial;
    public Material BlueprintMaterial
    {
        get
        {
            if (_blueprintMaterial == null)
            {
                _blueprintMaterial = GetMaterial("BlueprintMaterial");
            }
            return _blueprintMaterial;
        }
    }
    private Material _invalidBlueprintMaterial;
    public Material InvalidBlueprintMaterial
    {
        get
        {
            if (_invalidBlueprintMaterial == null)
            {
                _invalidBlueprintMaterial = GetMaterial("InvalidBlueprintMaterial");
            }
            return _invalidBlueprintMaterial;
        }
    }

    private List<Construct> _constructs;

    public List<Construct> Constructs
    {
        get
        {
            if (_constructs == null)
            {
                _constructs = new List<Construct>();
                foreach (var constructFile in Resources.LoadAll<TextAsset>(ConstructFolder))
                {
                    _constructs.Add(constructFile.text.LoadJson<Construct>());
                }
            }

            return _constructs;
        }
    }

    public void Awake()
    {
        StructureJson = Resources.LoadAll<TextAsset>(StructureFolder);
        CreatureFiles = Resources.LoadAll<TextAsset>(CreatureFolder);
        BiomeFiles = Resources.LoadAll<TextAsset>(BiomeFolder);
        ItemFiles = Resources.LoadAll<TextAsset>(ItemFolder);

        foreach (var mesh in Meshes)
        {
            if (MeshLookup.ContainsKey(mesh.name))
            {
                Debug.LogError($"Dupe mesh: {mesh.name}");
                continue;
            }
            MeshLookup.Add(mesh.name, mesh);
        }

        foreach (var material in Materials)
        {
            if (MaterialLookup.ContainsKey(material.name))
            {
                Debug.LogError($"Dupe material: {material.name}");
                continue;
            }
            MaterialLookup.Add(material.name, material);
        }
    }

    internal MeshRenderer GetMesh(string name)
    {
        if (MeshLookup.ContainsKey(name))
        {
            return MeshLookup[name];
        }
        return MeshLookup["DefaultCube"];
    }

    internal Material GetMaterial(string name)
    {
        if (MaterialLookup.ContainsKey(name))
        {
            return MaterialLookup[name];
        }
        return MaterialLookup["DefaultMaterial"];
    }

    internal Material[] GetMaterials(string materials)
    {
        if (string.IsNullOrEmpty(materials))
        {
            return null;
        }
        else
        {
            return materials.Split(',').Select(GetMaterial).ToArray();
        }
    }


    private List<Type> _allBehaviours;

    public List<Type> AllBehaviourTypes
    {
        get
        {
            if (_allBehaviours == null)
            {
                _allBehaviours = ReflectionHelper.GetAllTypes(typeof(IBehaviour));
            }
            return _allBehaviours;
        }
    }
}