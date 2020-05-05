﻿using Structures.Work;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Structures
{
    public class StructureController : MonoBehaviour
    {
        public StructureRenderer StructureRendererPrefab;

        private static List<Type> _structureTypes;
        private float _lastUpdate;
        private Dictionary<string, Structure> _structureDataReference;
        private Dictionary<string, string> _structureTypeFileMap;

        public static List<Type> StructureTypes
        {
            get
            {
                if (_structureTypes == null)
                {
                    _structureTypes = ReflectionHelper.GetAllTypes(typeof(Structure));
                }

                return _structureTypes;
            }
        }

        internal Dictionary<string, Structure> StructureDataReference
        {
            get
            {
                StructureTypeFileMap.First();
                return _structureDataReference;
            }
        }

        internal Dictionary<string, string> StructureTypeFileMap
        {
            get
            {
                if (_structureTypeFileMap == null)
                {
                    _structureTypeFileMap = new Dictionary<string, string>();
                    _structureDataReference = new Dictionary<string, Structure>();
                    foreach (var structureFile in Game.Instance.FileController.StructureJson)
                    {
                        try
                        {
                            var data = GetFromJson(structureFile.text);
                            _structureTypeFileMap.Add(data.Name, structureFile.text);
                            _structureDataReference.Add(data.Name, data);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Unable to load structure {structureFile}: {ex.Message}");
                        }
                    }
                }
                return _structureTypeFileMap;
            }
        }

        public static Structure GetFromJson(string json)
        {
            var structure = json.LoadJson<Structure>();
            var type = GetTypeFor(structure.Type);

            if (type != null)
            {
                return json.LoadJson(type) as Structure;
            }

            return structure;
        }

        public static Type GetTypeFor(string name)
        {
            return StructureTypes.Find(w => w.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public void RefreshStructure(Structure structure)
        {
            if (structure.Cell == null)
            {
                return;
            }
            structure.Renderer.SpriteRenderer.sprite = structure.GetSprite();

        }

        public Structure SpawnStructure(string name, Cell cell, Faction faction, bool draw = true)
        {
            var structureData = StructureTypeFileMap[name];

            var structure = GetFromJson(structureData);
            var renderer = Instantiate(StructureRendererPrefab, transform);
            renderer.transform.name = structure.Name + " " + structure.Id;
            structure.Renderer = renderer;
            renderer.Data = structure;

            structure.Cell = cell;
            IndexStructure(structure);

            if (draw)
            {
                structure.Refresh();
            }
            faction?.AddStructure(structure);

            if (structure is Container container)
            {
                var zone = Game.Instance.ZoneController.GetZoneForCell(cell);

                if (zone != null && zone is StorageZone store)
                {
                    container.Filter = store.Filter;
                }
            }

            RefreshStructure(structure);
            return structure;
        }

        public void Update()
        {

            if (Game.Instance.TimeManager.Paused)
                return;

            _lastUpdate += Time.deltaTime;

            if (_lastUpdate > Game.Instance.TimeManager.CreatureTick)
            {
                _lastUpdate = 0;
                foreach (var structure in Game.Instance.IdService.StructureLookup.Values.OfType<WorkStructureBase>().Where(s => !s.IsBluePrint))
                {
                    structure.Process(Game.Instance.TimeManager.CreatureTick);
                }
            }
        }

        internal void DestroyStructure(Structure structure)
        {
            if (structure != null)
            {
                if (structure.Cell == null)
                {
                    Debug.Log("Unbound structure");
                }

                if (structure.IsInterlocking())
                {
                    var cell = structure.Cell;
                    structure.Cell = null;
                    foreach (var interlocked in cell.NonNullNeighbors.Where(c => c.Structure?.IsInterlocking() == true).Select(c => c.Structure))
                    {
                        interlocked.UpdateInterlocking();
                    }
                }

                Game.Instance.IdService.RemoveEntity(structure);
                Game.Instance.FactionController.Factions[structure.FactionName].Structures.Remove(structure);
                Game.Instance.AddItemToDestroy(structure.Renderer.gameObject);
            }
        }

        internal Structure GetStructureBluePrint(string name, Cell cell, Faction faction)
        {
            var structure = SpawnStructure(name, cell, faction);
            structure.IsBluePrint = true;
            structure.Refresh();
            return structure;
        }

        private void IndexStructure(Structure structure)
        {
            Game.Instance.IdService.EnrollEntity(structure);
        }
    }
}
