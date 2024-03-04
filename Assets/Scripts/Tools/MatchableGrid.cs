using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class MatchableGrid : GridSystem<Matchable>
{
    private MatchablePool pool;
    [SerializeField] private Vector3 offScreenOffset;
    
    private void Start()
    {
        pool = (MatchablePool)MatchablePool.Instance;
    }

    public IEnumerator PopulateGrid(bool allowMatches = false)
    {
        Matchable newMatchable;
        Vector3 onScreenPosition;

        for (int y = 0; y < Dimensions.y ; y++) { 
            for (int x = 0; x < Dimensions.x ; x++)
            {
                newMatchable = pool.GetRandomMatchable();
                //newMatchable.transform.position = transform.position + new Vector3(x, y);
                onScreenPosition = transform.position + new Vector3(x, y);
                newMatchable.transform.position = onScreenPosition + offScreenOffset;
                newMatchable.gameObject.SetActive(true);
                PutItemOnGrid(new Vector2Int (x, y), newMatchable);

                int type = newMatchable.Type;

                while (!allowMatches && IsPartOfAMatch(newMatchable))
                {
                    if (type == pool.NextType(newMatchable))
                    {
                        Debug.LogWarning("Matchable Type Cannot Be Found X:" + x + " Y:" + y + ".");
                        Debug.Break();
                        break;
                    }                       
                }

                StartCoroutine(newMatchable.MoveToPosition(onScreenPosition));
                yield return new WaitForSeconds(0.01f);
            }
        }
    }

    public bool IsPartOfAMatch(Matchable matchable)
    {
        return false;
    }
}
