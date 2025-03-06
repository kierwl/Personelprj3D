using UnityEngine;

[CreateAssetMenu(fileName = "NewInteractableObject", menuName = "Interactable Object")]
public class InteractableObjectData : ScriptableObject
{
    public string objectName;  // ������Ʈ �̸�
    [TextArea] public string description; // ����
    public Sprite icon; // UI���� ǥ���� ������
}
