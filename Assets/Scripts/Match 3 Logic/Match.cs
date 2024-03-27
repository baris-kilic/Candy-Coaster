using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//Orientation enum class for getting information about is match vertical, horizontal and
//both class for cross matches.
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
    match4vertical,
    match4horizontal,
    match5,
    cross,
}

public class Match
{
    private Orientation orientation = Orientation.none;
    private int unlisted = 0;
    private List<Matchable> matchables;
    private Matchable toPowerUp = null;

    //Return the power type of the match by looking it's count and orientation.
    public PowerType Type 
    {
        get 
        {
            if (orientation == Orientation.both)
                return PowerType.cross;
            else if (matchables.Count > 4)
                return PowerType.match5;
            else if (matchables.Count == 4 && orientation == Orientation.vertical)
                return PowerType.match4vertical;
            else if (matchables.Count == 4 && orientation == Orientation.horizontal)
                return PowerType.match4horizontal;
            else
                return PowerType.none;
        }
    }

    //If we have a power up matchable(it can be done with swapping last element of the match, not coming
    //from chain reactions, it should be done by player),return it. If not, return the random matchable from match.
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

    //Merge function by looking the orientation of coming match and this match. It changes the orientation of these.
    //if the orientation is same (the match is vertical and the other match is vertical also), it doesn't change the orientation type.
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
}
