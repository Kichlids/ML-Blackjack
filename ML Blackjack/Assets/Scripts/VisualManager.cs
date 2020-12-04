using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualManager : MonoBehaviour
{
    public static VisualManager visual;

    public GameObject cardReversePrefab;

    public Transform deckSpawn;

    [Header("Card Layout Center")]
    public Transform playerCardsLayoutCenter;
    public Transform agentCardsLayoutCenter;
    public Transform dealerCardsLayoutCenter;

    [Header("Parent GameObjects")]
    public Transform deckParent;
    public Transform playerCardsParent;
    public Transform agentCardsParent;
    public Transform dealerCardsParent;

    [Header("List of cards in game")]
    public List<GameObject> reverseCardsInDeck;
    public List<GameObject> playerCards;
    public List<GameObject> agentCards;
    public List<GameObject> dealerCards;

    public float deckGapBetweenCards;

    // Space between center of cards in layout
    public float gapBetweenCardsLayout;


    private void Awake() {
        if (visual != null && visual != this) {
            Destroy(this.gameObject);
        }
        else {
            visual = this;
        }
    }


    private void Start() {
        reverseCardsInDeck = new List<GameObject>();
        playerCards = new List<GameObject>();
        agentCards = new List<GameObject>();
        dealerCards = new List<GameObject>();

        BuildNewDeck();
    }

    public void BuildNewDeck() {

        for (int i = 0; i < 52; i++) {
            GameObject instCardReverse = Instantiate(cardReversePrefab, deckSpawn.position + new Vector3(0, reverseCardsInDeck.Count * deckGapBetweenCards, 0), Quaternion.identity, deckParent);
            instCardReverse.transform.eulerAngles = new Vector3(-90, 0, 0);
            instCardReverse.transform.localScale = new Vector3(2, 2, 2);

            reverseCardsInDeck.Add(instCardReverse);
        }
    }

    public void PlayerPlayCard(Card card) {
        float y = playerCardsLayoutCenter.position.y;
        float z = playerCardsLayoutCenter.position.z;

        float xPos;

        // Even number of cards after addition
        if ((playerCards.Count + 1) % 2 == 0) {

            xPos = -((playerCards.Count + 1) / 2 - 1 + 0.5f) * gapBetweenCardsLayout;
            print("even");
            print(xPos);
        }
        // Odd number of cards after addition
        else {
            xPos = -((playerCards.Count + 1) / 2) * gapBetweenCardsLayout;
            print("odd");
        }

        for (int i = 0; i < playerCards.Count; i++) {
            playerCards[i].transform.position = new Vector3(xPos, y, z);

            xPos += gapBetweenCardsLayout;
        }

        GameObject newPlayerCard = Instantiate(card.cardPrefab, new Vector3(xPos, y, z), Quaternion.identity, playerCardsParent);
        newPlayerCard.transform.eulerAngles = new Vector3(-90, 0, 0);
        newPlayerCard.transform.localScale = new Vector3(2, 2, 2);

        playerCards.Add(newPlayerCard);
    }

    public void AgentPlayCard(Card card) {
        float x = agentCardsLayoutCenter.position.x;
        float y = agentCardsLayoutCenter.position.y;

        float zPos;

        // Even number of cards after addition
        if ((agentCards.Count + 1) % 2 == 0) {
            zPos = -((agentCards.Count + 1) / 2 - 1 + 0.5f) * gapBetweenCardsLayout;
        }
        // Odd number of cards after addition
        else {
            zPos = -((agentCards.Count + 1) / 2) * gapBetweenCardsLayout;
        }

        for (int i = 0; i < agentCards.Count; i++) {
            agentCards[i].transform.position = new Vector3(x, y, zPos);

            zPos += gapBetweenCardsLayout;
        }

        GameObject newAgentCard = Instantiate(card.cardPrefab, new Vector3(x, y, zPos), Quaternion.identity, agentCardsParent);
        newAgentCard.transform.eulerAngles = new Vector3(-90, 0, -90);
        newAgentCard.transform.localScale = new Vector3(2, 2, 2);

        agentCards.Add(newAgentCard);
    }
    
    public void DealerPlayCard(Card card) {
        float y = dealerCardsLayoutCenter.position.y;
        float z = dealerCardsLayoutCenter.position.z;

        float xPos;

        // Even number of cards after addition
        if ((dealerCards.Count + 1) % 2 == 0) {
            xPos = -((dealerCards.Count + 1) / 2 - 1 + 0.5f) * gapBetweenCardsLayout;
        }
        // Odd number of cards after addition
        else {
            xPos = -((dealerCards.Count + 1) / 2) * gapBetweenCardsLayout;
        }

        for (int i = 0; i < dealerCards.Count; i++) {
            dealerCards[i].transform.position = new Vector3(xPos, y, z);

            xPos += gapBetweenCardsLayout;
        }

        GameObject newDealerCard = Instantiate(card.cardPrefab, new Vector3(xPos, y, z), Quaternion.identity, dealerCardsParent);
        newDealerCard.transform.eulerAngles = new Vector3(-90, 0, 0);
        newDealerCard.transform.localScale = new Vector3(2, 2, 2);

        dealerCards.Add(newDealerCard);
    }

    public void ClearCardsOnPlay() {

        foreach (GameObject cards in playerCards) {
            Destroy(cards.gameObject);
        }
        foreach (GameObject cards in agentCards) {
            Destroy(cards.gameObject);
        }
        foreach (GameObject cards in dealerCards) {
            Destroy(cards.gameObject);
        }

        playerCards.Clear();
        agentCards.Clear();
        dealerCards.Clear();
    }
}
