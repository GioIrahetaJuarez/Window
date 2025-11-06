using UnityEngine;

public class DangerZoneSpawner : MonoBehaviour
{
    public int dangerZoneCount = 4;
    public float minDistanceFromCenter = 1.5f;
    public float maxDistanceFromCenter = 3.5f;
    public float minDistanceBetween = 1.0f;
    public GameObject dangerZonePrefab;

    void Start()
    {
        SpawnDangerZones();
    }

    void SpawnDangerZones()
    {
        Vector2[] positions = GenerateRandomPositions();
        
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject zone;
            
            if (dangerZonePrefab != null)
            {
                zone = Instantiate(dangerZonePrefab, positions[i], Quaternion.identity);
            }
            else
            {
                zone = new GameObject($"DangerZone_{i}");
                zone.AddComponent<WinPoint>();
                zone.transform.position = positions[i];
            }
        }
    }

    Vector2[] GenerateRandomPositions()
    {
        Vector2[] positions = new Vector2[dangerZoneCount];
        
        for (int i = 0; i < dangerZoneCount; i++)
        {
            bool validPosition = false;
            int attempts = 0;
            
            while (!validPosition && attempts < 100)
            {
                float distance = Random.Range(minDistanceFromCenter, maxDistanceFromCenter);
                float angle = Random.Range(0f, Mathf.PI * 2f);
                Vector2 newPos = new Vector2(
                    Mathf.Cos(angle) * distance,
                    Mathf.Sin(angle) * distance
                );
                
                validPosition = true;
                for (int j = 0; j < i; j++)
                {
                    if (Vector2.Distance(newPos, positions[j]) < minDistanceBetween)
                    {
                        validPosition = false;
                        break;
                    }
                }
                
                if (validPosition)
                {
                    positions[i] = newPos;
                }
                
                attempts++;
            }
        }
        
        return positions;
    }
}
