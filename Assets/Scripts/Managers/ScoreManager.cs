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
    private AudioManager audioManager;

    [SerializeField]
    private Transform collectionPoint;

    private int score;

    public int Score { get { return score; } }

    protected override void Init()
    {
        scoreText = GetComponent<Text>();
    }

    public void Reset()
    {
        score = 0;
        scoreText.text = "Score: " + score.ToString();
    }

    private void Start()
    {
        pool = (MatchablePool)MatchablePool.Instance;
        grid = (MatchableGrid)MatchableGrid.Instance;
        audioManager = AudioManager.Instance;
    }
    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = "Score : " + score;
    }

    //Resolving matches with power up boolean (if player uses powerup, it is true)
    public IEnumerator ResolveMatch(Match toResolve, bool fromPowerUp)
    {
        Matchable matchable;
        Matchable powerUpMatchable = null;
        Transform targetPoint = collectionPoint;
        bool isPowerUp = false;

        //if the match is not coming from a power up and the count is bigger than 3, getting powerupMatchable
        //from match and change its type, sprite and sorting order.
        if (!fromPowerUp && toResolve.Count > 3)
        {
            powerUpMatchable = pool.setPowerUpForMatchable(toResolve.getPowerUpMatchable, toResolve.Type);
            toResolve.RemoveMatchable(powerUpMatchable);
            targetPoint = powerUpMatchable.transform;
            powerUpMatchable.spriteRenderer.sortingOrder = 3;

            audioManager.PlaySound(SoundEffects.upgrade);
        } else
        {
            audioManager.PlaySound(SoundEffects.resolve);
        }

        //For every matchable in match, we remove them from grid and move into target point.
        //if the match is bigger than 3, we move the matchables to the power up matchable and merge them.
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
        //Add score by get square of the count of the match.
        AddScore(toResolve.Count * toResolve.Count);
        //if we have power up matchable, we reset it's sorting order.
        if (powerUpMatchable != null)
            powerUpMatchable.spriteRenderer.sortingOrder = 1;
        yield return null;
    }

}