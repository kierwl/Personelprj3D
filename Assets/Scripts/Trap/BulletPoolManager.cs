using UnityEngine;
using System.Collections.Generic;

public class BulletPoolManager : MonoBehaviour
{
    [Header("풀링 설정")]
    public GameObject bulletPrefab;   // 불릿 프리팹
    public int poolSize = 10;         // 풀 크기

    private Queue<GameObject> bulletPool = new Queue<GameObject>();

    private void Start()
    {
        // 풀 초기화
        InitializePool();
    }

    private void InitializePool()
    {
        // 풀에 총알을 미리 생성해서 넣어둡니다.
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);  // 처음엔 비활성화
            bulletPool.Enqueue(bullet);
        }
    }

    /// <summary>
    /// 총알을 풀에서 꺼내서 활성화하고 초기 위치 설정
    /// </summary>
    public GameObject GetBullet(Vector3 position, Quaternion rotation)
    {
        if (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();
            bullet.SetActive(true);
            bullet.transform.position = position;
            bullet.transform.rotation = rotation;
            return bullet;
        }
        else
        {
            // 만약 풀에 여유가 없다면, 새로 생성해서 반환
            GameObject bullet = Instantiate(bulletPrefab, position, rotation);
            return bullet;
        }
    }

    /// <summary>
    /// 사용이 끝난 총알을 풀에 반환하여 비활성화
    /// </summary>
    public void ReturnBulletToPool(GameObject bullet)
    {
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
    }
}
