using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class JobManager : Singleton<JobManager>
{
    public static readonly System.Random RNG = new System.Random();

    public struct SpaceShipMoveJob : IJobParallelForTransform
    {
        [ReadOnly]
        public Vector3 vec3CometPosition;

        [ReadOnly]
        public float fDeltaTime;

        [ReadOnly]
        public NativeList<Vector3> nl_InitialPos;

        [ReadOnly]
        public NativeArray<Vector3> na_PosOffset;

        public void Execute(int index, TransformAccess transform)
        {
            float step = GameManager.instance.ObjectSpeed * fDeltaTime;

            // musimy sami liczyc forward gdyz TransformAccess nie ma pola [forward] !!!
            Vector3 forward = transform.rotation * Vector3.forward;
            Vector3 dir = Vector3.RotateTowards(forward, vec3CometPosition + na_PosOffset[index], step, 0.0f);
            transform.rotation = Quaternion.LookRotation(dir);

            Vector3 pos = Vector3.MoveTowards(transform.position, vec3CometPosition + nl_InitialPos[index], step);

            Vector3 posFlat = new Vector3(pos.x, pos.y, 0);
            Vector3 maxPos = nl_InitialPos[index] + na_PosOffset[index];
            Vector3 flatDir = maxPos - posFlat;
            if (flatDir.magnitude > 0.1)
                pos += flatDir.normalized * fDeltaTime;

            transform.position = pos;
        }
    }

    public struct SpaceShipOffsetChangeJob : IJobParallelFor
    {
        [WriteOnly]
        public NativeArray<Vector3> na_PosOffset;
        
        public void Execute(int index)
        {
            float x = (float)JobManager.RNG.NextDouble();
            x = (x * 5) - 2.5f;

            float y = (float)JobManager.RNG.NextDouble();
            y = (y * 5) - 2.5f;

            na_PosOffset[index] = new Vector3(x, y, 0);        }
    }

    public struct RocketMoveJob : IJobParallelForTransform
    {
        [ReadOnly]
        public Vector3 vec3CometPosition;

        [ReadOnly]
        public float fDeltaTime;

        [ReadOnly]
        public NativeArray<int> RHits;

        // 0 - not used, 1 - used, 2 - used & close to comet
        public NativeArray<int> naUsedRockets;

        public NativeArray<float> naRotationSteps;

        public void Execute(int index, TransformAccess transform)
        {
            if (naUsedRockets[index] == 0)
            {
                naRotationSteps[index] = 0;
                transform.position = Vector3.zero;
                transform.rotation = Quaternion.identity;
                return;
            }

            float step = GameManager.instance.RocketSpeed * fDeltaTime;
            naRotationSteps[index] += step * 5;

            Vector3 forward = transform.rotation * Vector3.forward;
            Vector3 dir = Vector3.RotateTowards(forward, vec3CometPosition, step, 0.0f);
            transform.rotation = Quaternion.LookRotation(dir);

            transform.position = Vector3.MoveTowards(transform.position, vec3CometPosition, step);

            //if (Vector3.Distance(transform.position, vec3CometPosition) < 60.0f)
            if (RHits[index] == 1)
            {
                Vector3 eulers = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(new Vector3(eulers.x, eulers.y, eulers.z + naRotationSteps[index]));
                naUsedRockets[index] = 2;
            }
        }
    }

    private SpaceShipMoveJob m_spaceShipMove;
    private JobHandle m_spaceShipMoveJH;

    private SpaceShipOffsetChangeJob m_spaceShipOffsetChange;
    private JobHandle m_spaceShipOffsetChangeJH;

    private RocketMoveJob m_rocketMove;
    private JobHandle m_rocketMoveJH;

    private TransformAccessArray m_spaceShipTAA;
    private NativeList<Vector3> m_nlInitialPos;
    private NativeArray<Vector3> m_naPosOffsets;
    private TransformAccessArray m_spaceShipRocketDocksTAA;

    private NativeArray<int> m_naUsedRockets;
    private NativeArray<float> m_naRocketsRotationSteps;
    private TransformAccessArray m_rocketsTAA;

    private List<Rocket> m_rocketComponentList = new List<Rocket>();

    private NativeArray<RaycastCommand> m_naRCommands;
    private NativeArray<RaycastHit> m_naRHits;
    private JobHandle m_raycastJH;

    void Start ()
    {
        // trzeba tworzyc array z przynajmniej zerowym capacity 
        // konstruktor domyslny nie tworzy instancji!!!
        m_spaceShipTAA = new TransformAccessArray(0);
        m_nlInitialPos = new NativeList<Vector3>(Allocator.Persistent);
        m_naPosOffsets = new NativeArray<Vector3>(0, Allocator.Persistent);
        m_spaceShipRocketDocksTAA = new TransformAccessArray(0);

        var rocketPool = PoolManager.instance.RocketPool.m_pool;
        m_naUsedRockets = new NativeArray<int>(rocketPool.Count, Allocator.Persistent);
        m_naRocketsRotationSteps = new NativeArray<float>(rocketPool.Count, Allocator.Persistent);
        m_rocketsTAA = new TransformAccessArray(rocketPool.Count);
        for (int i = 0; i < rocketPool.Count; ++i)
        {
            m_rocketsTAA.Add(rocketPool[i].transform);
            m_rocketComponentList.Add(rocketPool[i].GetComponent<Rocket>());
        }

        m_naRCommands = new NativeArray<RaycastCommand>(rocketPool.Count, Allocator.Persistent);
        m_naRHits = new NativeArray<RaycastHit>(rocketPool.Count, Allocator.Persistent);

        StartCoroutine(CyclicPosChange());
        StartCoroutine(CyclicShooting());
    }

    IEnumerator CyclicPosChange()
    {
        yield return new WaitForSeconds(2);

        while (true)
        {
            m_spaceShipMoveJH.Complete();

            m_spaceShipOffsetChange = new SpaceShipOffsetChangeJob()
            {
                na_PosOffset = m_naPosOffsets
            };

            m_spaceShipOffsetChangeJH = m_spaceShipOffsetChange.Schedule(m_naPosOffsets.Length, 8);

            yield return new WaitForSeconds(5);
        }
    }

    IEnumerator CyclicShooting()
    {
        yield return new WaitForSeconds(2);

        while (true)
        {
            m_rocketMoveJH.Complete();

            for (int i = 0; i < m_spaceShipRocketDocksTAA.length; ++i)
                PoolManager.instance.RocketPool.SpawnObject(m_spaceShipRocketDocksTAA[i].position, m_spaceShipRocketDocksTAA[i].rotation);

            UpdateUsedRocketState();

            yield return new WaitForSeconds(1);
        }
    }

    void Update ()
    {
        m_spaceShipMove = new SpaceShipMoveJob()
        {
            vec3CometPosition = GameManager.instance.Comet.position,
            fDeltaTime = Time.deltaTime,
            nl_InitialPos = m_nlInitialPos,
            na_PosOffset = m_naPosOffsets
        };

        m_spaceShipMoveJH = m_spaceShipMove.Schedule(m_spaceShipTAA, m_spaceShipOffsetChangeJH);

        int raycastMask = 1 << LayerMask.NameToLayer("Comet");
        for (int i = 0; i < m_rocketsTAA.length; ++i)
        {
            Vector3 dir = GameManager.instance.Comet.position - m_rocketsTAA[i].position;
            m_naRCommands[i] = new RaycastCommand(m_rocketsTAA[i].position, dir.normalized, 15.0f, raycastMask);
        }

        m_raycastJH = RaycastCommand.ScheduleBatch(m_naRCommands, m_naRHits, 8);

        m_raycastJH.Complete();

        NativeArray<int> mHits = new NativeArray<int>(m_naRHits.Length, Allocator.Temp);
        for (int i = 0; i < m_naRHits.Length; ++i)
            mHits[i] = m_naRHits[i].collider != null ? 1 : 0;

        m_rocketMove = new RocketMoveJob()
        {
            vec3CometPosition = GameManager.instance.Comet.position,
            fDeltaTime = Time.deltaTime,
            RHits = mHits,
            naUsedRockets = m_naUsedRockets,
            naRotationSteps = m_naRocketsRotationSteps
        };

        m_rocketMoveJH = m_rocketMove.Schedule(m_rocketsTAA, m_raycastJH);

        JobHandle.ScheduleBatchedJobs();

        UpdateRockets();

        mHits.Dispose();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();

        m_spaceShipTAA.Dispose();
        m_nlInitialPos.Dispose();
        m_naPosOffsets.Dispose();
        m_spaceShipRocketDocksTAA.Dispose();

        m_naUsedRockets.Dispose();
        m_naRocketsRotationSteps.Dispose();
        m_rocketsTAA.Dispose();

        m_naRCommands.Dispose();
        m_naRHits.Dispose();
    }

    public void UpdateRockets()
    {
        m_rocketMoveJH.Complete();

        for (int i = 0; i < m_rocketComponentList.Count; ++i)
            m_rocketComponentList[i].UpdateState(m_naUsedRockets[i] == 2);

        UpdateUsedRocketState();
    }

    public void UpdateUsedRocketState()
    {
        var used = PoolManager.instance.RocketPool.UsedObjects;
        for (int i = 0; i < used.Count; ++i)
            m_naUsedRockets[i] = used[i] ? 1 : 0;
    }

    public void AddSpaceShipData(GameObject i_spaceShipGO, Vector3 i_initialPos)
    {
        m_spaceShipMoveJH.Complete();
        m_spaceShipOffsetChangeJH.Complete();

        m_spaceShipTAA.capacity = m_spaceShipTAA.length + 1;
        m_spaceShipTAA.Add(i_spaceShipGO.transform);
        m_nlInitialPos.Add(i_initialPos);
        m_naPosOffsets.Dispose();
        m_naPosOffsets = new NativeArray<Vector3>(m_spaceShipTAA.length, Allocator.Persistent, NativeArrayOptions.ClearMemory);

        var rocketDock = i_spaceShipGO.transform.Find("RocketDock");
        m_spaceShipRocketDocksTAA.capacity = m_spaceShipTAA.length;
        m_spaceShipRocketDocksTAA.Add(rocketDock);
    }

    internal void RemoveSpaceShipData(int cnt)
    {
        m_spaceShipMoveJH.Complete();
        m_spaceShipOffsetChangeJH.Complete();

        for (int i = 0; i < cnt; ++i)
        {
            m_spaceShipTAA.RemoveAtSwapBack(m_spaceShipTAA.length - 1);
            m_nlInitialPos.RemoveAtSwapBack(m_nlInitialPos.Length - 1);
            m_spaceShipRocketDocksTAA.RemoveAtSwapBack(m_spaceShipRocketDocksTAA.length - 1);
        }

        m_naPosOffsets.Dispose();
        m_naPosOffsets = new NativeArray<Vector3>(m_spaceShipTAA.length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
    }

    public void AddRockets(int cnt)
    {
        m_rocketMoveJH.Complete();
        m_raycastJH.Complete();

        m_rocketsTAA.capacity = m_rocketsTAA.length + cnt;
        var rocketPool = PoolManager.instance.RocketPool;
        for (int i = rocketPool.m_cnt - cnt; i < rocketPool.m_cnt; ++i)
            m_rocketsTAA.Add(rocketPool.m_pool[i].transform);

        List<int> tempUsed = new List<int>();
        for (int i = 0; i < m_naUsedRockets.Length; ++i)
            tempUsed.Add(m_naUsedRockets[i]);

        for (int i = 0; i < cnt; ++i)
            tempUsed.Add(0);

        m_naUsedRockets.Dispose();
        m_naUsedRockets = new NativeArray<int>(tempUsed.ToArray(), Allocator.Persistent);

        List<float> tempRot = new List<float>();
        for (int i = 0; i < m_naRocketsRotationSteps.Length; ++i)
            tempRot.Add(m_naRocketsRotationSteps[i]);

        for (int i = 0; i < cnt; ++i)
            tempRot.Add(0);

        m_naRocketsRotationSteps.Dispose();
        m_naRocketsRotationSteps = new NativeArray<float>(tempRot.ToArray(), Allocator.Persistent);

        m_rocketComponentList.Clear();

        for (int i = 0; i < rocketPool.m_pool.Count; ++i)
            m_rocketComponentList.Add(rocketPool.m_pool[i].GetComponent<Rocket>());

        m_naRCommands.Dispose();
        m_naRCommands = new NativeArray<RaycastCommand>(m_rocketsTAA.length, Allocator.Persistent);

        m_naRHits.Dispose();
        m_naRHits = new NativeArray<RaycastHit>(m_rocketsTAA.length, Allocator.Persistent);
    }

    public void RemoveRockets(int cnt)
    {
        m_rocketMoveJH.Complete();
        m_raycastJH.Complete();

        for (int i = 0; i < cnt; ++i)
        {
            m_rocketsTAA.RemoveAtSwapBack(m_rocketsTAA.length - 1);
            m_rocketComponentList.RemoveAt(m_rocketComponentList.Count - 1);
        }

        List<int> tempUsed = new List<int>();
        for (int i = 0; i < m_naUsedRockets.Length - cnt; ++i)
            tempUsed.Add(m_naUsedRockets[i]);

        m_naUsedRockets.Dispose();
        m_naUsedRockets = new NativeArray<int>(tempUsed.ToArray(), Allocator.Persistent);

        List<float> tempRot = new List<float>();
        for (int i = 0; i < m_naRocketsRotationSteps.Length - cnt; ++i)
            tempRot.Add(m_naRocketsRotationSteps[i]);

        m_naRocketsRotationSteps.Dispose();
        m_naRocketsRotationSteps = new NativeArray<float>(tempRot.ToArray(), Allocator.Persistent);

        m_naRCommands.Dispose();
        m_naRCommands = new NativeArray<RaycastCommand>(m_rocketsTAA.length, Allocator.Persistent);

        m_naRHits.Dispose();
        m_naRHits = new NativeArray<RaycastHit>(m_rocketsTAA.length, Allocator.Persistent);
    }
}
