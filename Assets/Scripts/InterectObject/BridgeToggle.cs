using System.Collections;
using UnityEngine;

public class BridgeToggle : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private Collider bridgeCollider;

    public float visibleTime = 3f; // 보이는 시간
    public float invisibleTime = 2f; // 안 보이는 시간

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
            meshRenderer.enabled = true; // 다리 보이게
            bridgeCollider.enabled = true; // 충돌 가능
            yield return new WaitForSeconds(visibleTime);

            meshRenderer.enabled = false; // 다리 안 보이게
            bridgeCollider.enabled = false; // 충돌 제거
            yield return new WaitForSeconds(invisibleTime);
        }
    }
}
