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

    private string jsonFilePath = "";  // JSON 파일 경로 문자열 값
    private string outputFolder = "Assets/ScriptableObjects/items"; //출력 SO파일을 경로 값
    private bool createDatabase = true;  //데이터 베이스를 사용 할 것인지에 대한 bool 값

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

    private void ConvertJsonToScriptableObjects()  //json파일을 scriptableobject 파일로 변환 시켜주는 함수
    {
        if(!Directory.Exists(outputFolder))  //폴더 위치 확인 후 없으면 생성
        {
            Directory.CreateDirectory(outputFolder); 
        }

        //json 파일 읽기
        string jsonText = File.ReadAllText(jsonFilePath);  //json 파일을 읽는다

        try
        {
            List<itemData> itemDataList = JsonConvert.DeserializeObject<List<itemData>>(jsonText);

            List<itemSO> createdItems = new List<ItemSO>();

            //각 아이템 데이터를 스크립터블 오브젝트로 변환
            foreach (var itemData in itemDataList)
            {
                ItemSO itemSO = ScriptableObject.CreateInstance<ItemSO>();  //ItemSO파일 생성

                //데이터 복사
                itemSO.id = itemData.id;
                itemSO.itemName = itemData.itemName;
                itemSO.nameEng = itemData.nameEng;
                itemSO.description = itemData.description;

                //열거형 변환
                if(System.Enum.TryParse(itemData.itemTypeString, out itemTypes parsedType))
                {
                    itemSO.itemType = parsedType;
                }
                else
                {
                    Debug.LogWarning($"아이템 '{itemData.itemName}' 의 유효하지 않은 타입 : {itemData.itemTypeString}");
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
                        Debug.LogWarning($"아이템 ' {itemData.nameEng}'의 아이콘을 찾을 수 없습니다. : {itemData.iconPath}");
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
            Debug.LogError($"JSON 변환 오류: {e}");
        }

    }
}

#endif