using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
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

    public IEnumerator PopulateGrid(bool allowMatches = false, bool initialPopulation = false)
    {
        List<Matchable> newMatchables = new List<Matchable>();
        Matchable newMatchable;

        for (int y = 0; y < Dimensions.y ; y++) { 
            for (int x = 0; x < Dimensions.x ; x++)
            {
                if (isEmpty(new Vector2Int(x, y)))
                {
                    newMatchable = pool.GetRandomMatchable();
                    //newMatchable.transform.position = transform.position + new Vector3(x, y);
                    newMatchable.transform.position = transform.position + new Vector3(x, y) + offScreenOffset;
                    newMatchable.gameObject.SetActive(true);
                    newMatchable.position = new Vector2Int(x, y);
                    PutItemOnGrid(new Vector2Int(x, y), newMatchable);


                    newMatchables.Add(newMatchable);

                    int initialType = newMatchable.Type;
                 
                    while (!allowMatches && IsPartOfAMatch(newMatchable))
                    {
                        if (initialType == pool.NextType(newMatchable))
                        {
                            Debug.LogWarning("Matchable Type Cannot Be Found X:" + x + " Y:" + y + ".");
                            Debug.Break();
                            yield return null;
                            break;
                        }
                    }
                }
            }
        }

        for (int i = 0; i < newMatchables.Count; i++)
        {
            if (i == newMatchables.Count - 1)
                yield return StartCoroutine(newMatchables[i].MoveToPosition(transform.position + new Vector3(newMatchables[i].position.x, newMatchables[i].position.y)));
            else
                StartCoroutine(newMatchables[i].MoveToPosition(transform.position + new Vector3(newMatchables[i].position.x, newMatchables[i].position.y)));
            if (initialPopulation)
                yield return new WaitForSeconds(0.1f);
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

    private Match GetMatchesInDirection(Match tree, Matchable toMatch, Vector2Int direction)
    {
        Match matches = new();

        Vector2Int position = toMatch.position + direction;

        while (CheckBound(position) && !isEmpty(position))
        {
            Matchable next = GetItemFromGrid(position);
            if (next.Type == toMatch.Type && next.getPowerType == PowerType.none)
            {
                if (!tree.Contains(next))
                    matches.AddMatchable(next);
                else
                    matches.AddUnlisted();
                position += direction;
            }
            else
                break;
        }

        return matches;
    }

    public IEnumerator TrySwap(Matchable[] toBeSwapped)
    {
        pool.allowSwap = false;
        Match matches1 = null;
        Match matches2 = null;
        bool powerUpMatch1 = false;
        bool powerUpMatch2 = false;
        Matchable[] copies = new Matchable[2];
        copies[0] = toBeSwapped[0];
        copies[1] = toBeSwapped[1];

        yield return StartCoroutine(Swap(copies));

        if (copies[0].getPowerType == PowerType.match5)
        {
            Match candyMatch = null;
            if (copies[1].getPowerType != PowerType.match5)
            {
                candyMatch = GetMatchesByType(copies[1]);
            } else
            {
                candyMatch = GetEverything(copies[1]);
            }
            candyMatch.AddMatchable(copies[0]);
            StartCoroutine(scoreManager.ResolveMatch(candyMatch,true));
            StartCoroutine(FindAndScanForMatches());
            yield break;
        }

        if (copies[1].getPowerType == PowerType.match5)
        {
            Match candyMatch = null;
            if (copies[0].getPowerType != PowerType.match5)
            {
                candyMatch = GetMatchesByType(copies[0]);
            }
            else
            {
                candyMatch = GetEverything(copies[0]);
            }
            candyMatch.AddMatchable(copies[1]);
            StartCoroutine(scoreManager.ResolveMatch(candyMatch, true));
            StartCoroutine(FindAndScanForMatches());
            yield break;
        }

        if (copies[0].getPowerType != PowerType.none)
        {
            powerUpMatch1 = true;
            matches1 = GetPowerUpMatch(copies[0]);
        } else
        {
            matches1 = GetMatch(copies[0]);
        }

        if (copies[1].getPowerType != PowerType.none)
        {
            powerUpMatch2 = true;
            matches2 = GetPowerUpMatch(copies[1]);
        } else
        {
            matches2 = GetMatch(copies[1]);
        }

        if (powerUpMatch1 || powerUpMatch2)
        {
            if (powerUpMatch1)
            {
                if (matches2 != null)
                {
                    List<Matchable> elementsToRemoveFromMatches1 = new List<Matchable>();
                    foreach (Matchable matchable1 in matches1.Matchables)
                    {
                        foreach (Matchable matchable2 in matches2.Matchables)
                        {
                            if (matchable1 == matchable2)
                            {
                                elementsToRemoveFromMatches1.Add(matchable1);
                                break; 
                            }
                        }
                    }

                    // Remove elements from matches1
                    foreach (Matchable matchableToRemove in elementsToRemoveFromMatches1)
                    {
                        matches1.RemoveMatchable(matchableToRemove);
                    }
                }
                StartCoroutine(scoreManager.ResolveMatch(matches1, true));
            }
            if (powerUpMatch2)
            {
                if (matches1 != null)
                {
                    List<Matchable> elementsToRemoveFromMatches2 = new List<Matchable>();
                    foreach (Matchable matchable2 in matches2.Matchables)
                    {
                        foreach (Matchable matchable1 in matches1.Matchables)
                        {
                            if (matchable2 == matchable1)
                            {
                                elementsToRemoveFromMatches2.Add(matchable2);
                                break; 
                            }
                        }
                    }

                    // Remove elements from matches2
                    foreach (Matchable matchableToRemove in elementsToRemoveFromMatches2)
                    {
                        matches2.RemoveMatchable(matchableToRemove);
                    }
                }
                StartCoroutine(scoreManager.ResolveMatch(matches2, true));
            }
        }

        if (matches1 != null && !powerUpMatch1)
        {
            StartCoroutine(scoreManager.ResolveMatch(matches1, false));
        }
        if (matches2 != null && !powerUpMatch2)
        {
            StartCoroutine(scoreManager.ResolveMatch(matches2, false));
        }

        if (matches1 == null && matches2 == null)
        {
            yield return StartCoroutine(Swap(copies));

            if (ScanForMatches())
            {
                StartCoroutine(FindAndScanForMatches());
            } else
            {
                pool.allowSwap = true;
            }
        }
            
        else
        {
            StartCoroutine(FindAndScanForMatches());
        }    
    }

    private IEnumerator FindAndScanForMatches()
    {
        CollapseGrid();
        yield return StartCoroutine(PopulateGrid(true));
        if (ScanForMatches())
        {
            StartCoroutine(FindAndScanForMatches());
        } else
        {
            pool.allowSwap = true;
        }
    }

    private Match GetMatch(Matchable matchable)
    {
        Match match = new Match(matchable);
        Match horizontalMatch,
            verticalMatch;

        horizontalMatch = GetMatchesInDirection(match, matchable, Vector2Int.left);
        horizontalMatch.Merge(GetMatchesInDirection(match, matchable, Vector2Int.right));
        
        horizontalMatch.setOrientation(Orientation.horizontal);
        if (horizontalMatch.Count > 1)
        {
            match.Merge(horizontalMatch);
            GetBranches(match, horizontalMatch, Orientation.vertical);
        }

        verticalMatch = GetMatchesInDirection(match, matchable, Vector2Int.up);
        verticalMatch.Merge(GetMatchesInDirection(match, matchable, Vector2Int.down));

        verticalMatch.setOrientation(Orientation.vertical);
        if (verticalMatch.Count > 1)
        {
            match.Merge(verticalMatch);
            GetBranches(match, verticalMatch, Orientation.horizontal);
        }

        if (match.Count == 1)
            return null;

        return match;
    }

    private void GetBranches(Match tree, Match selectedBranch, Orientation orientation)
    {
        Match branch;
        
        foreach (Matchable match in selectedBranch.Matchables)
        {
            branch = GetMatchesInDirection(tree, match, orientation == Orientation.horizontal ? Vector2Int.left : Vector2Int.up);
            branch.Merge(GetMatchesInDirection(tree, match, orientation == Orientation.horizontal ? Vector2Int.right : Vector2Int.down));
            branch.setOrientation(orientation);
            if (branch.Count > 1)
            {
                tree.Merge(branch);
                GetBranches(tree, branch, orientation == Orientation.horizontal ? Orientation.vertical : Orientation.horizontal); 
            }
        }
    }

    private IEnumerator Swap(Matchable[] toBeSwapped)
    {
        SwapItems(toBeSwapped[0].position, toBeSwapped[1].position);

        Vector2Int temp = toBeSwapped[0].position;
        toBeSwapped[0].position = toBeSwapped[1].position;
        toBeSwapped[1].position = temp;

        Vector3 pos1 = toBeSwapped[0].transform.position;
        Vector3 pos2 = toBeSwapped[1].transform.position;

        // Wait for both coroutines to finish
                     StartCoroutine(toBeSwapped[0].SwapToPosition(pos2));
        yield return StartCoroutine(toBeSwapped[1].SwapToPosition(pos1));
    }

    private Match GetPowerUpMatch(Matchable matchable)
    {
        Match match = null;
        switch (matchable.getPowerType) 
        {
            case PowerType.match4horizontal:
                match = GetHorizontalMatch(matchable);
                break;
            case PowerType.match4vertical:
                match = GetVerticalMatch(matchable);
                break;
            case PowerType.match5:

                break;
            case PowerType.cross:
                match = GetDiagonalMatch(matchable);
                break;
            default:
                break;
        }
        return match;
    }

    private Match GetVerticalMatch(Matchable matchable)
    {
        Match match = new Match(matchable);
        for (int y = 0; y < Dimensions.y; y++)
        {
            Matchable newMatchable = GetItemFromGrid(new Vector2Int(matchable.position.x, y));
            if (!isEmpty(new Vector2Int(matchable.position.x, y)) && newMatchable.Idle && newMatchable.getPowerType == PowerType.none) 
            { 
                match.AddMatchable(newMatchable);
            }
        }
        return match;
    }

    private Match GetHorizontalMatch(Matchable matchable)
    {
        Match match = new Match(matchable);
        for (int x = 0; x < Dimensions.x; x++)
        {
            Matchable newMatchable = GetItemFromGrid(new Vector2Int(x, matchable.position.y));
            if (!isEmpty(new Vector2Int(x, matchable.position.y)) && newMatchable.Idle && newMatchable.getPowerType == PowerType.none)
            {
                match.AddMatchable(newMatchable);
            }
        }
        return match;
    }

    private Match GetDiagonalMatch(Matchable matchable)
    {
        Match match = new Match(matchable);
        for (int y = 0; y < Dimensions.y; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                if (Math.Abs(x-matchable.position.x) == Math.Abs(y-matchable.position.y))
                {
                    Matchable newMatchable = GetItemFromGrid(new Vector2Int(x, y));
                    if (!isEmpty(new Vector2Int(x, y)) && newMatchable.Idle && newMatchable.getPowerType == PowerType.none)
                    {
                        match.AddMatchable(newMatchable);
                    }
                }

            }
        }
        return match;
    }

    private Match GetMatchesByType(Matchable matchable)
    {
        Match match = new Match(matchable);
        for (int y = 0; y < Dimensions.y; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                Matchable newMatchable = GetItemFromGrid(new Vector2Int(x, y));
                if (!isEmpty(new Vector2Int(x, y)) && newMatchable.Idle && newMatchable.getPowerType == PowerType.none && matchable.Type == newMatchable.Type)
                {
                    match.AddMatchable(newMatchable);
                }
            }
        }
        return match;
    }

    private Match GetEverything(Matchable matchable)
    {
        Match match = new Match(matchable);
        for (int y = 0; y < Dimensions.y; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                Matchable newMatchable = GetItemFromGrid(new Vector2Int(x, y));
                if (!isEmpty(new Vector2Int(x, y)) && newMatchable.Idle && newMatchable.getPowerType == PowerType.none)
                {
                    match.AddMatchable(newMatchable);
                }
            }
        }
        return match;
    }

    private void CollapseGrid()
    {
        for (int x = 0; x < Dimensions.x; x++)
        {
            for (int y = 0; y < Dimensions.y - 1; y++) 
            {
                if (isEmpty(new Vector2Int (x, y)))
                {
                    for (int k = y + 1; k < Dimensions.y; k++)
                    {
                        if (!isEmpty(new Vector2Int (x, k)) && GetItemFromGrid(new Vector2Int(x, k)).Idle)
                        {
                            // Move upper matchable to the empty part (x,k) to (x,y)
                            MoveMatchableToTheDown(GetItemFromGrid(new Vector2Int(x, k)), new Vector2Int(x, y));
                            break;
                        }
                    }
                }
            }
        }
    }

    private void MoveMatchableToTheDown(Matchable matchable, Vector2Int pos) 
    {
        RemoveItemFromGrid(matchable.position);
        PutItemOnGrid(pos, matchable);
        matchable.position = pos;
        StartCoroutine(matchable.MoveToPosition(transform.position + new Vector3 (pos.x, pos.y)));
    }

    private bool ScanForMatches()
    {
        bool hasAMatch = false;
        for (int y = 0; y < Dimensions.y; y++)
        {
            for (int x = 0; x  < Dimensions.x; x++)
            {
                if (!isEmpty(new Vector2Int (x, y)))
                {

                    Matchable matchable = GetItemFromGrid(new Vector2Int(x, y));
                        
                    if (!matchable.Idle || matchable.getPowerType != PowerType.none)
                        continue;

                    Match match = GetMatch(matchable);

                    if (match != null)
                    {
                        hasAMatch = true;
                        StartCoroutine(scoreManager.ResolveMatch(match, false));
                    }
                }

            }
        }
        return hasAMatch;
    }
}
