using UnityEngine;

[CreateAssetMenu(fileName = "NewInteractableObject", menuName = "Interactable Object")]
public class InteractableObjectData : ScriptableObject
{
    public string objectName;  // 오브젝트 이름
    [TextArea] public string description; // 설명
    public Sprite icon; // UI에서 표시할 아이콘
}
