using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class UIInventory : MonoBehaviour
{
    // �κ��丮 ���� �迭
    public ItemSlot[] slots;

    // �κ��丮 â�� ���õ� UI ��ҵ�
    public GameObject inventoryWindow;
    public Transform slotPanel;
    public Transform dropPosition;

    [Header("Selected Item")]
    // ���õ� �����۰� ���õ� ������
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

    // ���� ������ ������ �ε���
    private int curEquipIndex;

    // �÷��̾� ��Ʈ�ѷ��� ����
    private PlayerController controller;
    private PlayerCondition condition;
    public BuffUIManager buffUIManager; // ���� UI �Ŵ��� ����

    void Start()
    {
        // �÷��̾� ��Ʈ�ѷ��� ���� �ʱ�ȭ
        controller = CharacterManager.Instance.Player.controller;
        condition = CharacterManager.Instance.Player.condition;
        dropPosition = CharacterManager.Instance.Player.dropPosition;

        // �κ��丮 ��۰� ������ �߰� �̺�Ʈ ���
        controller.inventory += Toggle;
        CharacterManager.Instance.Player.addItem += AddItem;

        // �κ��丮 â ��Ȱ��ȭ
        inventoryWindow.SetActive(false);

        // ���� �迭 �ʱ�ȭ
        slots = new ItemSlot[slotPanel.childCount];
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = slotPanel.GetChild(i).GetComponent<ItemSlot>();
            slots[i].index = i;
            slots[i].inventory = this;
            slots[i].Clear();
        }

        // ���õ� ������ â �ʱ�ȭ
        ClearSelectedItemWindow();
    }

    // ���õ� ������ â �ʱ�ȭ
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

    // �κ��丮 â ���
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

    // �κ��丮 â�� ���� �ִ��� Ȯ��
    public bool IsOpen()
    {
        return inventoryWindow.activeInHierarchy;
    }

    // ������ �߰�
    public void AddItem()
    {
        ItemData data = CharacterManager.Instance.Player.itemData;

        // �������� ���� ������ ���
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

        // �� ���Կ� ������ �߰�
        ItemSlot emptySlot = GetEmptySlot();
        if (emptySlot != null)
        {
            emptySlot.item = data;
            emptySlot.quantity = 1;
            UpdateUI();
            CharacterManager.Instance.Player.itemData = null;
            return;
        }

        // �� ������ ������ ������ ������
        ThrowItem(data);
        CharacterManager.Instance.Player.itemData = null;
    }

    // UI ������Ʈ
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

    // ���� ������ ������ ���� ã��
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

    // �� ���� ã��
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

    // ������ ������
    public void ThrowItem(ItemData data)
    {
        Instantiate(data.dropPrefab, dropPosition.position, Quaternion.Euler(Vector3.one * Random.value * 360));
    }

    // ������ ����
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

    // ��� ��ư Ŭ�� ��
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

    // ������ ��ư Ŭ�� ��
    public void OnDropButton()
    {
        ThrowItem(selectedItem);
        RemoveSelctedItem();
    }

    // ���õ� ������ ����
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
    // Ư�� �����۰� ������ ������ �ִ��� Ȯ��
    public bool HasItem(ItemData item, int quantity)
    {
        return false;
    }

}
