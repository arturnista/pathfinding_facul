using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatMovement : MonoBehaviour
{

    [SerializeField] private float m_MoveSpeed = 10f;
    [SerializeField] private GameObject m_LookCamera;
    [SerializeField] private GameObject m_FirstPersonCamera;

    [Header("Effects")]
    [SerializeField] private List<GameObject> m_WalkEffects;
    [SerializeField] private GameObject m_ReachEffect;
    [SerializeField] private GameObject m_FireworksEffects;

    private List<Vector3> m_Path;
    private int m_PathIndex;

    private float m_Delay;

    private void Start()
    {
        // Debug.DrawRay(MazeGenerator.Instance.GetWorldPosition(new Vector3(1f, 0f, 0f)), Vector3.up * 10f, Color.yellow);
        // Debug.DrawRay(MazeGenerator.Instance.GetWorldPosition(new Vector3(20f, 0f, 18f)), Vector3.up * 10f, Color.blue);
        // Debug.Break();

        m_Path = null;
        m_LookCamera.SetActive(true);
        m_FirstPersonCamera.SetActive(false);
        StartCoroutine(MazeGenerator.Instance.FindPath(
            new Vector2Int(0, 1),
            new Vector2Int(18, 20),
            HandleFindPath
        ));
    }

    private void HandleFindPath(List<Vector3> path)
    {
        m_PathIndex = 0;
        m_Path = path;
        m_LookCamera.SetActive(false);
        m_FirstPersonCamera.SetActive(true);
        m_Delay = 3f;
    }

    private void Update()
    {
        if (m_Path == null) return;
        if (m_Path.Count <= m_PathIndex) return;
        if (m_Delay > 0f)
        {
            m_Delay -= Time.deltaTime;
            return;
        }

        var targetPosition = m_Path[m_PathIndex] + (Vector3.up * .7f);

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, m_MoveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) < .1f)
        {
            m_PathIndex += 1;
            if (m_Path.Count <= m_PathIndex)
            {
                Instantiate(m_ReachEffect, transform.position + transform.forward + transform.up, Quaternion.identity);
                StartCoroutine(ReachEffect());
            }
            else if (m_PathIndex % 5 == 0)
            {
                Instantiate(m_WalkEffects[Random.Range(0, m_WalkEffects.Count)], transform.position + transform.forward + transform.up, Quaternion.identity);
            }
        }

        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction, Vector3.up), 3f * Time.deltaTime);
    }

    private IEnumerator ReachEffect()
    {
        for (int i = 0; i < 15; i++)
        {
            Instantiate(m_FireworksEffects, transform.position + (Random.onUnitSphere * 2f), Quaternion.identity);
            yield return new WaitForSeconds(Random.Range(0f, .1f));
        }
    }

}
