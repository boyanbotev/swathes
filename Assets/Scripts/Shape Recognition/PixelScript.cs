using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PixelScript : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        ConnectionManager.OnJemClicked(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ConnectionManager.OnJemEnter(this);
    }

    public void SelectJem()
    {
        transform.localScale *= 1.33f; //was 1.4f
    }

    public void DeselectJem()
    {
        transform.localScale /= 1.33f; //was 1.4f
    }
}
