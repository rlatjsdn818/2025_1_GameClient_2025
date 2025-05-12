using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public List<CardData> deckCards = new List<CardData>();
    public List<CardData> handCards = new List<CardData>();
    public List<CardData> discardCards = new List<CardData>();

    public GameObject cardPrefab;
    public Transform deckPosition;
    public Transform handPosition;
    public Transform discardPosition;

    public List<GameObject> cardObjects = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        ShuffleDeck();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.D))
        {
            DrawCard();
        }
        if(Input.GetKeyDown(KeyCode.F))
        {
            ReturnDiscardsToDeck();
        }

        ArrangeHand();
    }
    public void ShuffleDeck()
    {
        List<CardData> tempDeck = new List<CardData>(deckCards);
        deckCards.Clear();

        while (tempDeck.Count > 0)
        {
            int randIndex = Random.Range(0, tempDeck.Count);
            deckCards.Add(tempDeck[randIndex]);
            tempDeck.RemoveAt(randIndex);
        }

        Debug.Log("덱을 섞었습니다. : " + deckCards.Count + "장");

    }
    public void DrawCard()
    {
        if(handCards.Count >= 0)
        {
            Debug.Log("손패가 가득 참! (최대 6장)");
            return;
        }

        if (deckCards.Count == 0)
        {
            Debug.Log("덱에 카드가 없음");
            return;
        }

        CardData cardData = deckCards[0];
        deckCards.RemoveAt(0);

        handCards.Add(cardData);

        GameObject cardObj = Instantiate(cardPrefab, deckPosition.position, Quaternion.identity);

        CardDisplay cardDisplay = cardObj.GetComponent<CardDisplay>();

        if (cardDisplay != null)
        {
            cardDisplay.SetupCard(cardData);
            cardDisplay.cardIndex = handCards.Count - 1;
            cardObjects.Add(cardObj);
        }

        //손패 위치 업데이트
        //ArrangeHand();

        Debug.Log("카드를 드로우 했습니다. : " + cardData.cardName + " (손패 : " + handCards.Count + "/6");

    }
    public void ArrangeHand()                        //손에 있는 카드 재정렬
    {
        if (handCards.Count == 0) return;

        //손패 배치를 위한 변수
        float cardWidth = 1.2f;
        float spacing = cardWidth * 1.8f;
        float totalWidth = (handCards.Count - 1) * spacing;
        float startX = -totalWidth / 2f;

        //각 카드 위치 설정
        for (int i = 0; i < cardObjects.Count; i++)
        {
            if (cardObjects[i] != null)
            {
                //드래그 중인 카드는 건너뛰기
                CardDisplay display = cardObjects[i].GetComponent<CardDisplay>();
                if (display != null && display.isDragging)
                    continue;

                //목표 위치 계산
                Vector3 targetPosition = handPosition.position + new Vector3(startX + (i * spacing), 0, 0);

                //부드러운 이동
                cardObjects[i].transform.position = Vector3.Lerp(cardObjects[i].transform.position, targetPosition, Time.deltaTime * 10f);
            }
        }
    }
    public void DiscardCard(int handIndex)          //카드 버리기(디스카드)
    {
        if (handIndex < 0 || handIndex >= handCards.Count)
        {
            Debug.Log("유효하지 않은 카드 인덱스 입니다!");
            return;
        }

        CardData cardData = handCards[handIndex];   //손패에서 카드 가져오기
        handCards.RemoveAt(handIndex);

        discardCards.Add(cardData);                 //버린 카드 더미에 추가

        if (handIndex < cardObjects.Count)          //해당 카드 게임 오브젝트 제거
        {
            Destroy(cardObjects[handIndex]);
            cardObjects.RemoveAt(handIndex);
        }

        for (int i = 0; i < cardObjects.Count; i++) //카드 인덱스 재설정
        {
            CardDisplay display = cardObjects[i].GetComponent<CardDisplay>();
            if (display != null) display.cardIndex = i;
        }

        ArrangeHand();                              //손패 위치 업데이트
        Debug.Log("카드를 버렸습니다. " + cardData.cardName);
    }
    public void ReturnDiscardsToDeck()
    {
        if (discardCards.Count == 0)
        {
            Debug.Log("버린 카드 더미가 비어 있습니다.");
            return;
        }

        deckCards.AddRange(discardCards);   //버린 카드를 모두 덱에 추가
        discardCards.Clear();               //버린 카드 더미 비우기
        ShuffleDeck();                      //덱 섞기

        Debug.Log("버린 카드 " + deckCards.Count + "장을 덱으로 되돌리고 섞었습니다.");
    }
}
