using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleView : EntityView
{
    public ParticleSystem particles;

    public override void SetVisible(bool state)
    {
        if (state) particles.Play();
        else       particles.Stop();
    }
}