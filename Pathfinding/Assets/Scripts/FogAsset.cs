using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class FogAsset : PlayableAsset
{

    [SerializeField] private Color m_Color = default;
    [SerializeField] private float m_Density = .002f;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<FogBehaviour>.Create(graph);

        var shakeBehaviour = playable.GetBehaviour();
        shakeBehaviour.Color = m_Color;
        shakeBehaviour.Density = m_Density;
        return playable;
    }
    
}
