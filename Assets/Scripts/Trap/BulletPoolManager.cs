using UnityEngine;
using System.Collections.Generic;

public class BulletPoolManager : MonoBehaviour
{
    [Header("Ǯ�� ����")]
    public GameObject bulletPrefab;   // �Ҹ� ������
    public int poolSize = 10;         // Ǯ ũ��

    private Queue<GameObject> bulletPool = new Queue<GameObject>();

    private void Start()
    {
        // Ǯ �ʱ�ȭ
        InitializePool();
    }

    private void InitializePool()
    {
        // Ǯ�� �Ѿ��� �̸� �����ؼ� �־�Ӵϴ�.
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);  // ó���� ��Ȱ��ȭ
            bulletPool.Enqueue(bullet);
        }
    }

    /// <summary>
    /// �Ѿ��� Ǯ���� ������ Ȱ��ȭ�ϰ� �ʱ� ��ġ ����
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
            // ���� Ǯ�� ������ ���ٸ�, ���� �����ؼ� ��ȯ
            GameObject bullet = Instantiate(bulletPrefab, position, rotation);
            return bullet;
        }
    }

    /// <summary>
    /// ����� ���� �Ѿ��� Ǯ�� ��ȯ�Ͽ� ��Ȱ��ȭ
    /// </summary>
    public void ReturnBulletToPool(GameObject bullet)
    {
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
    }
}
