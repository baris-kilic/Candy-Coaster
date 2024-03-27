using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MatchableGrid : GridSystem<Matchable>
{
    private MatchablePool pool;
    private ScoreManager scoreManager;
    private AudioManager audioManager;

    // Offset for placing items off-screen initially
    [SerializeField] private Vector3 offScreenOffset;

    [SerializeField]
    private List<Matchable> possibleMoves;

    private void Start()
    {
        pool = (MatchablePool)MatchablePool.Instance;
        audioManager = AudioManager.Instance;
        scoreManager = ScoreManager.Instance;
    }

    public IEnumerator Reset()
    {
        for (int y = 0; y < Dimensions.y; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                // Remove and return items from the grid to the pool
                if (!isEmpty(new Vector2Int(x, y)))
                {
                    pool.ReturnObjectToPool(RemoveItemFromGrid(new Vector2Int(x, y)));
                }
            }
        }
        // Populate the grid after resetting
        yield return StartCoroutine(PopulateGrid(false,true));
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
                    // Get a random matchable from the pool
                    newMatchable = pool.GetRandomMatchable();
                    // Position the matchable off-screen initially
                    newMatchable.transform.position = transform.position + new Vector3(x, y) + offScreenOffset;
                    newMatchable.gameObject.SetActive(true);
                    newMatchable.position = new Vector2Int(x, y);
                    PutItemOnGrid(new Vector2Int(x, y), newMatchable);

                    // Add the matchable to the list of new matchables
                    newMatchables.Add(newMatchable);

                    // Check if the new matchable creates an immediate match (if not allowed)
                    int initialType = newMatchable.Type;
                    while (!allowMatches && IsPartOfAMatch(newMatchable))
                    {
                        // If the matchable creates a match with its initial type, log a warning
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

        // Move the newly added matchables into position
        for (int i = 0; i < newMatchables.Count; i++)
        {
            if (initialPopulation)
                StartCoroutine(audioManager.PlayDelayedSound(SoundEffects.land, 0.8f));
            if (i == newMatchables.Count - 1)
                yield return StartCoroutine(newMatchables[i].MoveToPosition(transform.position + 
                    new Vector3(newMatchables[i].position.x, newMatchables[i].position.y)));
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

        while (CheckBound(position) && !isEmpty(position) && GetItemFromGrid(position).Type == toMatch.Type 
            && GetItemFromGrid(position).getPowerType == PowerType.none)
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
        // Disable swapping while processing
        pool.allowSwap = false;

        Match matches1 = null;
        Match matches2 = null;
        bool powerUpMatch1 = false;
        bool powerUpMatch2 = false;
        Matchable[] copies = new Matchable[2];
        copies[0] = toBeSwapped[0];
        copies[1] = toBeSwapped[1];

        // Cancel any ongoing hints
        Hint.Instance.CancelHint();

        // Perform the swap and wait for it to complete
        yield return StartCoroutine(Swap(copies));

        // Handling matches involving Match-5 power-ups, if we use match5 powerup with a normal matchable, 
        //it finds all the matchables with same type and match them. If the match5 powerup combines with 
        //match5 powerup also, it matches everything.
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
            // Update move count and check for additional matches and break.
            UpdateCount();
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
            // Update move count and check for additional matches and break.
            UpdateCount();
            StartCoroutine(FindAndScanForMatches());
            yield break;
        }

        // Check if either matchable is a power-up
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

        // Handle power-up matches, if we have multiple matches, remove the duplicate matchables for matches.
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

        // Resolve regular matches if any
        if (matches1 != null && !powerUpMatch1)
        {
            StartCoroutine(scoreManager.ResolveMatch(matches1, false));
        }
        if (matches2 != null && !powerUpMatch2)
        {
            StartCoroutine(scoreManager.ResolveMatch(matches2, false));
        }

        // If no matches were made, revert the swap
        if (matches1 == null && matches2 == null)
        {
            yield return StartCoroutine(Swap(copies));

            // Check for new matches after reverting the swap
            if (ScanForMatches())
            {
                StartCoroutine(FindAndScanForMatches());
            } else
            {
                // If no matches found, allow swapping again
                pool.allowSwap = true;
            }
        }
            
        else
        {
            // If matches were found, update move count and check for additional matches
            UpdateCount();
            StartCoroutine(FindAndScanForMatches());
        }    
    }

    // Coroutine to find and scan for matches after grid changes
    private IEnumerator FindAndScanForMatches()
    {
        // Collapse the grid to remove empty spaces
        CollapseGrid();
        yield return StartCoroutine(PopulateGrid(true));
        // If matches are found, continue searching
        if (ScanForMatches())
        {
            StartCoroutine(FindAndScanForMatches());
        }
        else
        {
            // If no more matches, check for available moves
            CheckForMoves();
            // Allow swapping again
            pool.allowSwap = true;
        }
    }

    // Check for available moves
    public void CheckForMoves()
    {
        if (ScanForMoves() == 0 || GameManager.Instance.MoveCount == 0)
        {
            // If no moves available or no more moves allowed, end game
            GameManager.Instance.NoMoreMoves();
        }
        else
        {
            // If moves available, indicate a random move as a hint
            Hint.Instance.IndicateHint(possibleMoves[UnityEngine.Random.Range(0, possibleMoves.Count)].transform);
        }
    }

    // Method to find matches starting from a given matchable
    private Match GetMatch(Matchable matchable)
    {
        Match match = new Match(matchable);
        Match horizontalMatch,
            verticalMatch;

        // Find horizontal matches
        horizontalMatch = GetMatchesInDirection(match, matchable, Vector2Int.left);
        horizontalMatch.Merge(GetMatchesInDirection(match, matchable, Vector2Int.right));
        
        horizontalMatch.setOrientation(Orientation.horizontal);

        // If horizontal match found, merge and search for vertical branches
        if (horizontalMatch.Count > 1)
        {
            match.Merge(horizontalMatch);
            GetBranches(match, horizontalMatch, Orientation.vertical);
        }

        // Find vertical matches
        verticalMatch = GetMatchesInDirection(match, matchable, Vector2Int.up);
        verticalMatch.Merge(GetMatchesInDirection(match, matchable, Vector2Int.down));

        verticalMatch.setOrientation(Orientation.vertical);
        // If vertical match found, merge and search for horizontal branches
        if (verticalMatch.Count > 1)
        {
            match.Merge(verticalMatch);
            GetBranches(match, verticalMatch, Orientation.horizontal);
        }

        // If no match found, return null
        if (match.Count == 1)
            return null;

        return match;
    }

    // Method to recursively find branches of a match
    private void GetBranches(Match tree, Match selectedBranch, Orientation orientation)
    {
        Match branch;
        
        foreach (Matchable match in selectedBranch.Matchables)
        {
            // Find matches in the specified direction
            branch = GetMatchesInDirection(tree, match, orientation == Orientation.horizontal ? Vector2Int.left : Vector2Int.up);
            branch.Merge(GetMatchesInDirection(tree, match, orientation == Orientation.horizontal ? Vector2Int.right : Vector2Int.down));
            branch.setOrientation(orientation);

            // If branch found, merge and continue searching recursively
            if (branch.Count > 1)
            {
                tree.Merge(branch);
                GetBranches(tree, branch, orientation == Orientation.horizontal ? Orientation.vertical : Orientation.horizontal); 
            }
        }
    }

    // Coroutine to perform swapping animation
    private IEnumerator Swap(Matchable[] toBeSwapped)
    {
        // Swap items in the grid
        SwapItems(toBeSwapped[0].position, toBeSwapped[1].position);

        // Swap positions of matchables
        Vector2Int temp = toBeSwapped[0].position;
        toBeSwapped[0].position = toBeSwapped[1].position;
        toBeSwapped[1].position = temp;

        // Get positions for animation
        Vector3 pos1 = toBeSwapped[0].transform.position;
        Vector3 pos2 = toBeSwapped[1].transform.position;

        audioManager.PlaySound(SoundEffects.swap);

        // Wait for both coroutines to finish
        StartCoroutine(toBeSwapped[0].SwapToPosition(pos2));
        yield return StartCoroutine(toBeSwapped[1].SwapToPosition(pos1));
    }

    public void UpdateCount()
    {
        GameManager.Instance.SetMoveCount(GameManager.Instance.MoveCount - 1);
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
        audioManager.PlaySound(SoundEffects.powerup);
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
        audioManager.PlaySound(SoundEffects.powerup);
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
        audioManager.PlaySound(SoundEffects.powerup);
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
                if (!isEmpty(new Vector2Int(x, y)) && newMatchable.Idle && newMatchable.getPowerType == PowerType.none 
                    && matchable.Type == newMatchable.Type)
                {
                    match.AddMatchable(newMatchable);
                }
            }
        }
        audioManager.PlaySound(SoundEffects.powerup);
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
        audioManager.PlaySound(SoundEffects.powerup);
        return match;
    }

    // Method to collapse the grid by moving matchables downward to fill empty spaces
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
                            // Move the matchable above the empty space downward
                            MoveMatchableToTheDown(GetItemFromGrid(new Vector2Int(x, k)), new Vector2Int(x, y));
                            break;
                        }
                    }
                }
            }
        }
    }

    // Method to move a matchable downward in the grid and animate the movement
    private void MoveMatchableToTheDown(Matchable matchable, Vector2Int pos) 
    {
        RemoveItemFromGrid(matchable.position);
        PutItemOnGrid(pos, matchable);
        matchable.position = pos;
        StartCoroutine(matchable.MoveToPosition(transform.position + new Vector3 (pos.x, pos.y)));
    }

    // Method to scan for matches in the grid
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

                    // Check if the matchable is idle and does not have any special power
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

    // Method to scan for possible moves
    private int ScanForMoves()
    {
        possibleMoves = new List<Matchable>();
        bool hasAMatch = false;

        for (int y = 0; y < Dimensions.y; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                Matchable newMatchable = GetItemFromGrid(new Vector2Int(x, y));

                // Check if the cell is within the grid boundaries and not empty
                if (CheckBound(new Vector2Int(x, y)) && !isEmpty(new Vector2Int(x, y)) && 
                    newMatchable.getPowerType == PowerType.none && CanMove(newMatchable))
                {
                    hasAMatch = true;
                    possibleMoves.Add(newMatchable);
                }
                // If the cell contains a special matchable and no other moves have been found, add it as a possible move
                else if (CheckBound(new Vector2Int(x, y)) && !isEmpty(new Vector2Int(x, y)) &&
                    newMatchable.getPowerType != PowerType.none && !hasAMatch)
                {
                    possibleMoves.Add(newMatchable);
                }
            }
        }
        return possibleMoves.Count;
    }

    // Method to check if a matchable can be moved
    private bool CanMove(Matchable newMatchable)
    {
        // Check if the matchable can move in any direction
        return CanMoveWithDirection(newMatchable, Vector2Int.up) ||
               CanMoveWithDirection(newMatchable, Vector2Int.down) ||
               CanMoveWithDirection(newMatchable, Vector2Int.left) ||
               CanMoveWithDirection(newMatchable, Vector2Int.right);
    }


    // Method to check if a matchable can move in a specific direction
    private bool CanMoveWithDirection(Matchable newMatchable, Vector2Int direction)
    {
        int matchableType = newMatchable.Type;
        int upperMatchCount = 0;
        int lowerMatchCount = 0;
        int leftMatchCount = 0;
        int rightMatchCount = 0;

        // Count matches in each direction
        if (CheckBound(newMatchable.position + Vector2Int.up))
            upperMatchCount = CountMatchesInDirectionWithPosition(newMatchable.position + direction, Vector2Int.up, matchableType);
        if (CheckBound(newMatchable.position + Vector2Int.down))
            lowerMatchCount = CountMatchesInDirectionWithPosition(newMatchable.position + direction, Vector2Int.down, matchableType);
        if (CheckBound(newMatchable.position + Vector2Int.left))
            leftMatchCount = CountMatchesInDirectionWithPosition(newMatchable.position + direction, Vector2Int.left, matchableType);
        if (CheckBound(newMatchable.position + Vector2Int.right))
            rightMatchCount = CountMatchesInDirectionWithPosition(newMatchable.position + direction, Vector2Int.right, matchableType);

        // Check if the matchable can move in the specified direction
        if (direction == Vector2Int.up)
        {
            if (upperMatchCount == 2 || (leftMatchCount >= 1 && rightMatchCount >= 1) || (leftMatchCount == 2) || (rightMatchCount == 2))
                return true;
        }
        else if (direction == Vector2Int.down)
        {
            if (lowerMatchCount == 2 || (leftMatchCount >= 1 && rightMatchCount >= 1) || (leftMatchCount == 2) || (rightMatchCount == 2))
                return true;
        }
        else if (direction == Vector2Int.left)
        {
            if (leftMatchCount == 2 || (upperMatchCount >= 1 && lowerMatchCount >= 1) || (upperMatchCount == 2) || (lowerMatchCount == 2))
                return true;
        }
        else if (direction == Vector2Int.right)
        {
            if (rightMatchCount == 2 || (upperMatchCount >= 1 && lowerMatchCount >= 1) || (upperMatchCount == 2) || (lowerMatchCount == 2))
                return true;
        }

        return false;
    }

    // Method to count matches in a direction starting from a given position
    private int CountMatchesInDirectionWithPosition(Vector2Int position, Vector2Int direction, int matchType)
    {
        int matches = 0;

        Vector2Int checkPosition = position + direction;

        // Count consecutive matches in the specified direction
        while (CheckBound(checkPosition) && !isEmpty(checkPosition) && GetItemFromGrid(checkPosition).Type == matchType 
            && GetItemFromGrid(checkPosition).getPowerType == PowerType.none)
        {
            //Debug.Log("In While Loop for " + position + " Direction :" + direction + "Match Type: " + matchType);
            matches++;
            checkPosition += direction;
        }

        return matches;
    }
}
