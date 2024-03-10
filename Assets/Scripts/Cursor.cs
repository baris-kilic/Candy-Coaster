using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Cursor : Singleton<Cursor>
{
    private SpriteRenderer spriteRenderer;

    private Matchable[] selected;

    private MatchableGrid grid;

    protected override void Init()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.enabled = false;

        selected = new Matchable[2];
    }

    public void SelectFirst(Matchable toSelect)
    {
        selected[0] = toSelect;

        if (!enabled || selected[0] == null)
        {
            return;
        }
        transform.position = toSelect.transform.position;
        spriteRenderer.size = Vector2.one;
        spriteRenderer.enabled = true;
    }

    public void SelectSecond(Matchable matchable) { 
        selected[1] = matchable;

        if (!enabled || selected[0] == null || selected[1] == null || !selected[0].Idle || !selected[1].Idle || selected[0] == selected[1])
            return;
        
        if (SelectedAreAdjacent())
        {
            print("Swapping matchables at positions : (" + selected[0].position.x + ", " + selected[0].position.y + " ) and ( " + selected[1].position.x + ", " + selected[1].position.y);
            StartCoroutine(grid.TrySwap(selected));
        }
        SelectFirst(null);
    }

    private void Start()
    {
        grid = (MatchableGrid)MatchableGrid.Instance;
    }

    private bool SelectedAreAdjacent()
    {
        if (selected[0].position.x == selected[1].position.x)
        {
            if (selected[0].position.y == selected[1].position.y + 1)
            {
                spriteRenderer.size = new Vector2Int(1, 2);
                transform.position += Vector3.down / 2;
                return true;
            }
            else if (selected[0].position.y == selected[1].position.y - 1)
            {
                spriteRenderer.size = new Vector2Int(1, 2);
                transform.position += Vector3.up / 2;
                return true;
            }
        }
        if (selected[0].position.y == selected[1].position.y)
        {
            if (selected[0].position.x == selected[1].position.x + 1)
            {
                spriteRenderer.size = new Vector2Int(2, 1);
                transform.position += Vector3.left / 2;
                return true;
            }
            else if (selected[0].position.x == selected[1].position.x - 1)
            {
                spriteRenderer.size = new Vector2Int(2, 1);
                transform.position += Vector3.right / 2;
                return true;
            }
        }
        return false;
    }
}
