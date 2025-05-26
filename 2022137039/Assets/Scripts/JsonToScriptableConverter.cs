#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Unity.VisualScripting;

public class JsonToScriptableConverter : EditorWindow
{

    private string jsonFilePath = "";  // JSON ���� ��� ���ڿ� ��
    private string outputFolder = "Assets/ScriptableObjects/items"; //��� SO������ ��� ��
    private bool createDatabase = true;  //������ ���̽��� ��� �� �������� ���� bool ��

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
            ConvertJsonToScriptableObjects();
        }
    }

    private void ConvertJsonToScriptableObjects()  //json������ scriptableobject ���Ϸ� ��ȯ �����ִ� �Լ�
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
                
                    If(itemSO.icon == null)
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

                EditorUtility.DisplayDialog("Success" $"Created {createdItems.Count} scriptable objects!", "OK");
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error",$"Failed to Convert JSON : {e.Message}", "OK");
            Debug.LogError($"JSON ��ȯ ����: {e}");
        }

    }
}

#endif