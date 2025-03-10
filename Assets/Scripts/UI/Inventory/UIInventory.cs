using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class UIInventory : MonoBehaviour
{
    // 인벤토리 슬롯 배열
    public ItemSlot[] slots;

    // 인벤토리 창과 관련된 UI 요소들
    public GameObject inventoryWindow;
    public Transform slotPanel;
    public Transform dropPosition;

    [Header("Selected Item")]
    // 선택된 아이템과 관련된 변수들
    private ItemData selectedItem;
    private int selectedItemIndex;
    public TextMeshProUGUI selectedItemName;
    public TextMeshProUGUI selectedItemDescription;
    public TextMeshProUGUI selectedItemStatName;
    public TextMeshProUGUI selectedItemStatValue;
    public GameObject useButton;
    public GameObject equipButton;
    public GameObject unEquipButton;
    public GameObject dropButton;

    // 현재 장착된 아이템 인덱스
    private int curEquipIndex;

    // 플레이어 컨트롤러와 상태
    private PlayerController controller;
    private PlayerCondition condition;
    public BuffUIManager buffUIManager; // 버프 UI 매니저 연결

    void Start()
    {
        // 플레이어 컨트롤러와 상태 초기화
        controller = CharacterManager.Instance.Player.controller;
        condition = CharacterManager.Instance.Player.condition;
        dropPosition = CharacterManager.Instance.Player.dropPosition;

        // 인벤토리 토글과 아이템 추가 이벤트 등록
        controller.inventory += Toggle;
        CharacterManager.Instance.Player.addItem += AddItem;

        // 인벤토리 창 비활성화
        inventoryWindow.SetActive(false);

        // 슬롯 배열 초기화
        slots = new ItemSlot[slotPanel.childCount];
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = slotPanel.GetChild(i).GetComponent<ItemSlot>();
            slots[i].index = i;
            slots[i].inventory = this;
            slots[i].Clear();
        }

        // 선택된 아이템 창 초기화
        ClearSelectedItemWindow();
    }

    // 선택된 아이템 창 초기화
    void ClearSelectedItemWindow()
    {
        selectedItem = null;

        selectedItemName.text = string.Empty;
        selectedItemDescription.text = string.Empty;
        selectedItemStatName.text = string.Empty;
        selectedItemStatValue.text = string.Empty;

        useButton.SetActive(false);
        equipButton.SetActive(false);
        unEquipButton.SetActive(false);
        dropButton.SetActive(false);
    }

    // 인벤토리 창 토글
    public void Toggle()
    {
        if (IsOpen())
        {
            inventoryWindow.SetActive(false);
        }
        else
        {
            inventoryWindow.SetActive(true);
        }
    }

    // 인벤토리 창이 열려 있는지 확인
    public bool IsOpen()
    {
        return inventoryWindow.activeInHierarchy;
    }

    // 아이템 추가
    public void AddItem()
    {
        ItemData data = CharacterManager.Instance.Player.itemData;

        // 아이템이 스택 가능한 경우
        if (data.canStack)
        {
            ItemSlot slot = GetItemStack(data);
            if (slot != null)
            {
                slot.quantity++;
                UpdateUI();
                CharacterManager.Instance.Player.itemData = null;
                return;
            }
        }

        // 빈 슬롯에 아이템 추가
        ItemSlot emptySlot = GetEmptySlot();
        if (emptySlot != null)
        {
            emptySlot.item = data;
            emptySlot.quantity = 1;
            UpdateUI();
            CharacterManager.Instance.Player.itemData = null;
            return;
        }

        // 빈 슬롯이 없으면 아이템 버리기
        ThrowItem(data);
        CharacterManager.Instance.Player.itemData = null;
    }

    // UI 업데이트
    public void UpdateUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item != null)
            {
                slots[i].Set();
            }
            else
            {
                slots[i].Clear();
            }
        }
    }

    // 스택 가능한 아이템 슬롯 찾기
    ItemSlot GetItemStack(ItemData data)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == data && slots[i].quantity < data.maxStackAmount)
            {
                return slots[i];
            }
        }
        return null;
    }

    // 빈 슬롯 찾기
    ItemSlot GetEmptySlot()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == null)
            {
                return slots[i];
            }
        }
        return null;
    }

    // 아이템 버리기
    public void ThrowItem(ItemData data)
    {
        Instantiate(data.dropPrefab, dropPosition.position, Quaternion.Euler(Vector3.one * Random.value * 360));
    }

    // 아이템 선택
    public void SelectItem(int index)
    {
        if (slots[index].item == null) return;

        selectedItem = slots[index].item;
        selectedItemIndex = index;

        selectedItemName.text = selectedItem.displayName;
        selectedItemDescription.text = selectedItem.description;

        selectedItemStatName.text = string.Empty;
        selectedItemStatValue.text = string.Empty;

        for (int i = 0; i < selectedItem.consumables.Length; i++)
        {
            selectedItemStatName.text += selectedItem.consumables[i].type.ToString() + "\n";
            selectedItemStatValue.text += selectedItem.consumables[i].value.ToString() + "\n";
        }

        useButton.SetActive(selectedItem.type == ItemType.Consumable);
        equipButton.SetActive(selectedItem.type == ItemType.Equipable && !slots[index].equipped);
        unEquipButton.SetActive(selectedItem.type == ItemType.Equipable && slots[index].equipped);
        dropButton.SetActive(true);
    }

    // 사용 버튼 클릭 시
    public void OnUseButton()
    {
        if (selectedItem.type == ItemType.Consumable)
        {
            for (int i = 0; i < selectedItem.consumables.Length; i++)
            {
                switch (selectedItem.consumables[i].type)
                {
                    case ConsumableType.Health:
                        condition.Heal(selectedItem.consumables[i].value); break;
                    case ConsumableType.Hunger:
                        condition.Eat(selectedItem.consumables[i].value); break;
                    case ConsumableType.Speed:
                        controller.ApplyBoost(value => controller.moveSpeed = value, controller.moveSpeed, selectedItem.consumables[i].value, 5.0f, selectedItem.icon);
                        break;
                    case ConsumableType.Jump:
                        controller.ApplyBoost(value => controller.jumpPower = value, controller.jumpPower, selectedItem.consumables[i].value, 10.0f, selectedItem.icon);
                        break;
                }
            }
            RemoveSelctedItem();
        }
    }

    // 버리기 버튼 클릭 시
    public void OnDropButton()
    {
        ThrowItem(selectedItem);
        RemoveSelctedItem();
    }

    // 선택된 아이템 제거
    void RemoveSelctedItem()
    {
        slots[selectedItemIndex].quantity--;

        if (slots[selectedItemIndex].quantity <= 0)
        {
            if (slots[selectedItemIndex].equipped)
            {
                UnEquip(selectedItemIndex);
            }
            
            selectedItem = null;
            slots[selectedItemIndex].item = null;
            selectedItemIndex = -1;
            ClearSelectedItemWindow();
        }

        UpdateUI();
    }
    public void OnEquipButton()
    {
        if (slots[curEquipIndex].equipped)
        {
            UnEquip(curEquipIndex);
        }

        slots[selectedItemIndex].equipped = true;
        curEquipIndex = selectedItemIndex;
        CharacterManager.Instance.Player.equip.EquipNew(selectedItem);
        UpdateUI();

        SelectItem(selectedItemIndex);
    }
    void UnEquip(int index)
    {
        slots[index].equipped = false;
        CharacterManager.Instance.Player.equip.UnEquip();
        UpdateUI();

        if (selectedItemIndex == index)
        {
            SelectItem(selectedItemIndex);
        }
    }
    public void OnUnEquipButton()
    {
        UnEquip(selectedItemIndex);
    }
    // 특정 아이템과 수량을 가지고 있는지 확인
    public bool HasItem(ItemData item, int quantity)
    {
        return false;
    }

}
