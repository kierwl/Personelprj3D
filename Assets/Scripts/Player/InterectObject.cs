using UnityEngine;

public class InterectObject : MonoBehaviour, IInteractable
{
    public InteractableObjectData data; // ScriptableObject ������ ���
    private bool isPlayerInRange = false;

    public string GetInteractPrompt()
    {
        return $"{data.objectName}\n{data.description}";
    }

    public void OnInteract()
    {
        if (isPlayerInRange)
        {
            Debug.Log($"�÷��̾ {data.objectName}��(��) ��ȣ�ۿ���");
            Destroy(gameObject); // ��ȣ�ۿ� �� ���� (�ʿ��ϸ� ��Ȱ��ȭ�� ���� ����)
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
