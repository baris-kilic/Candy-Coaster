using UnityEngine;

public class Candy : MonoBehaviour
{
    // Grid position of the candy
    public static Candy instance;

    public Candy getInstance()
    {
        if (instance == null)
        {
            instance = new Candy();
        }
        return instance;
    }

    public int gridX;
    public int gridY;

    // Type of candy
    public CandyType candyType;

    // Enum for different candy types
    public enum CandyType
    {
        Candy1,
        Candy2,
        Candy3,
        Candy4,
        Candy5
        // Add more candy types if needed
    }

    // Method to set the candy type
    public void SetCandyType(CandyType type)
    {
        candyType = type;
        // Adjust the candy appearance based on its type (e.g., change sprite)
        // You can customize this method further based on your game's requirements
    }

    // Method to retrieve the grid position of the candy
    public Vector2Int GridPosition => new Vector2Int(gridX, gridY);
}
