using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class FogBehaviour : PlayableBehaviour
{

    public Color Color;
    public float Density;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        RenderSettings.fogColor = Color;
        RenderSettings.fogDensity = Density * info.effectiveWeight;
    }

}
