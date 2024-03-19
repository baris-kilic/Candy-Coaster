using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchablePool : ObjectPool<Matchable>
{
    [SerializeField] private int howManyTypes;
    [SerializeField] private Sprite[] sprites;
    public bool allowSwap = true;

    [SerializeField] private Sprite crossPowerUp;
    [SerializeField] private Sprite match4VerticalPowerUp;
    [SerializeField] private Sprite match4HorizontalPowerUp;
    [SerializeField] private Sprite match5PowerUp;
    public void RandomizeType(Matchable toRandomize)
    {
        int random = Random.Range(0,howManyTypes);
        toRandomize.SetType(random, sprites[random]);

    }

    public Matchable GetRandomMatchable()
    {
        Matchable matchable = GetPooledObject();
        RandomizeType(matchable);
        return matchable;
    }

    public int NextType(Matchable newMatchable)
    {
        int nextType = (newMatchable.Type + 1) % howManyTypes;
        newMatchable.SetType(nextType, sprites[nextType]);
        return nextType;
    }

    public Matchable setPowerUpForMatchable(Matchable matchable, PowerType powerType)
    {
        if (powerType == PowerType.match4vertical)
            return matchable.setPowerUp(match4VerticalPowerUp, powerType);

        else if (powerType == PowerType.match4horizontal)
            return matchable.setPowerUp(match4HorizontalPowerUp, powerType);

        else if (powerType == PowerType.match5)
            return matchable.setPowerUp(match5PowerUp, powerType);
        
        else if (powerType == PowerType.cross)
            return matchable.setPowerUp(crossPowerUp, powerType);

        return matchable;
    }

    public void ChangeType(Matchable matchable, int v)
    {
        matchable.SetType(v, sprites[v]);
    }
}
