using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Matchable : Movable
{
    private MatchablePool pool;
    private int type;
    private Cursor cursor;
    private PowerType powerType = PowerType.none;

    public int Type 
    {
        get 
        {
            return type;
        } 
    }    
    
    public SpriteRenderer spriteRenderer;

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

    /*First, we reset the power type before we send the object to the pool. After, increasing
     * the sorting order for player can see the resolving animations smoothly. Move the object to
     * the corresponding position and isPowerUp boolean(if powerup, we merge them in powerUp matchable,
     * if not, our collection point is the train at the bottom). Reset the sorting order and return to object
     * to the pool.
     */
    public IEnumerator Resolve(Transform collectionPoint, bool isPowerUp)
    {
        this.powerType = PowerType.none;
        spriteRenderer.sortingOrder = 2;

        yield return StartCoroutine(MoveToPositionTween(collectionPoint.position, isPowerUp));

        spriteRenderer.sortingOrder = 1;
        pool.ReturnObjectToPool(this);
    }

    public Matchable setPowerUp(Sprite sprite, PowerType powerType)
    {
        this.powerType = powerType;
        spriteRenderer.sprite = sprite;
        return this;
    }

    private void OnMouseDown()
    {
        if (pool.allowSwap)
            cursor.SelectFirst(this);
    }

    private void OnMouseUp()
    {
        if (pool.allowSwap)
            cursor.SelectFirst(null);
    }

    private void OnMouseEnter()
    {
        if (pool.allowSwap)
            cursor.SelectSecond(this);
    }

    public override string ToString()
    {
        return gameObject.name;
    }
}
