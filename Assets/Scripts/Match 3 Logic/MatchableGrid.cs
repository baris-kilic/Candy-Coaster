using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchableGrid : GridSystem<Matchable>
{
    private MatchablePool pool;
    private ScoreManager scoreManager;

    [SerializeField] private Vector3 offScreenOffset;
    
    private void Start()
    {
        pool = (MatchablePool)MatchablePool.Instance;
        scoreManager = ScoreManager.Instance;
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
                newMatchable.position = new Vector2Int(x, y);
                newMatchable.gameObject.SetActive(true);
                PutItemOnGrid(new Vector2Int (x, y), newMatchable);

                int initialType = newMatchable.Type;

                while (!allowMatches && IsPartOfAMatch(newMatchable))
                {
                    if (initialType == pool.NextType(newMatchable))
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

    public bool IsPartOfAMatch(Matchable toMatch)
    {
        int horizontalMatches = CountMatchesInDirection(toMatch, Vector2Int.left) + CountMatchesInDirection(toMatch, Vector2Int.right);
        if (horizontalMatches > 1) 
            return true;

        int verticalMatches = CountMatchesInDirection(toMatch, Vector2Int.up) + CountMatchesInDirection(toMatch, Vector2Int.down);
        if (verticalMatches > 1)
            return true;
        return false;
    }

    private int CountMatchesInDirection(Matchable toMatch, Vector2Int direction)
    {
        int matches = 0;

        Vector2Int position = toMatch.position + direction;

        while (CheckBound(position) && !isEmpty(position) && GetItemFromGrid(position).Type == toMatch.Type)
        {
            matches++;
            position += direction;
        }

        return matches;
    }

    private Match GetMatchesInDirection(Matchable toMatch, Vector2Int direction)
    {
        Match matches = new();

        Vector2Int position = toMatch.position + direction;

        while (CheckBound(position) && !isEmpty(position))
        {
            Matchable next = GetItemFromGrid(position);
            if (next.Type == toMatch.Type)
            {
                matches.AddMatchable(next);
                position += direction;
            }
            else
                break;
        }

        return matches;
    }

    public IEnumerator TrySwap(Matchable[] toBeSwapped)
    {
        Matchable[] copies = new Matchable[2];
        copies[0] = toBeSwapped[0];
        copies[1] = toBeSwapped[1];

        yield return StartCoroutine(Swap(copies));

        Match matches1 = GetMatch(copies[0]);
        Match matches2 = GetMatch(copies[1]);

        if (matches1 != null)
        {
            StartCoroutine(scoreManager.ResolveMatch(matches1));
        }
        if (matches2 != null)
        {
            StartCoroutine(scoreManager.ResolveMatch(matches2));
        }
        if (matches1 == null && matches2 == null)
            StartCoroutine(Swap(copies));
       
    }

    private Match GetMatch(Matchable matchable)
    {
        Match match = new Match(matchable);
        Match horizontalMatch,
            verticalMatch;

        horizontalMatch = GetMatchesInDirection(matchable, Vector2Int.left);
        horizontalMatch.Merge(GetMatchesInDirection(matchable, Vector2Int.right));

        if (horizontalMatch.Count > 1)
            match.Merge(horizontalMatch);
        
        verticalMatch = GetMatchesInDirection(matchable, Vector2Int.up);
        verticalMatch.Merge(GetMatchesInDirection(matchable, Vector2Int.down));

        if (verticalMatch.Count > 1)
            match.Merge(verticalMatch);

        if (match.Count == 1)
            return null;

        return match;
    }

    private IEnumerator Swap(Matchable[] toBeSwapped)
    {
        SwapItems(toBeSwapped[0].position, toBeSwapped[1].position);

        Vector2Int temp = toBeSwapped[0].position;
        toBeSwapped[0].position = toBeSwapped[1].position;
        toBeSwapped[1].position = temp;

        Vector3 pos1 = toBeSwapped[0].transform.position;
        Vector3 pos2 = toBeSwapped[1].transform.position;

        Coroutine swap1 = StartCoroutine(toBeSwapped[0].SwapToPosition(pos2));
        Coroutine swap2 = StartCoroutine(toBeSwapped[1].SwapToPosition(pos1));

        // Wait for both coroutines to finish
        yield return swap1;
        yield return swap2;
    }
}
