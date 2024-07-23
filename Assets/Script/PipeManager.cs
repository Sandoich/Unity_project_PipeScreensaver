using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 

This code is for the project of automatically creating a pipeline (similar to the Win95 screensaver) in Unity. 
You can set your own range for the space; here, I have set the height, width, and length to 20. 

The main idea is using GrowPipe() to find the next valid direction and position, 
then using CreateNewPipe() to make a new chunk to connected with the previous one.

Hope you can enjoy it!


*/

public class PipeManager : MonoBehaviour
{
    // Prefab for straight pipes, used to generate new pipe segments
    public GameObject straightPipePrefab;

    // Length of each pipe segment
    public float pipeLength = 2f;

    // Dimensions of the 3D range (length, width, height)
    public float rangeLength = 20f; // Length in x direction
    public float rangeWidth = 20f; // Width in y direction
    public float rangeHeight = 20f; // Height in z direction

    // Current position of the pipe segment
    private Vector3 currentPos;

    // Current direction of the pipe segment
    private Vector3 currentDirection;

    // Set to store pipe positions, used to check for overlap
    private HashSet<Vector3> pipePositions = new HashSet<Vector3>();

    // Current color
    private Color currentColor;

    void Start()
    {
        // Starting point at (0, 0, 0)
        currentPos = Vector3.zero;

        // Randomly choose an initial direction
        currentDirection = RandomDirection();

        // Add the starting position to the set
        pipePositions.Add(currentPos);

        // Randomly choose an initial color
        currentColor = RandomColor();

        // Create the first straight pipe
        CreateNewPipe(currentPos, currentDirection);

    }

    void Update()
    {

        // Call GrowPipe method every frame
         GrowPipe();

    }

    void GrowPipe()
    {
        // Get the next valid direction and position
        Vector3 nextPosition;
        Vector3 nextDirection = GetValidDirection(out nextPosition);

        // If a valid direction is found, generate a new pipe
        if (nextDirection != Vector3.zero)
        {
            CreateNewPipe(currentPos, nextDirection);
        }
        else
        {
            // If no available direction, print a warning
            Debug.LogWarning("No available direction to generate new pipe");

            // Find a new start position and continue growing
            currentPos = FindNewStartPosition();
            currentDirection = RandomDirection();

            // Choose a random color for the new start position
            currentColor = RandomColor();
            
            CreateNewPipe(currentPos, currentDirection);
        }
    }

    Vector3 GetValidDirection(out Vector3 nextPosition)
    {
        // Define all possible directions
        List<Vector3> directions = new List<Vector3>
        {
            Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back
        };

        // Shuffle the list of directions
        Shuffle(directions);

        // Iterate through all directions to check if a new pipe can be generated
        foreach (Vector3 direction in directions)
        {
            // Calculate the end position of the new pipe
            nextPosition = currentPos + direction * pipeLength;

            // Check if the new position is within bounds and doesn't overlap with existing pipes
            if (IsWithinBounds(nextPosition) && !pipePositions.Contains(nextPosition))
            {
                return direction;
            }
        }

        // If no valid direction is found, return Vector3.zero
        nextPosition = Vector3.zero;
        return Vector3.zero;
    }

    void CreateNewPipe(Vector3 position, Vector3 direction)
    {
        // Calculate the end position of the new pipe
        Vector3 adjustedPosition = position + direction * pipeLength;

        // Calculate the rotation of the new pipe to match the current direction
        Quaternion rotation = Quaternion.LookRotation(direction);

        // Create the new pipe
        GameObject newPipe = Instantiate(
            straightPipePrefab,
            position, // Use the current position as the start point
            rotation
        );

        // Get all renderer components
        Renderer[] renderers = newPipe.GetComponentsInChildren<Renderer>();

        // Set the current color for all renderers
        foreach (Renderer renderer in renderers)
        {
            renderer.material.color = currentColor;
        }

        // Update currentPos to point to the end of the new pipe
        currentPos = adjustedPosition;

        // Add the new pipe position to the set
        pipePositions.Add(currentPos);

        // Output debug information
        Debug.Log($"Created pipe at {currentPos} with direction {direction}");
        Debug.Log($"Current pipe end position: {adjustedPosition}");
    }

    Vector3 RandomDirection()
    {
        // Return a random direction (up, down, left, right, forward, back)
        int rand = Random.Range(0, 6);
        switch (rand)
        {
            case 0: return Vector3.up;
            case 1: return Vector3.down;
            case 2: return Vector3.left;
            case 3: return Vector3.right;
            case 4: return Vector3.forward;
            case 5: return Vector3.back;
        }
        return Vector3.zero;
    }

    // Randomly generate a color
    Color RandomColor()
    {
        return new Color(Random.value, Random.value, Random.value);
    }

    // Shuffle the list of elements
    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            // Temporarily store the current element
            T temp = list[i];

            // Randomly choose an index
            int randomIndex = Random.Range(i, list.Count);

            // Swap the current element with the randomly chosen element
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // Check if the position is within bounds
    bool IsWithinBounds(Vector3 position)
    {
        return position.x >= -rangeLength / 2 && position.x <= rangeLength / 2 &&
               position.y >= -rangeWidth / 2 && position.y <= rangeWidth / 2 &&
               position.z >= -rangeHeight / 2 && position.z <= rangeHeight / 2;
    }

    // Find a new starting position
    Vector3 FindNewStartPosition()
    {
        Vector3 newPosition;
        int attempts = 0;

        // Try to find an empty position
        do
        {
            newPosition = new Vector3(
                Random.Range(-rangeLength / 2, rangeLength / 2),
                Random.Range(-rangeWidth / 2, rangeWidth / 2),
                Random.Range(-rangeHeight / 2, rangeHeight / 2)
            );
            attempts++;
        } while (pipePositions.Contains(newPosition) && attempts < 100);

        if (attempts >= 100)
        {
            Debug.LogError("Unable to find a new starting position");
            return Vector3.zero;
        }

        return newPosition;
    }
}
