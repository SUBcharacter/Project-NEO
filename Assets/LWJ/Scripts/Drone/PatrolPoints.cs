using System.Collections.Generic;
using UnityEngine;

public class PatrolPoints : MonoBehaviour
{
    [SerializeField] private List<Vector3> points = new List<Vector3>();
    private bool isInitialized = false;
    public void Initialize()
    {
        points.Clear();
        foreach (Transform child in transform)
        {
            points.Add(child.position);
        }

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        transform.SetParent(null);
    }
    public Vector3 GetRandomPoint()
    {
        if (points.Count == 0) return transform.position;
        return points[Random.Range(0, points.Count)];
    }

    public Vector3 GetNextPoint(int currentIndex, out int nextIndex)
    {
        if (points.Count == 0)
        {
            nextIndex = currentIndex;
            return transform.position;
        }

        nextIndex = (currentIndex + 1) % points.Count;
        return points[nextIndex];
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var p in points)
        {
            if (p != null) Gizmos.DrawWireSphere(p, 0.3f);
        }
    }
}
