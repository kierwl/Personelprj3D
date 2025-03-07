using UnityEngine;
using UnityEngine.UI;

public class BuffUIManager : MonoBehaviour
{
    public GameObject buffUIPrefab; // ���� UI ������
    public Transform buffUIParent;  // UI ǥ���� ��ġ (Canvas �ȿ� ��ġ)

    public void ShowBuff(Sprite icon, float duration)
    {
        GameObject buffUI = Instantiate(buffUIPrefab, buffUIParent);
        buffUI.GetComponent<BuffUI>().SetBuff(icon, duration);
    }
}
