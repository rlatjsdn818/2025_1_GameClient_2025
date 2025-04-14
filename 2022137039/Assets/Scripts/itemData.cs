using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class itemData
{
    public int id;
    public string itemName;
    public string description;
    public string nameEng;
    public string itemTypeString;
    [NonSerialized]
    public itemTypes itemType;
    public int price;
    public int power;
    public int level;
    public bool isStackable;
    public string iconPath;

    //���ڿ��� ���������� ��ȯ�ϴ� �޼���
    public void InitalizeEnums()
    {
        if(Enum.TryParse(itemTypeString, out itemType parsedType))
        {
            itemType = parsedType;
        }
        else
        {
            Debug.LogError($"������ '{itemName}'�� ��ȿ���� ���� ������ Ÿ�� : {itemTypeString}");
            //�⺻�� ����
            itemType = itemType.Consumable;
        }
    }
}
