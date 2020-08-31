﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Models
{
    public class MeshRendererFactory : MonoBehaviour
    {
        private static Dictionary<string, MeshRenderer> _itemMeshLookup;
        private static Dictionary<string, MeshRenderer> _structureMeshLookup;

        public void Awake()
        {
            if (_structureMeshLookup == null)
            {
                _structureMeshLookup = LoadMeshesFromBundle("structures");
            }

            if (_itemMeshLookup == null)
            {
                _itemMeshLookup = LoadMeshesFromBundle("items");
            }
        }

        public MeshRenderer GetItemMesh(string name)
        {
            if (_itemMeshLookup.ContainsKey(name))
            {
                return _itemMeshLookup[name];
            }
            return _structureMeshLookup["DefaultCube"];
        }

        public MeshRenderer GetStructureMesh(string name)
        {
            if (_structureMeshLookup.ContainsKey(name))
            {
                return _structureMeshLookup[name];
            }
            return _structureMeshLookup["DefaultCube"];
        }

        public MeshRenderer InstantiateMesh(MeshRenderer renderer, Transform parent)
        {
            return Instantiate(renderer, parent);
        }

        private static AssetBundle GetAssetBundle(string bundleName)
        {
            var path = $@"{Environment.CurrentDirectory}\Assets\AssetBundles\{bundleName}";
            return AssetBundle.LoadFromFile(path);
        }

        private static Dictionary<string, MeshRenderer> LoadMeshesFromBundle(string bundleName)
        {
            var bundle = GetAssetBundle(bundleName);
            var meshes = new Dictionary<string, MeshRenderer>();

            foreach (var gameObject in bundle.LoadAllAssets<GameObject>())
            {
                var renderer = gameObject.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    if (meshes.ContainsKey(gameObject.name))
                    {
                        Debug.LogError($"Dupe mesh: {gameObject.name}");
                        continue;
                    }
                    meshes.Add(gameObject.name, renderer);
                }
            }

            return meshes;
        }
    }
}