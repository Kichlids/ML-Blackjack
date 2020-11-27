using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dealer : MonoBehaviour
{
    public static Dealer dealer;

    public int error;

    public CardData[] allCards;

    public List<Card> deck;

    private void Awake() {
        if (dealer != null && dealer != this) {
            Destroy(this.gameObject);
        }
        else {
            dealer = this;
        }

        ConstructDeck();
    }

    private void Start() {

        error = 0;
    }

    public Card DrawCard() {

        int dealt = Random.Range(0, deck.Count);
        Card dealtCard = deck[dealt];
        deck.RemoveAt(dealt);

        return dealtCard;
    }

    public int ComputeScore(List<Card> hand) {
        List<Card> sortedHand = hand;
        sortedHand.Sort((s1, s2) => s1.cardValue.CompareTo(s2.cardValue));

        // Count the number of aces
        int aceCount = 0;
        for (int i = 0; i < hand.Count; i++) {
            if (hand[i].isAce) {
                aceCount++;
            }
        }
        // Get score with Ace value of 11
        int sum = 0;
        for (int i = 0; i < hand.Count; i++) {
            sum += hand[i].cardValue;
        }
        // Change Ace value to 1 if over 21
        for (int i = 0; i < aceCount; i++) {
            if (sum > 21) {
                sum -= 10;
            }
        }

        // debug
        if (sum <= 0) {
            error++;
            print("Printing cards...");
            for (int i = 0; i < hand.Count; i++) {
                print(hand[i].cardName);
            }
        }

        return sum;
    }

    public void ConstructDeck() {
        deck.Clear();

        foreach (CardData data in allCards) {
            Card card;
            card.cardName = data.cardName;
            card.cardValue = data.value;
            card.isAce = data.isAce;
            card.cardPrefab = data.cardPrefab;

            deck.Add(card);
        }
    }
}

[System.Serializable]
public struct Card {
    public string cardName;
    public int cardValue;
    public bool isAce;
    public GameObject cardPrefab;

    public int CompareTo(Card card) {
        return cardValue.CompareTo(card.cardValue);
    }
}
