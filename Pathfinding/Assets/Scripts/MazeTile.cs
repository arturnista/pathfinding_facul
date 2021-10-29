using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeTile : MonoBehaviour
{

    [SerializeField] private GameObject m_CurrentMesh;
    [SerializeField] private GameObject m_CurrentActive;
    [SerializeField] private GameObject m_CurrentClosed;
    [SerializeField] private GameObject m_CurrentPath;

    public void SetStatus(Tile.Status status)
    {
        m_CurrentMesh.SetActive(false);
        m_CurrentActive.SetActive(false);
        m_CurrentClosed.SetActive(false);
        m_CurrentPath.SetActive(false);
        switch (status)
        {
            case Tile.Status.Current:
                m_CurrentMesh.SetActive(true);
                break;
            case Tile.Status.Active:
                m_CurrentActive.SetActive(true);
                break;
            case Tile.Status.Closed:
                m_CurrentClosed.SetActive(true);
                break;
            case Tile.Status.Path:
                m_CurrentPath.SetActive(true);
                break;
        }
    }

}
