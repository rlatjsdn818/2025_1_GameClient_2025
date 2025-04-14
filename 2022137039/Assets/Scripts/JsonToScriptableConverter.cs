#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine.Assertions;
using System;

public enum ConversionType
{
    Items,
    Dialogs
}

[Serializable]
public class DialogRowData
{
    public int? id;
    public string characterName;
    public string text;
    public int? nextId;
    public string portraitPath;
    public string choiceText;
    public int? choiceNextId;
}

public class JsonToScriptableConverter : EditorWindow
{

    private string jsonFilePath = "";  // JSON ���� ��� ���ڿ� ��
    private string outputFolder = "Assets/ScriptableObjects"; //��� SO������ ��� ��
    private bool createDatabase = true;  //������ ���̽��� ��� �� �������� ���� bool ��
    private ConversionType conversionType = ConversionType.Items;

    [MenuItem("Tools/JSON to Scriptable Objects")]

    public static void ShowWindow()
    {
        GetWindow<JsonToScriptableConverter>("JSON to Scriptable Objects");
    }

    void OnGUI()
    {
        GUILayout.Label("JSON to Scriptable Object Converter", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if(GUILayout.Button("Select JSON File"))
        {
            jsonFilePath = EditorUtility.OpenFilePanel("Select JSON File", "", "json");
        }

        EditorGUILayout.LabelField("Selected File : ", jsonFilePath);
        EditorGUILayout.Space();

        conversionType = (ConversionType)EditorGUILayout.EnumPopup("Conversion Type", conversionType);

        if (conversionType == ConversionType.Items && outputFolder == "Asset/ScriptableObjects")
        {
            outputFolder = "Assets/ScriptableObjects/Items";
        }
        else if (conversionType == ConversionType.Items && outputFolder == "Asset/ScriptableObjects")
        {
            outputFolder = "Assets/ScriptableObjects/Dialogs";
        }


        outputFolder = EditorGUILayout.TextField("output folder : ", outputFolder);
        createDatabase = EditorGUILayout.Toggle("Create Database Asset", createDatabase);
        EditorGUILayout.Space();

        if (GUILayout.Button("Convert to Scriptable Objects"))
        {
            if (string.IsNullOrEmpty(jsonFilePath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a JSON file first!", "OK");
                return;
            }

            switch (conversionType)
                {
                case ConversionType.Items:
                    ConvertJsonToItemScriptableObjects();
                    break;
                case ConversionType.Dialogs:
                    ConvertJsonToDialogScriptableObjects();
                    break;
                }
        }
    }

    private void ConvertJsonToItemScriptableObjects()  //json������ scriptableobject ���Ϸ� ��ȯ �����ִ� �Լ�
    {
        if(!Directory.Exists(outputFolder))  //���� ��ġ Ȯ�� �� ������ ����
        {
            Directory.CreateDirectory(outputFolder); 
        }

        //json ���� �б�
        string jsonText = File.ReadAllText(jsonFilePath);  //json ������ �д´�

        try
        {
            List<itemData> itemDataList = JsonConvert.DeserializeObject<List<itemData>>(jsonText);

            List<itemSO> createdItems = new List<ItemSO>();

            //�� ������ �����͸� ��ũ���ͺ� ������Ʈ�� ��ȯ
            foreach (var itemData in itemDataList)
            {
                ItemSO itemSO = ScriptableObject.CreateInstance<ItemSO>();  //ItemSO���� ����

                //������ ����
                itemSO.id = itemData.id;
                itemSO.itemName = itemData.itemName;
                itemSO.nameEng = itemData.nameEng;
                itemSO.description = itemData.description;

                //������ ��ȯ
                if(System.Enum.TryParse(itemData.itemTypeString, out itemTypes parsedType))
                {
                    itemSO.itemType = parsedType;
                }
                else
                {
                    Debug.LogWarning($"������ '{itemData.itemName}' �� ��ȿ���� ���� Ÿ�� : {itemData.itemTypeString}");
                }

                itemSO.price = itemData.price;
                itemSO.power = itemData.power;
                itemSO.level = itemData.level;
                itemSO.isStackable = itemData.isStackable;

                if (!string.IsNullOrEmpty(itemData.iconPath))
                {
                    itemSO.icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Asset/Resources/{itemData.iconPath}.png");

                    if(itemSO.icon == null)
                    {
                        Debug.LogWarning($"������ ' {itemData.nameEng}'�� �������� ã�� �� �����ϴ�. : {itemData.iconPath}");
                    }
                }

                string assetPath = $"{outputFolder}/item_{itemData.id.ToString("D4")}_{itemData.nameEng}.asset";
                AssetDatabase.CreateAsset(itemSO, assetPath);

                itemSO.name = $"item_{itemData.id.ToString("D4")}+{itemData.nameEng}";
                createdItems.Add( itemSO );
                EditorUtility.SetDirty( itemSO );
            }
            if (createDatabase && createdItems.Count > 0)
            {
                ItemDatabaseSO database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
                database.items = createdItems;

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("Success" , $"Created {createdItems.Count} scriptable objects!", "OK");
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error",$"Failed to Convert JSON : {e.Message}", "OK");
            Debug.LogError($"JSON ��ȯ ����: {e}");
        }

    }
    //��ȭ JSON�� ��ũ���ͺ� ������Ʈ�� ��ȯ

    private void ConvertJsonToDialogScriptableObjects()
    {
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        string jsonText = File.ReadAllText(jsonFilePath);

        try
        {

            //JSON �Ľ�
            List<DialogRowData> rowDataList = JsonConvert.DeserializeObject<List<DialogRowData>>(jsonText);

            //��ȭ ������ �籸��
            Dictionary<int, DialogSO> dialogMap = new Dictionary<int, DialogSO>();
            List<DialogSO> createDialogs = new List<DialogSO>();

            //1�ܰ� : ��ȭ �׸� ����
            foreach(var rowData in rowDataList)
            {
                //Id �ִ� ���� ��ȭ�� ó��
                if(rowData.id.HasValue)
                {
                    DialogSO dialogSO = ScriptableObject.CreateInstance<DialogSO>();

                    //������ ����
                    dialogSO.id = rowData.id.Value;
                    dialogSO.characterName = rowData.characterName;
                    dialogSO.text = rowData.text;
                    dialogSO.nextId = rowData.nextId.HasValue ? rowData.nextId.Value : -1;
                    dialogSO.portraitPath = rowData.portraitPath;
                    dialogSO.choices = new List<DialogChoiceSO>();

                    if(!string.IsNullOrEmpty(rowData.portraitPath))
                    {
                        dialogSO.portrait = Resources.Load<Sprite>(rowData.portraitPath);

                        if(dialogSO.portrait != null)
                            {
                            Debug.LogWarning($"��ȭ {rowData.id}�� �ʻ�ȭ�� ã�� �� �����ϴ�.");
                            }
                    }
                    dialogMap[dialogSO.id] = dialogSO;
                    createDialogs.Add(dialogSO);

                }
            }
            //2�ܰ� : ������ �׸� ó�� �� ����
            foreach(var rowData in rowDataList)
            {
                //id�� ���� choiceText�� �ִ� ���� 
                if(!rowData.id.HasValue && !string.IsNullOrEmpty(rowData.choiceText)&& rowData.choiceNextId.HasValue)
                {
                    //���� ���� ID�� �θ� ID�� ���
                    int parentId = -1;

                    int currentIndex = rowDataList.IndexOf(rowData);
                    for (int i = currentIndex - 1; i >- 0; i--)
                    {
                        parentId = rowDataList[i].id.Value;
                        break;
                    }
                }
                //�θ� id�� ã�� ���߰Ŵ� �θ� id�� -1�� ��� (ù��° �׸�)
                if (parentId == -1)
                {
                    Debug.LogWarning($"������ '{rowData.choiceText}'�� �θ� ��ȭ�� ã�� �� �����ϴ�.");
                }
                if (dialogMap.TryGetValue(parentId, out DialogSO parentDialog))
                {
                    DialogChoiceSO choiceSO = ScriptableObject.CreateInstance<DialogChoiceSO>();
                    choiceSO.text = rowData.choiceText;
                    choiceSO.nextId = rowData.choiceNextId.Value;

                    string choiceAssetPath = $"{outputFolder}/Choice {parentId} {parentDialog.choices.Count + 1}.asset";
                    AssetDatabase.CreateAsset(choiceSO, choiceAssetPath );
                    EditorUtility.SetDirty(choiceSO);

                    parentDialog.choices.Add(choiceSO);
                }
                else
                {
                    Debug.LogWarning($"������ '{rowData.choiceText}'�� ������ ��ȭ (ID : {parentId})�� ã�� �� �����ϴ�.");
                }
            }

            //3�ܰ� : ��ȭ ��ũ���ͺ� ������Ʈ ����
            foreach (var dialog in createDialogs)
            {
                string assetPath = $"{outputFolder}/Dialog {dialog.id.ToString("D4")}.asset";
                AssetDatabase.CreateAsset(dialog, assetPath );

                dialog.name = $"Dialog_{dialog.id.ToString("D4")}";

                EditorUtility.SetDirty(dialog);
            }

            if (createDatabase && createDialogs.Count > 0)
            {
                DialogDatabaseSO database = ScriptableObject.CreateInstance<DialogDatabaseSO>();
                database.dialogs = createDialogs;

                AssetDatabase.CreateAsset(database, $"{outputFolder}/DialogDatabase.asset");
                EditorUtility.SetDirty(database);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", $"Create {createDialogs.Count} dialogs scriptable objects!", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to convert JSON: {e.Message}", "OK");
            Debug.LogError($"JSON ��ȯ ���� : {e}");
        }
    }
}

#endif