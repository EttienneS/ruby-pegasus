﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BoxButtonImageSwap : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{
    private Sprite _selectedSprite;
    private Sprite _unselectedSprite;
    private Image _image;

    public void OnPointerEnter(PointerEventData eventData)
    {
        _image.sprite = _selectedSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _image.sprite = _unselectedSprite;
    }

    private void Start()
    {
        _image = GetComponent<Image>();
        if (_image == null)
        {
            _image = GetComponentInChildren<Image>();
        }

        _unselectedSprite = Resources.Load<Sprite>("uibox");
        _selectedSprite = Resources.Load<Sprite>("uiboxselected");
    }
}