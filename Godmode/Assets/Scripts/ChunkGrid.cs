using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class ChunkGrid : MonoBehaviour
{
    public Destructable[] chunksBot = new Destructable[3];
    public Destructable[] chunksMid = new Destructable[3];
    public Destructable[] chunksTop = new Destructable[3];

    public ChunkGrid[] neighbors = new ChunkGrid[4];

    //Chunk to Destroy
    float x;
    float y;

    public void ChunkDestroyed(DestructableChunk chunk)
    {
        for (int i = 0; i < 3; i++)
        {
            if (chunksBot[i] == chunk)
            {
                x = i;
                y = 0;
                break;
            }
        }

        for (int i = 0; i < 3; i++)
        {
            if (chunksMid[i] == chunk)
            {
                x = i;
                y = 1;
                break;
            }
        }

        for (int i = 0; i < 3; i++)
        {
            if (chunksTop[i] == chunk)
            {
                x = i;
                y = 2;
                break;
            }
        }


        if(x == 0 && y == 0)
        {
            neighbors[0].chunksBot[2].Destruction();
            neighbors[3].chunksTop[0].Destruction();
        }
        else if (x == 0 && y == 1)
        {
            neighbors[0].chunksMid[2].Destruction();
        }
        else if (x == 0 && y == 2)
        {
            neighbors[0].chunksTop[0].Destruction();
            neighbors[1].chunksBot[0].Destruction();
        }
        else if (x == 1 && y == 0)
        {
            neighbors[3].chunksTop[1].Destruction();
        }
        else if (x == 1 && y == 1)
        {

        }
        else if (x == 1 && y == 2)
        {
            neighbors[1].chunksTop[1].Destruction();
        }
        else if (x == 2 && y == 0)
        {
            neighbors[2].chunksBot[0].Destruction();
            neighbors[3].chunksTop[2].Destruction();
        }
        else if (x == 2 && y == 1)
        {
            neighbors[2].chunksMid[0].Destruction();
        }
        else if (x == 2 && y == 2)
        {
            neighbors[1].chunksBot[2].Destruction();
            neighbors[2].chunksTop[0].Destruction();
        }

    }

    public void ChunkDestroyed(DestructableChunk chunk, Vector3 dir, float force)
    {
        for (int i = 0; i < 3; i++)
        {
            if (chunksBot[i] == chunk)
            {
                x = i;
                y = 0;
                break;
            }
        }

        for (int i = 0; i < 3; i++)
        {
            if (chunksMid[i] == chunk)
            {
                x = i;
                y = 1;
                break;
            }
        }

        for (int i = 0; i < 3; i++)
        {
            if (chunksTop[i] == chunk)
            {
                x = i;
                y = 2;
                break;
            }
        }


        if (x == 0 && y == 0)
        {
            if (neighbors[0])
                neighbors[0].chunksBot[2].Destruction(dir, force);
            if (neighbors[3])
                neighbors[3].chunksTop[0].Destruction(dir, force);
        }
        else if (x == 0 && y == 1)
        {
            if (neighbors[0])
                neighbors[0].chunksMid[2].Destruction(dir, force);
        }
        else if (x == 0 && y == 2)
        {
            if (neighbors[0])
                neighbors[0].chunksTop[0].Destruction(dir, force);
            if (neighbors[1])
                neighbors[1].chunksBot[0].Destruction(dir, force);
        }
        else if (x == 1 && y == 0)
        {
            if (neighbors[3])
                neighbors[3].chunksTop[1].Destruction(dir, force);
        }
        else if (x == 1 && y == 1)
        {

        }
        else if (x == 1 && y == 2)
        {
            if(neighbors[1])
                neighbors[1].chunksTop[1].Destruction(dir, force);
        }
        else if (x == 2 && y == 0)
        {
            if (neighbors[2])
                neighbors[2].chunksBot[0].Destruction(dir, force);
            if (neighbors[3])
                neighbors[3].chunksTop[2].Destruction(dir, force);
        }
        else if (x == 2 && y == 1)
        {
            if (neighbors[2])
                neighbors[2].chunksMid[0].Destruction(dir, force);
        }
        else if (x == 2 && y == 2)
        {
            if (neighbors[1])
                neighbors[1].chunksBot[2].Destruction(dir, force);
            if (neighbors[2])
                neighbors[2].chunksTop[0].Destruction(dir, force);
        }

    }

}
