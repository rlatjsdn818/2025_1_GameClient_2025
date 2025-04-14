using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Database")]

public class ItemDatabaseSO : ScriptableObject
{
    public List <ItemSO> items = new List<ItemSO>();  //ItemSO�� ����Ʈ�� ����

    //ĳ���� ���� ���� ã��
    private Dictionary<int, ItemSO> itemsById;  //ID�� ������ ã�� ���� ĳ��
    private Dictionary<string, ItemSO> itemsByName;  //�̰� �̸����� ������ ã��

    public void Initialize()   //�ʱ� ���� �Լ�
    {
        itemsById = new Dictionary<int, ItemSO>();    //���� ���� �߱� ������ Dictionary �Ҵ�
        itemsByName = new Dictionary<string, ItemSO>();

        foreach (var item in items)   //items ����Ʈ�� ���� �Ǿ� �ִ� ���� ������ Dictionary�� �Է�.
        {
            itemsById[item.id] = item;
            itemsByName[item.itemName] = item;
        }
    }

    //ID�� ������ ã��

    public ItemSO GetItemById(int id)
    {
        if(itemsById == null)  //itemsbyid�� ĳ���� �Ǿ����� �ʴٸ� �ʱ�ȭ �Ѵ�
        {
            Initialize();
        }

        if (itemsById.TryGetValue(id, out ItemSO item))  //id ���� ã�Ƽ� itemSO�� �����Ѵ�.
            return item;

        return null;  //������ NULL
    }
    //�̸����� ������ ã��
    public ItemSO GetItemByName(string name)
    {
        if (itemsByName == null)
        {
            Initialize();
        }

        if (itemsByName.TryGetValue(name, out ItemSO item))
            return item;

        return null;
    }

    public List<ItemSO> GetItemByType(itemTypes type)
    {
        return items.FindAll(item => item.itemName == name);
    }
}
