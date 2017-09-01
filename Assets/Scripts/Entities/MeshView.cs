using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshView : EntityView
{
    public Collider collision;
    public Renderer renderMesh;

    public override void SetVisible(bool state)
    {
        renderMesh.enabled = state;
        collision.enabled = state;
    }
}
