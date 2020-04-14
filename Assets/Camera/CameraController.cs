﻿using System.Linq;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera Camera;
    internal float Speed;

    public float SpeedMin = 0.1f;
    public float SpeedMax = 2f;
    public int ZoomMax = 15;
    public int ZoomMin = 2;
    public float ZoomStep = 5;

    public float RenderHeight = 2.5f;
    public float RenderWidth = 4f;
    private float _journeyLength;
    private Vector3 _panDesitnation;

    private bool _panning;

    private Vector3 _panSource;

    private float _startTime;

    public void MoveToViewPoint(Vector3 panDesitnation)
    {
        _startTime = Time.time;
        _panSource = transform.position;
        _panDesitnation = panDesitnation;
        _journeyLength = Vector3.Distance(_panSource, _panDesitnation);

        _panning = true;
    }

    public void Start()
    {
        Camera = GetComponent<Camera>();
    }

    internal void MoveToCell(Cell cell)
    {
        MoveToViewPoint(cell.Vector);
    }

    public void Update()
    {
        if (_panning)
        {
            var distCovered = (Time.time - _startTime) * 1000;
            var fracJourney = distCovered / _journeyLength;

            var lerp = Vector3.Lerp(_panSource, _panDesitnation, fracJourney);

            transform.position = new Vector3(lerp.x, lerp.y, -10);

            if (transform.position.x == _panDesitnation.x
                && transform.position.y == _panDesitnation.y)
            {
                _panning = false;
            }
        }
        else
        {
            if (Game.Instance == null || Game.Instance.Typing)
            {
                return;
            }

            float horizontal = Input.GetAxis("Horizontal") / Time.timeScale;
            float vertical = Input.GetAxis("Vertical") / Time.timeScale;
            float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
            if (horizontal != 0 || vertical != 0 || mouseWheel != 0)
            {
                var step = Mathf.Clamp((int)Game.Instance.TimeManager.TimeStep, 1, 8);
                var x = Mathf.Clamp(transform.position.x + (horizontal * Speed * step), Game.Instance.Map.MinX, Game.Instance.Map.MaxX);
                var y = Mathf.Clamp(transform.position.y + (vertical * Speed * step), Game.Instance.Map.MinY, Game.Instance.Map.MaxY);
                transform.position = new Vector3(x, y, transform.position.z);

                Camera.orthographicSize = Mathf.Clamp(Camera.orthographicSize - (mouseWheel * ZoomStep),
                    ZoomMin, ZoomMax);

                Speed = Helpers.ScaleValueInRange(SpeedMin, SpeedMax, ZoomMin, ZoomMax, Camera.orthographicSize);
                ZoomStep = Mathf.Max(2f, Camera.orthographicSize / 2f);

                // expand the map as the camera reaches the edges
                //var point = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2));
                //Game.Instance.Map.ExpandChunks(Game.Instance.GetSelectedCells(point, point).First());
            }
        }
    }
}