using UnityEngine;

public class InterectObject : MonoBehaviour, IInteractable
{
    public InteractableObjectData data; // ScriptableObject 데이터 사용
    private bool isPlayerInRange = false;

    public string GetInteractPrompt()
    {
        return $"{data.objectName}\n{data.description}";
    }

    public void OnInteract()
    {
        if (isPlayerInRange)
        {
            Debug.Log($"플레이어가 {data.objectName}을(를) 상호작용함");
            Destroy(gameObject); // 상호작용 후 삭제 (필요하면 비활성화로 변경 가능)
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}
