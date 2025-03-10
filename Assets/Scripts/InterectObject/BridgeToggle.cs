using System.Collections;
using UnityEngine;

public class BridgeToggle : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private Collider bridgeCollider;

    public float visibleTime = 3f; // ���̴� �ð�
    public float invisibleTime = 2f; // �� ���̴� �ð�

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        bridgeCollider = GetComponent<Collider>();
        StartCoroutine(ToggleBridge());
    }

    IEnumerator ToggleBridge()
    {
        while (true)
        {
            meshRenderer.enabled = true; // �ٸ� ���̰�
            bridgeCollider.enabled = true; // �浹 ����
            yield return new WaitForSeconds(visibleTime);

            meshRenderer.enabled = false; // �ٸ� �� ���̰�
            bridgeCollider.enabled = false; // �浹 ����
            yield return new WaitForSeconds(invisibleTime);
        }
    }
}
