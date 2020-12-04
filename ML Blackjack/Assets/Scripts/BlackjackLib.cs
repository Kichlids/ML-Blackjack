using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackjackLib : MonoBehaviour
{
    public static BlackjackLib bjLib;

    [Header("Player variables")]
    public int playerScore;
    public List<Card> playerHand;
    public float playerMoney;
    public float playerCurrentBet;
    public bool canPlayerDouble = true;
    public bool playerNotBust = true;

    [Space]

    [Header("Agent variables")]
    public int agentScore;
    public List<Card> agentHand;
    public float agentMoney;
    public float agentCurrentBet;
    public bool canAgentDouble = true;
    public bool agentNotBust = true;

    [Header("Q Learning variables")]
    public int low = 20;
    public int high = 20;
    public int usableAce;

    [Space]

    [Header("Dealer variables")]
    public int dealerScore;
    public List<Card> dealerHand;
    public int dealerStop;

    [Space]

    [Header("General variables")]
    public float bettingOdds;
    public float startingBet;
    public bool isRealGame = true;


    private List<Card> deck;
    private Dealer dealer;
    private QLearning ql;

    private void Awake() {
        if (bjLib != null && bjLib != this) {
            Destroy(this.gameObject);
        }
        else {
            bjLib = this;
        }
    }

    private void Start() {
        dealer = Dealer.dealer;
        ql = QLearning.ql;

        isRealGame = false;

        agentCurrentBet = startingBet;
        playerCurrentBet = startingBet;
    }

    // Deal hands to agent and dealer
    public void DealHands() {

        Card cardDrawn;

        // Deal two cards to player
        for (int i = 0; i < 2; i++) {

            // Deal hand to the agent
            cardDrawn = dealer.DrawCard();

            if (cardDrawn.cardValue >= ql.lowLowerEnd && cardDrawn.cardValue <= ql.lowHigherEnd) {
                low--;
            }
            else if (cardDrawn.cardValue >= ql.highLowerEnd) {
                high--;
            }
            playerHand.Add(cardDrawn);
        }
        playerScore = dealer.ComputeScore(playerHand);

        // Deal two cards to agent
        for (int i = 0; i < 2; i++) {

            // Deal hand to the agent
            cardDrawn = dealer.DrawCard();

            if (cardDrawn.cardValue >= ql.lowLowerEnd && cardDrawn.cardValue <= ql.lowHigherEnd) {
                low--;
            }
            else if (cardDrawn.cardValue >= ql.highLowerEnd) {
                high--;
                if (cardDrawn.isAce) {
                    usableAce = 1;
                }
            }
            agentHand.Add(cardDrawn);
        }
        agentScore = dealer.ComputeScore(agentHand);

        // Deal a card to dealer
        cardDrawn = dealer.DrawCard();
        if (cardDrawn.cardValue >= ql.lowLowerEnd && cardDrawn.cardValue <= ql.lowHigherEnd){
            low--;
        }
        else if (cardDrawn.cardValue >= ql.highLowerEnd) {
            high--;
        }
        dealerHand.Add(cardDrawn);

        dealerScore = dealer.ComputeScore(dealerHand);
    }

    public void DoubleBet() {

        if (canAgentDouble) {
            agentCurrentBet *= 2;
        }
    }

    // Return the reward resulting from the decision made
    public float DealCards(string decision) {
           
        // Draw a card
        if (decision == "hit") {
            canAgentDouble = false;
            Card drawnCard = dealer.DrawCard();

            if (drawnCard.cardValue >= ql.lowLowerEnd && drawnCard.cardValue <= ql.lowHigherEnd) {
                low--;
            }
            else if (drawnCard.cardValue >= ql.highLowerEnd) {
                high--;
            }

            agentHand.Add(drawnCard);


            int oldScore = agentScore;
            agentScore = dealer.ComputeScore(agentHand);

            if (drawnCard.isAce && oldScore <= 10) {
                usableAce = 1;
            }
            if (oldScore > agentScore) {
                usableAce = 0;
            }

            if (agentScore > 21) {
                agentNotBust = false;
                return -agentCurrentBet;
            }
            else {
                return agentCurrentBet * 0.1f;
            }
        }
        // Stay and let dealer hit
        else if (decision == "stay" || agentScore == 21) {
            Card cardDrawn = dealer.DrawCard();
            if (cardDrawn.cardValue >= ql.lowLowerEnd && cardDrawn.cardValue <= ql.lowHigherEnd) {
                low--;
            }
            else if (cardDrawn.cardValue >= ql.highLowerEnd) {
                high--;
            }
            dealerHand.Add(cardDrawn);
            dealerScore = dealer.ComputeScore(dealerHand);

            for (int i = 0; i < 21; i++) {
                
                if (dealerScore < dealerStop && dealerScore < agentScore) {

                    Card cardDraw = dealer.DrawCard();

                    if (cardDraw.cardValue >= ql.lowLowerEnd && cardDraw.cardValue <= ql.lowHigherEnd) {
                        low--;
                    }
                    else if (cardDrawn.cardValue >= ql.highLowerEnd) {
                        high--;
                    }
                    dealerHand.Add(cardDrawn);
                    dealerScore = dealer.ComputeScore(dealerHand);
                }
                else {
                    break;
                }
            }

            // Dealer busts
            if (dealerScore > 21) {
                agentNotBust = false;
                ql.wins++;
                return agentCurrentBet * bettingOdds;
            }
            // Dealer and agent tie
            else if (dealerScore == agentScore) {
                agentNotBust = false;
                return 0;
            }
            // Dealer beats agent
            else if (dealerScore > agentScore) {

                agentNotBust = false;
                return -agentCurrentBet;
            }
            // Agent beats dealer
            else {
                agentNotBust = false;
                ql.wins++;
                return agentCurrentBet * bettingOdds;
            }
        }
        // double
        else {
            DoubleBet();
            return 0;
        }
    }

    public void Reset(bool hardReset) {

        playerScore = 0;
        agentScore = 0;
        dealerScore = 0;
        playerHand.Clear();
        agentHand.Clear();
        dealerHand.Clear();
        playerCurrentBet = startingBet;
        agentCurrentBet = startingBet;
        agentNotBust = true;
        playerNotBust = true;
        usableAce = 0;
        canAgentDouble = true;

        if (hardReset) {
            dealer.ConstructDeck();
            low = 20;
            high = 20;
        }
    }

    public int GetCurrentDeckSize() {
        return dealer.deck.Count;
    }
}
