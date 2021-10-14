using UnityEngine;

public class BoidsController : Controller
{
    [Range(0, 1)] public float separateWeight = 0.5f;

    [Range(0, 1)] public float alineWeight = 0.5f;

    [Range(0, 1)] public float cohesionWeight = 0.5f;

    [Range(0, 1)] public float targetWeight = 0.5f;

    public bool RandomBool = false;

    // public float maxNeighborDistance = 20;
    //
    // public float maxSeparateDistance = 4;

    public float neighborDistance = 10;

    public float separateDistance = 2;

    void Update()
    {
        // if (entityList.Count >= 2)
        // {
        //     separateDistance =
        //         Mathf.PerlinNoise(entityList[0].transform.position.x, entityList[0].transform.position.y) *
        //         maxSeparateDistance;
        //     neighborDistance =
        //         Mathf.PerlinNoise(entityList[1].transform.position.x, entityList[1].transform.position.y) *
        //         maxNeighborDistance;
        // }
        if (RandomBool)
        {
            if (entityList.Count >= 3)
            {
                separateWeight =
                    Mathf.PerlinNoise(entityList[0].transform.position.x, entityList[0].transform.position.y);
                alineWeight = Mathf.PerlinNoise(entityList[1].transform.position.x, entityList[1].transform.position.y);
                cohesionWeight =
                    Mathf.PerlinNoise(entityList[2].transform.position.x, entityList[2].transform.position.y);
            }
        }

        foreach (var entity in entityList)
        {
            var sf = entity.Separate(entityList, separateDistance);
            var af = entity.Align(entityList, neighborDistance);
            var cf = entity.Cohesion(entityList, neighborDistance);
            var tf = entity.Seek(GetTarget());
            entity.AddForce((sf * separateWeight + af * alineWeight + cf * cohesionWeight + tf * targetWeight) /
                            (separateWeight + alineWeight + targetWeight + cohesionWeight));
        }
    }
}