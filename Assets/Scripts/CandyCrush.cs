using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;

public class CandyCrush : MonoBehaviour
{
    public GameObject[,] candies;
    public GameObject candyPrefab;
    public int gridSizeX = 5;
    public int gridSizeY = 8;
    public float candySize = 1.0f;

    private GameObject selectedCandy;

    void Start()
    {
        candies = new GameObject[gridSizeX, gridSizeY];
        InitializeGrid();
    }

    void InitializeGrid()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 pos = new Vector3(x * candySize, y * candySize, 0);
                GameObject candy = Instantiate(candyPrefab, pos, Quaternion.identity);
                candy.GetComponent<Candy>().gridX = x;
                candy.GetComponent<Candy>().gridY = y;
                candies[x, y] = candy;
            }
        }
    }

    void SwapCandies(Vector2Int firstIndex, Vector2Int secondIndex)
    {
        GameObject firstCandy = candies[firstIndex.x, firstIndex.y];
        GameObject secondCandy = candies[secondIndex.x, secondIndex.y];

        // Swap positions
        firstCandy.transform.DOMove(secondCandy.transform.position, 0.5f);
        secondCandy.transform.DOMove(firstCandy.transform.position, 0.5f);

        // Update grid
        candies[firstIndex.x, firstIndex.y] = secondCandy;
        candies[secondIndex.x, secondIndex.y] = firstCandy;

        // Update candy grid indices
        firstCandy.GetComponent<Candy>().gridX = secondIndex.x;
        firstCandy.GetComponent<Candy>().gridY = secondIndex.y;
        secondCandy.GetComponent<Candy>().gridX = firstIndex.x;
        secondCandy.GetComponent<Candy>().gridY = firstIndex.y;
    }

    void CheckForMatches()
    {
        List<GameObject> matchedCandies = new List<GameObject>();

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                GameObject currentCandy = candies[x, y];
                if (currentCandy != null)
                {
                    List<GameObject> horizontalMatches = FindMatchesInDirection(currentCandy, Vector2Int.right);
                    List<GameObject> verticalMatches = FindMatchesInDirection(currentCandy, Vector2Int.up);

                    if (horizontalMatches.Count >= 2)
                    {
                        matchedCandies.AddRange(horizontalMatches);
                    }

                    if (verticalMatches.Count >= 2)
                    {
                        matchedCandies.AddRange(verticalMatches);
                    }
                }
            }
        }

        // Remove matched candies and add scores
        foreach (GameObject candy in matchedCandies)
        {
            candies[candy.GetComponent<Candy>().gridX, candy.GetComponent<Candy>().gridY] = null;
            Destroy(candy);
            // Add score or perform other actions here
        }
    }

    List<GameObject> FindMatchesInDirection(GameObject startCandy, Vector2Int direction)
    {
        List<GameObject> matches = new List<GameObject>();
        matches.Add(startCandy);

        Candy startCandyComponent = startCandy.GetComponent<Candy>();
        int startX = startCandyComponent.gridX;
        int startY = startCandyComponent.gridY;

        Candy.CandyType startCandyType = startCandyComponent.candyType; // Explicitly reference CandyType enum

        int currentX = startX + direction.x;
        int currentY = startY + direction.y;

        while (IsValidGridPosition(currentX, currentY) &&
               candies[currentX, currentY] != null &&
               candies[currentX, currentY].GetComponent<Candy>().candyType == startCandyType)
        {
            matches.Add(candies[currentX, currentY]);
            currentX += direction.x;
            currentY += direction.y;
        }

        return matches;
    }

    bool IsValidGridPosition(int x, int y)
    {
        return x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY;
    }

    void OnMouseDown()
    {
        if (selectedCandy == null)
        {
            selectedCandy = gameObject;
        }
        else
        {
            SwapCandies(selectedCandy.GetComponent<Candy>().GridPosition, gameObject.GetComponent<Candy>().GridPosition);
            selectedCandy = null;
            CheckForMatches();
        }
    }
}
