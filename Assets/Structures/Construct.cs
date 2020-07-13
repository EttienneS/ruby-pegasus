﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Structures
{
    public class Construct
    {
        public string Floor;
        public Dictionary<char, string> Key;
        public string Name;
        public List<string> Plan;

        [JsonIgnore]
        private List<string> _currentPlan;

        [JsonIgnore]
        public List<string> CurrentPlan
        {
            get
            {
                if (_currentPlan == null)
                {
                    _currentPlan = ValidatePlan(Plan);
                }

                return _currentPlan;
            }
            set
            {
                _currentPlan = ValidatePlan(value);
            }
        }

        private List<string> ValidatePlan(List<string> plan)
        {
            var newPlan = plan.ToList();

            var longest = 0;
            for (var i = 0; i < newPlan.Count; i++)
            {
                if (newPlan[i].Length > longest)
                {
                    longest = newPlan[i].Length;
                }

                newPlan[i] = newPlan[i].PadRight(newPlan.Count, '.');
            }

            var lineCount = newPlan.Count;
            for (var i = 0; i < longest - lineCount; i++)
            {
                var line = string.Empty.PadRight(longest, '.');
                newPlan.Add(line);
            }

            return newPlan;
        }

        [JsonIgnore]
        public List<string> FlippedPlan
        {
            get
            {
                var flipped = CurrentPlan.ToList();
                flipped.Reverse();

                return flipped;
            }
        }

        [JsonIgnore]
        private Texture2D _texture;

        [JsonIgnore]
        public int Height
        {
            get
            {
                return CurrentPlan.Count;
            }
        }

        private Sprite _sprite;

        [JsonIgnore]
        public Sprite Sprite
        {
            get
            {
                if (_sprite == null)
                {
                    _sprite = Sprite.Create(Texture,
                                            new Rect(0, 0, Texture.width, Texture.height),
                                            new Vector2(0.5f, 0.5f), Map.PixelsPerCell);
                }
                return _sprite;
            }
        }

        [JsonIgnore]
        public int Width
        {
            get
            {
                var longest = 0;

                foreach (var line in CurrentPlan)
                {
                    if (line.Length > longest)
                    {
                        longest = line.Length;
                    }
                }

                return longest;
            }
        }

        [JsonIgnore]
        internal Texture2D Texture
        {
            get
            {
                if (_texture == null)
                {
                    _texture = GetTexture();
                }

                return _texture;
            }
        }

        public string Description { get; set; }

        [JsonIgnore]
        public Cost TotalCost
        {
            get
            {
                var cost = new Cost();

                foreach (var line in FlippedPlan)
                {
                    foreach (var character in line)
                    {
                        if (character == '.')
                        {
                            continue;
                        }

                        var structure = Game.Instance.StructureController.StructureDataReference[GetStructure(character)];
                        cost = Cost.AddCost(cost, structure.Cost);
                    }
                }

                return cost;
            }
        }

        internal Texture2D GetTexture()
        {
            var texture = new Texture2D(Width * Map.PixelsPerCell, Height * Map.PixelsPerCell);

            var y = 0;
            var x = 0;

            foreach (var line in FlippedPlan)
            {
                foreach (var character in line)
                {
                    var startX = x * Map.PixelsPerCell;
                    var startY = y * Map.PixelsPerCell;

                    Texture2D sourceTexture;
                    if (character == '.')
                    {
                        sourceTexture = new Texture2D(1, 1);
                        sourceTexture.SetPixel(0, 0, new Color(0, 0, 0, 0));
                        sourceTexture.Apply();
                    }
                    else
                    {
                        var structure = Game.Instance.StructureController.StructureDataReference.Values.First(s => s.Name == GetStructure(character));
                        sourceTexture = Game.Instance.SpriteStore.GetSprite(structure.Icon).texture;
                    }
                    var constructTexture = sourceTexture.Clone();
                    constructTexture.ScaleToGridSize(1, 1);

                    for (var subTexX = 0; subTexX < Map.PixelsPerCell; subTexX++)
                    {
                        for (var subTexY = 0; subTexY < Map.PixelsPerCell; subTexY++)
                        {
                            var pixel = constructTexture.GetPixel(subTexX, subTexY);
                            texture.SetPixel(startX + subTexX,
                                             startY + subTexY,
                                             pixel);
                        }
                    }

                    x++;
                }
                y++;
            }
            texture.Apply();

            return texture;
        }

        public string GetStructure(char character)
        {
            string structureName = Floor;
            if (character != ' ')
            {
                structureName = Key[character];
            }

            return structureName;
        }

        internal bool ValidateStartPos(Cell cellData)
        {
            var x = 0;
            var y = 0;
            foreach (var line in FlippedPlan)
            {
                foreach (var character in line)
                {
                    if (character == '.')
                    {
                        x++;
                        continue;
                    }
                    var cell = Game.Instance.Map.GetCellAtCoordinate(cellData.X + x, cellData.Z + y);

                    if (cell.TravelCost > 0)
                    {
                        if (cell.Structure?.Name.Equals(GetStructure(character)) == false)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }

                    x++;
                }
                x = 0;
                y++;
            }
            return true;
        }

        internal bool Place(Cell origin, Faction faction)
        {
            var x = 0;
            var y = 0;

            foreach (var line in FlippedPlan)
            {
                foreach (var character in line)
                {
                    if (character == '.')
                    {
                        x++;
                        continue;
                    }

                    var cell = Game.Instance.Map.GetCellAtCoordinate(origin.X + x, origin.Z + y);

                    if (cell.TravelCost > 0 && cell.Structure == null)
                    {
                        Game.Instance.StructureController.SpawnBlueprint(GetStructure(character), cell, faction);
                    }

                    x++;
                }
                x = 0;
                y++;
            }
            return true;
        }

        public void RotateLeft()
        {
            RotateRight();
            RotateRight();
            RotateRight();
        }

        public void RotateRight()
        {
            var current = new char[Width, Height];
            var x = 0;
            var y = 0;

            foreach (var line in CurrentPlan)
            {
                foreach (var character in line)
                {
                    current[x++, y] = character;
                }
                x = 0;
                y++;
            }

            var newArray = new char[Width, Height];

            for (int width = Width - 1; width >= 0; width--)
            {
                for (int height = 0; height < Height; height++)
                {
                    newArray[height, Width - width - 1] = current[width, height];
                }
            }

            var newPlan = new List<string>();
            for (y = 0; y < Height; y++)
            {
                var line = string.Empty;
                for (x = 0; x < Width; x++)
                {
                    line += newArray[x, y];
                }
                newPlan.Add(line);
            }

            CurrentPlan = newPlan;
        }
    }
}