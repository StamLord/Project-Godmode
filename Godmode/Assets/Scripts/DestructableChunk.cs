using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructableChunk : Destructable
{
    public ChunkGrid cg;
    public bool isDestroyed;

    private void Start()
    {
        cg = GetComponentInParent<ChunkGrid>();
    }

    public override void Destruction()
    {
        if (isDestroyed)
            return;

        isDestroyed = true;
        cg.ChunkDestroyed(this);
        
        base.Destruction();
    }

    public override void Destruction(Vector3 direction, float force)
    {
        if (isDestroyed)
            return;

        isDestroyed = true;
        cg.ChunkDestroyed(this, direction, force);
        base.Destruction(direction, force);
    }
}
