using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum MoveType
{
    Stop,
    Seek,
    Flee,
    Arrive,
    Pursuit,
    Evade,
    OffsetPursuit,
    Interpose,
    Wander,
}

public class CommonController : Controller
{
    public GameObject blockPrefab;
    public List<Block> blockList = new List<Block>();

    public Dropdown dropdown;
    public MoveType type;
    public float minBlockRadius = 5;
    public float maxBlockRadius = 15;

    [Header("Flee")] public float keepDistance = 5;
    [Header("Arrive")] public float slowDownDis = 4;
    [Header("Pursuit")] public float pursuitDistance = 1;
    [Header("OffsetPursuit")] public List<Vector3> pursuitOffset;
    [Header("Wander")] public float wanderRadius = 10;
    public float wanderDistance = 5;
    public float wanderJitter = 3;
    public bool wanderLimit = true;

    void Start()
    {
        dropdown.onValueChanged.AddListener(OnTypeChanged);
    }

    void Update()
    {
        switch (type)
        {
            case MoveType.Seek:
                foreach (var entity in entityList)
                {
                    entity.AddForce(entity.Seek(GetTarget()));
                }

                break;
            case MoveType.Flee:
                foreach (var entity in entityList)
                {
                    entity.AddForce(entity.Flee(GetTarget(), keepDistance));
                }

                break;
            case MoveType.Arrive:
                foreach (var entity in entityList)
                {
                    entity.AddForce(entity.Arrive(GetTarget(), slowDownDis));
                }

                break;
            case MoveType.Pursuit:
                if (entityList.Count < 2)
                    Debug.LogError("至少添加两个单位！");
                else
                {
                    var target = entityList[0];
                    target.AddForce(target.Arrive(GetTarget(), slowDownDis));
                    for (int i = 1; i < entityList.Count; i++)
                        entityList[i].AddForce(entityList[i].Pursuit(target, pursuitDistance));
                }

                break;
            case MoveType.Evade:
                if (entityList.Count < 2)
                    Debug.LogError("至少添加两个单位！");
                else
                {
                    var target = entityList[0];
                    target.AddForce(target.Arrive(GetTarget(), slowDownDis));
                    for (int i = 1; i < entityList.Count; i++)
                        entityList[i].AddForce(entityList[i].Evade(target, keepDistance));
                }

                break;
            case MoveType.OffsetPursuit:
                if (entityList.Count < 2)
                    Debug.LogError("至少添加两个单位！");
                else if (entityList.Count - 1 > pursuitOffset.Count)
                    Debug.LogError("缺少offset配置！");
                else
                {
                    var target = entityList[0];
                    target.AddForce(target.Arrive(GetTarget(), slowDownDis));
                    for (int i = 1; i < entityList.Count; i++)
                        entityList[i].AddForce(entityList[i].OffsetPursuit(target, pursuitOffset[i - 1]));
                }

                break;
            case MoveType.Interpose:
                if (entityList.Count < 3)
                    Debug.LogError("至少添加三个单位！");
                else
                {
                    var targetA = entityList[0];
                    var targetB = entityList[1];
                    targetA.AddForce(targetA.Wander(wanderRadius, wanderDistance, wanderJitter, true));
                    targetB.AddForce(targetB.Wander(wanderRadius, wanderDistance, wanderJitter, true));
                    for (var i = 2; i < entityList.Count; i++)
                        entityList[i].AddForce(entityList[i].Interpose(targetA, targetB));
                }

                break;
            case MoveType.Wander:
                foreach (var entity in entityList)
                {
                    entity.AddForce(entity.Wander(wanderRadius, wanderDistance, wanderJitter, wanderLimit));
                }

                break;
            default:
                foreach (var entity in entityList)
                {
                    entity.Stop();
                }

                break;
        }
    }

    private void OnTypeChanged(int i)
    {
        type = (MoveType)i;
    }

    public void AddBlock()
    {
        var mp = mainCamera.ScreenToWorldPoint(Vector3.zero);
        var np = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        var block = GameObject.Instantiate(blockPrefab);
        block.transform.position =
            new Vector3(Random.Range(np.x, mp.x), Random.Range(np.y, mp.y), 0);
        var b = block.GetComponent<Block>();
        blockList.Add(b);
        b.radius = Random.Range(minBlockRadius, maxBlockRadius);
        b.transform.localScale = new Vector3(b.radius, 6, b.radius);
    }
}