using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class GridSystem<T> : Singleton<GridSystem<T>>
{
    private T[,] data;

    private Vector2Int dimensions = new Vector2Int (1, 1);

    public Vector2Int Dimensions
    {
        get
        {
            return dimensions;
        }
    }

    private bool isReady;

    public bool IsReady 
    { 
        get 
        { 
            return isReady; 
        } 
    }

    public void InitGridSystem(Vector2Int dimensions)
    {
        
        if(dimensions.x <= 0 || dimensions.y <= 0) 
        {
            Debug.LogError("Dimensions should be larger than 0");
        }
        this.dimensions = dimensions;
        Debug.Log(dimensions.x);
        Debug.Log(dimensions.y);
        data = new T[dimensions.x, dimensions.y];
        isReady = true;
    }

    public void ClearGrid()
    {
        data = new T[dimensions.x,dimensions.y];
    }

    public bool CheckBound(Vector2Int position)
    {
        if (!isReady)
        {
            Debug.LogError("Grid has not been initialized.");
        }
        if (position.x < 0 || position.x >= dimensions.x || position.y < 0 || position.y >= dimensions.y)
        {
            return false;
        }
        else return true;
    }
    
    public bool isEmpty(Vector2Int position)
    {

        if (!CheckBound(position))
            Debug.LogError("Not of bound");

        return EqualityComparer<T>.Default.Equals(data[position.x, position.y], default(T));
    }

    public bool PutItemOnGrid(Vector2Int position, T item, bool allowOverWrite = false)
    {
        if (!CheckBound(position))
            Debug.LogError("Not of bound");

        if (!allowOverWrite && !isEmpty(position))
            return false;

        data[position.x, position.y] = item;
        return true;
    }

    public T GetItemFromGrid(Vector2Int position)
    {
        if (!CheckBound(position))
            Debug.LogError("Not of bound");
       
        return data[position.x, position.y];
    }

    public T RemoveItemFromGrid(Vector2Int position)
    {
        if (!CheckBound(position))
        {
            Debug.LogError("Not of bound");
        }

        T temp;
        temp = data[position.x, position.y];
        data[position.x, position.y] = default(T);
        return temp;
    }

    public void SwapItems(Vector2Int pos1, Vector2Int pos2)
    {
        if (!CheckBound(pos1) || !CheckBound(pos2))
        {
            Debug.LogError("Not of bound");
        }

        T temp;
        temp = data[pos1.x, pos1.y];
        data[pos1.x, pos1.y] = data[pos2.x, pos2.y];
        data[pos2.x, pos2.y] = temp;
    }

    public override string ToString()
    {
        string s = "";
        if (data != null)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                s += "[";
                for (int x = 0; x < dimensions.x; x++)
                {
                    if (isEmpty(new Vector2Int(x, y)))
                        s += "x";   
                    else
                        s += data[x, y].ToString();

                    if (x != dimensions.x - 1)
                    {
                        s += ",";
                    }
                }

                s += "]\n";
            }
        }

        return s;
    }
}
