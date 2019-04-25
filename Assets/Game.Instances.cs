﻿using UnityEngine;

public partial class Game //.Instances
{
    private static CameraController _cameraController;
    private static CellInfoPanel _cellInfoPanel;
    private static CraftingScreen _craftinScreen;
    private static CreatureController _creatureController;
    private static CreatureInfoPanel _creatureInfoPanel;
    private static FileController _fileController;
    private static Game _gameInstance;
    private static ItemController _itemController;
    private static MapGrid _mapGrid;
    private static OrderSelectionController _orderSelectionController;
    private static OrderTrayController _orderTrayController;
    private static SaveManager _saveManager;
    private static SpriteStore _spriteStore;
    private static StockpileController _stockpileController;
    private static StructureController _structureController;
    private static SunController _sunController;
    private static Taskmaster _taskmaster;
    private static TimeManager _timeManager;

    public static CameraController CameraController
    {
        get
        {
            if (_cameraController == null)
            {
                _cameraController = GameObject.Find(ControllerConstants.CameraController).GetComponent<CameraController>();
            }

            return _cameraController;
        }
    }

    public static CellInfoPanel CellInfoPanel
    {
        get
        {
            if (_cellInfoPanel == null)
            {
                _cellInfoPanel = GameObject.Find("CellInfoPanel").GetComponent<CellInfoPanel>();
            }

            return _cellInfoPanel;
        }
    }

    public static Game Controller
    {
        get
        {
            if (_gameInstance == null)
            {
                _gameInstance = GameObject.Find(ControllerConstants.GameController).GetComponent<Game>();
            }

            return _gameInstance;
        }
    }

    public static CraftingScreen CraftingScreen
    {
        get
        {
            return _craftinScreen ?? (_craftinScreen = GameObject.Find("CraftingPanel").GetComponent<CraftingScreen>());
        }
    }

    public static CreatureController CreatureController
    {
        get
        {
            if (_creatureController == null)
            {
                _creatureController = GameObject.Find(ControllerConstants.CreatureController).GetComponent<CreatureController>();
            }

            return _creatureController;
        }
    }

    public static CreatureInfoPanel CreatureInfoPanel
    {
        get
        {
            if (_creatureInfoPanel == null)
            {
                _creatureInfoPanel = GameObject.Find("CreatureInfoPanel").GetComponent<CreatureInfoPanel>();
            }

            return _creatureInfoPanel;
        }
    }

    public static FileController FileController
    {
        get
        {
            if (_fileController == null)
            {
                _fileController = GameObject.Find("FileController").GetComponent<FileController>();
                _fileController.Load();
            }

            return _fileController;
        }
    }

    public static ItemController ItemController
    {
        get
        {
            if (_itemController == null)
            {
                _itemController = GameObject.Find(ControllerConstants.ItemController).GetComponent<ItemController>();
            }

            return _itemController;
        }
    }

    public static MapGrid MapGrid
    {
        get
        {
            if (_mapGrid == null)
            {
                _mapGrid = GameObject.Find(ControllerConstants.MapController).GetComponent<MapGrid>();
            }

            return _mapGrid;
        }
    }

    public static OrderSelectionController OrderSelectionController
    {
        get
        {
            if (_orderSelectionController == null)
            {
                _orderSelectionController = GameObject.Find(ControllerConstants.OrderSelectionController).GetComponent<OrderSelectionController>();
            }

            return _orderSelectionController;
        }
    }

    public static OrderTrayController OrderTrayController
    {
        get
        {
            if (_orderTrayController == null)
            {
                _orderTrayController = GameObject.Find("OrderTray").GetComponent<OrderTrayController>();
            }

            return _orderTrayController;
        }
    }

    public static SaveManager SaveManager
    {
        get
        {
            if (_saveManager == null)
            {
                _saveManager = GameObject.Find("SaveManager").GetComponent<SaveManager>();
            }

            return _saveManager;
        }
    }

    public static SpriteStore SpriteStore
    {
        get
        {
            if (_spriteStore == null)
            {
                _spriteStore = GameObject.Find(ControllerConstants.SpriteController).GetComponent<SpriteStore>();
                _spriteStore.LoadResources();
            }

            return _spriteStore;
        }
    }

    public static StockpileController StockpileController
    {
        get
        {
            if (_stockpileController == null)
            {
                _stockpileController = GameObject.Find(ControllerConstants.StockpileController).GetComponent<StockpileController>();
            }

            return _stockpileController;
        }
    }

    public static StructureController StructureController
    {
        get
        {
            if (_structureController == null)
            {
                _structureController = GameObject.Find(ControllerConstants.StructureController).GetComponent<StructureController>();
            }

            return _structureController;
        }
    }

    public static SunController SunController
    {
        get
        {
            if (_sunController == null)
            {
                _sunController = GameObject.Find("SunController").GetComponent<SunController>();
            }

            return _sunController;
        }
    }

    public static Taskmaster Taskmaster
    {
        get
        {
            if (_taskmaster == null)
            {
                _taskmaster = GameObject.Find("Taskmaster").GetComponent<Taskmaster>();
            }

            return _taskmaster;
        }
    }

    public static TimeManager TimeManager
    {
        get
        {
            if (_timeManager == null)
            {
                _timeManager = GameObject.Find(ControllerConstants.TimeController).GetComponent<TimeManager>();
            }

            return _timeManager;
        }
    }
}