using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Matchable : Movable
{
    private int type;

    public int Type 
    {
        get {
            return type;
        } 
    }    
    
    public SpriteRenderer sprireRenderer;

    private void Awake()
    {
        sprireRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetType(int type, Sprite sprite)
    {
        this.type = type;
        sprireRenderer.sprite = sprite;
    }

    public override string ToString()
    {
        return gameObject.name;
    }
}
