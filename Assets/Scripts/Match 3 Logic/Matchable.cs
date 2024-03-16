using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Matchable : Movable
{
    private MatchablePool pool;
    private int type;
    private Cursor cursor;
    private bool hasPowerUp = false;
    private PowerType powerType;

    public int Type 
    {
        get 
        {
            return type;
        } 
    }    
    
    public SpriteRenderer spriteRenderer;
    public bool HasPowerUp { get { return hasPowerUp; } }

    public Vector2Int position;

    public PowerType getPowerType { get { return  powerType; } }

    private void Awake()
    {
        cursor = Cursor.Instance;
        pool = (MatchablePool)MatchablePool.Instance;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetType(int type, Sprite sprite)
    {
        this.type = type;
        spriteRenderer.sprite = sprite;
    }

    public IEnumerator Resolve(Transform collectionPoint, bool isPowerUp)
    {
        spriteRenderer.sortingOrder = 2;

        yield return StartCoroutine(MoveToPosition(collectionPoint.position, isPowerUp));

        spriteRenderer.sortingOrder = 1;
        pool.ReturnObjectToPool(this);
    }

    public void setPowerUp(Sprite sprite, PowerType powerType)
    {

        spriteRenderer.sprite = sprite;
        hasPowerUp = true;
    }

    private void OnMouseDown()
    {
        cursor.SelectFirst(this);
    }

    private void OnMouseUp()
    {
        cursor.SelectFirst(null);
    }

    private void OnMouseEnter()
    {
        cursor.SelectSecond(this);
    }

    public override string ToString()
    {
        return gameObject.name;
    }
}
