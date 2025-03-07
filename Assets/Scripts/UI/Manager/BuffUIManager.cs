using UnityEngine;
using UnityEngine.UI;

public class BuffUIManager : MonoBehaviour
{
    public GameObject buffUIPrefab; // 버프 UI 프리팹
    public Transform buffUIParent;  // UI 표시할 위치 (Canvas 안에 배치)

    public void ShowBuff(Sprite icon, float duration)
    {
        GameObject buffUI = Instantiate(buffUIPrefab, buffUIParent);
        buffUI.GetComponent<BuffUI>().SetBuff(icon, duration);
    }
}
