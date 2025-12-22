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

        // 2. 좌표를 다 땄으므로, 이제 자식 오브젝트들은 필요 없습니다.
        // 자식을 파괴하거나 비활성화하여 드론이 움직일 때 연산되지 않도록 합니다.
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        // 3. (중요) 자기 자신도 부모(드론)와의 연결을 끊습니다.
        // 이제 이 스크립트가 붙은 오브젝트는 세계 어딘가에 고정됩니다.
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
