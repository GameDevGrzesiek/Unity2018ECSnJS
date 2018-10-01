using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class PoolManager : Singleton<PoolManager>
    {
        [Serializable]
        public class ObjectPool
        {
            public bool IsExpandable = false;
            public CustomBehaviour m_template;
            public int m_cnt = 0;
            private List<CustomBehaviour> m_pool = new List<CustomBehaviour>();
            private List<bool> m_used = new List<bool>();

            public void Init()
            {
                AddInstances(m_cnt);
            }

            public void Expand(int i_cnt)
            {
                m_cnt += i_cnt;

                if (i_cnt > 0)
                    AddInstances(i_cnt);
                else
                    RemoveInstances(i_cnt);
            }

            private void AddInstances(int i_cnt)
            {
                for (int i = 0; i < i_cnt; ++i)
                {
                    var pooledObj = Instantiate(m_template, PoolManager.instance.gameObject.transform);
                    pooledObj.gameObject.SetActive(false);
                    m_pool.Add(pooledObj);
                    m_used.Add(false);
                }

                UIManager.instance.RefreshPoolCount();
            }

            private void RemoveInstances(int i_cnt)
            {
                int itemCnt = Math.Abs(i_cnt);

                for (int i = 0; i < itemCnt; ++i)
                {
                    m_pool[m_pool.Count - 1].StopAllCoroutines();
                    m_pool[m_pool.Count - 1].gameObject.SetActive(false);
                    GameObject.Destroy(m_pool[m_pool.Count - 1].gameObject);
                    m_pool.RemoveAt(m_pool.Count - 1);
                    m_used.RemoveAt(m_pool.Count - 1);
                }

                UIManager.instance.RefreshPoolCount();
            }

            public CustomBehaviour SpawnObject(Vector3 i_position, Quaternion i_rotation)
            {
                int index = m_used.IndexOf(false);
                if (index >= 0 && index < m_pool.Count)
                {
                    m_used[index] = true;
                    var objToReturn = m_pool[index];
                    objToReturn.Restart();
                    objToReturn.transform.position = i_position;
                    objToReturn.transform.rotation = i_rotation;
                    objToReturn.gameObject.SetActive(true);
                    return objToReturn;
                }
                else
                {
                    if (IsExpandable)
                    {
                        Expand(1);
                        return SpawnObject(i_position, i_rotation);
                    }
                    else
                    {
                        //Debug.LogWarning("Wrong index in object pool!");
                    }
                }

                return null;
            }

            public void ReturnToPool(CustomBehaviour i_obj)
            {
                i_obj.StopAllCoroutines();
                i_obj.gameObject.SetActive(false);
                int index = m_pool.IndexOf(i_obj);
                if (index >= 0 && index < m_used.Count)
                {
                    m_used[index] = false;
                }
                else
                {
                    Debug.LogWarning("Returning to the wrong pool!");
                }
            }
        }

        public ObjectPool RocketPool;
        public ObjectPool SpaceShipPool;
        public ObjectPool RocketExplosionPool;

        public void Start()
        {
            RocketPool.Init();
            SpaceShipPool.Init();
            RocketExplosionPool.Init();
        }
    }
}
