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

public class Match
{
    private Orientation orientation = Orientation.none;
    private int unlisted = 0;
    private List<Matchable> matchables;
    public List<Matchable> Matchables { get { return matchables; } }
    public int Count {  get { return matchables.Count + unlisted; } }
    public Orientation Orientation { get { return orientation; } }
    public Match()
    {
        matchables = new List<Matchable>(5);
    }

    public Match(Matchable original) : this()
    {
        AddMatchable(original);
    }
    public bool Contains(Matchable matchable)
    {
        return matchables.Contains(matchable);
    } 
    public void AddUnlisted()
    {
        unlisted++;
    }

    public void Merge(Match toMerge)
    {
        matchables.AddRange(toMerge.matchables);
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
