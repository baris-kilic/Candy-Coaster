using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Orientation
{
    none,
    horizontal,
    vertical,
    both
}

public enum PowerType
{
    none,
    match4,
    match5,
    cross,
}

public class Match
{
    private Orientation orientation = Orientation.none;
    private int unlisted = 0;
    private List<Matchable> matchables;
    private Matchable toPowerUp = null;

    public Matchable getPowerUpMatchable 
    {  
        get 
        {
            if (toPowerUp != null)
                return toPowerUp; 
            else 
                return matchables[Random.Range(0,matchables.Count)];
        } 
    } 
    public List<Matchable> Matchables { get { return matchables; } }
    public int Count {  get { return matchables.Count + unlisted; } }
    public Orientation Orientation { get { return orientation; } }
    public Match()
    {
        matchables = new List<Matchable>(5);
    }
    public void setOrientation(Orientation orientation) {  this.orientation = orientation; }
    public Match(Matchable original) : this()
    {
        AddMatchable(original);
        toPowerUp = original;
    }
    public bool Contains(Matchable matchable)
    {
        return matchables.Contains(matchable);
    } 
    public void AddUnlisted()
    {
        unlisted++;
    }
    public void RemoveMatchable(Matchable matchable)
    {
        matchables.Remove(matchable);
    }
    public void Merge(Match toMerge)
    {
        matchables.AddRange(toMerge.matchables);

        if (
            orientation == Orientation.both
         || toMerge.orientation == Orientation.both
         || (orientation == Orientation.horizontal && toMerge.orientation == Orientation.vertical)
         || (orientation == Orientation.vertical && toMerge.orientation == Orientation.horizontal)
         )
            orientation = Orientation.both;

        else if (toMerge.orientation == Orientation.horizontal)
            orientation = Orientation.horizontal;

        else if (toMerge.orientation == Orientation.vertical)
            orientation = Orientation.vertical;
    }

    public void AddMatchable(Matchable toAdd)
    {
        matchables.Add(toAdd);
    }
    public override string ToString()
    {
        string s = "Match of type " + matchables[0].Type + " : ";

        foreach (Matchable m in matchables)
        {
            s += "(" + m.position.x + ", " + m.position.y + ") ";
        }

        return s;
    }
}
