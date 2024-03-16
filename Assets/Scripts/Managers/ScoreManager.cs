using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ScoreManager : Singleton<ScoreManager>
{
    private Text scoreText;
    private MatchableGrid grid;
    private MatchablePool pool;

    [SerializeField]
    private Transform collectionPoint;

    private int score;

    public int Score {  get { return score; } }

    protected override void Init()
    {
        scoreText = GetComponent<Text>();   
    }
    private void Start()
    {
        pool = (MatchablePool)MatchablePool.Instance;
        grid = (MatchableGrid)MatchableGrid.Instance;
    }
    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = "Score : " + score;
    }

    public IEnumerator ResolveMatch(Match toResolve)
    {
        Matchable matchable;
        Matchable powerUpMatchable = null;
        Transform targetPoint = collectionPoint;
        bool isPowerUp = false;

        if (toResolve.Count > 3)
        {
            powerUpMatchable = toResolve.getPowerUpMatchable;
            if (toResolve.Orientation == Orientation.both)
            {
                pool.setPowerUpForMatchable(toResolve.getPowerUpMatchable, PowerType.cross);
            }
            else if (toResolve.Orientation == Orientation.horizontal || toResolve.Orientation == Orientation.vertical && toResolve.Count == 4)
            {
                pool.setPowerUpForMatchable(toResolve.getPowerUpMatchable, PowerType.match4);
            }
            else if (toResolve.Orientation == Orientation.horizontal || toResolve.Orientation == Orientation.vertical && toResolve.Count == 5)
            {
                pool.setPowerUpForMatchable(toResolve.getPowerUpMatchable, PowerType.match5);
            }
            toResolve.RemoveMatchable(powerUpMatchable);
            targetPoint = powerUpMatchable.transform;
            powerUpMatchable.spriteRenderer.sortingOrder = 3;

        }

        for (int i = 0; i != toResolve.Count; i++)
        {
            matchable = toResolve.Matchables[i];

            grid.RemoveItemFromGrid(matchable.position);
            if (powerUpMatchable != null)
            {
                targetPoint = powerUpMatchable.transform;
                isPowerUp = true;
            }
               
            if (i == toResolve.Count - 1)
            {
                yield return StartCoroutine(matchable.Resolve(targetPoint, isPowerUp));
            }
            else
            {
                StartCoroutine(matchable.Resolve(targetPoint, isPowerUp));
            }
            
        }

        AddScore(toResolve.Count * toResolve.Count);
        if (powerUpMatchable != null)
            powerUpMatchable.spriteRenderer.sortingOrder = 1;
        yield return null;
    }
}
