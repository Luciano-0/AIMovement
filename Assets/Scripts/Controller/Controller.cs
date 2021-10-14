using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public List<MovingEntity> entityList = new List<MovingEntity>();
    public GameObject entityPrefab;
    public Camera mainCamera => Camera.main;
    

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

    protected Vector3 GetTarget()
    {
        var mp = Input.mousePosition;
        var sp = new Vector3(mp.x, mp.y, 100);
        return mainCamera.ScreenToWorldPoint(sp);
    }
}