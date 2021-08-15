using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementManager : MonoBehaviour
{
    public List<MovingEntity> entityList = new List<MovingEntity>();
    public GameObject entityPrefab;

    [Range(0, 1)] public float separateWeight = 0.5f;

    [Range(0, 1)] public float alineWeight = 0.5f;

    [Range(0, 1)] public float cohesionWeight = 0.5f;

    [Range(0, 1)] public float targetWeight = 0.5f;

    public float maxNeighborDistance = 20;

    public float maxSeparateDistance = 4;

    public float neighborDistance = 10;

    public float separateDistance = 2;

    public Camera mainCamera => Camera.main;

    void Start()
    {
    }

    void Update()
    {
        if (entityList.Count >= 2)
        {
            separateDistance =
                Mathf.PerlinNoise(entityList[0].transform.position.x, entityList[0].transform.position.y) *
                maxSeparateDistance;
            neighborDistance =
                Mathf.PerlinNoise(entityList[1].transform.position.x, entityList[1].transform.position.y) *
                maxNeighborDistance;
        }

        foreach (var entity in entityList)
        {
            var sf = entity.Separate(entityList, separateDistance);
            var af = entity.Aline(entityList, neighborDistance);
            var cf = entity.Cohesion(entityList, neighborDistance);
            var tf = entity.Seek(GetTarget());
            entity.AddForce((sf * separateWeight + af * alineWeight + cf * cohesionWeight + tf * targetWeight) /
                            (separateWeight + alineWeight + targetWeight + cohesionWeight));
        }
    }

    public void AddEntity(MovingEntity entity)
    {
        entityList.Add(entity);
    }

    public void AddEntity()
    {
        var mp = mainCamera.ScreenToWorldPoint(Vector3.zero);
        var np = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        var entity = GameObject.Instantiate(entityPrefab);
        entity.transform.position =
            new Vector3(Random.Range(np.x, mp.x), Random.Range(np.y, mp.y), Random.Range(-5, 5));
        entityList.Add(entity.GetComponent<MovingEntity>());
    }

    private Vector3 GetTarget()
    {
        var mp = Input.mousePosition;
        var sp = new Vector3(mp.x, mp.y, 100);
        return mainCamera.ScreenToWorldPoint(sp);
    }
}