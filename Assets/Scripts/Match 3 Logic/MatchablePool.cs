using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchablePool : ObjectPool<Matchable>
{
    [SerializeField] private int howManyTypes;
    [SerializeField] private Sprite[] sprites;

    [SerializeField] private Sprite crossPowerUp;
    [SerializeField] private Sprite match4PowerUp;
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

    public void setPowerUpForMatchable(Matchable matchable, PowerType powerType)
    {
        if (powerType == PowerType.match4)
            matchable.setPowerUp(match4PowerUp, powerType);
        
        else if (powerType == PowerType.match5)
            matchable.setPowerUp(match5PowerUp, powerType);
        
        else if (powerType == PowerType.cross)
            matchable.setPowerUp(crossPowerUp, powerType);
    }
}
