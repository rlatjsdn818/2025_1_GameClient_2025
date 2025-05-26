using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;

public class itemDataLoader : MonoBehaviour
{
    [SerializeField]
    private string jsonFileName = "items";  //Resources 폴더에서 가져올 json 파일 이름

    private List<itemData> itemList;

    // Start is called before the first frame update
    void Start()
    {
        LoadItemData();
    }

    void LoadItemData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(jsonFileName); //TextAsset 형태로 Json 파일을 로딩한다.

        if(jsonFile != null)   //파일 읽을때는 Null 값처리를 한다.
        {
            
            //원본 텍스트에서 UTF8로 변환처리
            byte[] bytes = Encoding.Default.GetBytes(jsonFile.text);
            string correntText = Encoding.UTF8.GetString(bytes);

            //변환된 텍스트 사용
            itemList = JsonConvert.DeSerializeObject<List<itemData>>(correntText);

            Debug.Log($"로드된 아이템 수 : {itemList.Count}");

            foreach(var item in itemList)
            {
                Debug.Log($"아이템: {EncodeKorean{item.itemName}}, 설명: {EncodeKorean(item.description)}");
            }
        }
        else
        {
            Debug.LogError($"JSON 파일을 찾을 수 없습니다. : {jsonFileName}");
        }
    }

    //한글 이코딩을 위한 헬퍼 함수
    private string EncodeKorean(string text)
    {
        if (string.isNullOrEmpty(text)) return "";           //텍스트가 NULL 값이면 함수를 끝낸다
        byte[] bytes = Encoding.Default.GetBytes(text);      //string을 Byte 배열로 변환 후
        return Encoding.UTF8.GetString(bytes);               //인코딩을 UTF8로 바꾼다.
    }
}
