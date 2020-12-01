using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blackjack : MonoBehaviour
{
    public static Blackjack bj;
    
    public int agentScore;
    public int dealerScore;
    public List<Card> agentHand;
    public List<Card> dealerHand;
    public List<Card> deck;

    public int low = 20;
    public int high = 20;
    public int usableAce;

    private int lowLowerEnd;
    private int lowHigherEnd;
    private int highLowerEnd;

    public int dealerStop;
    public float bettingOdds;
    public float startingBet;
    public float agentCurrentBet;
    public bool agentWin;
    public bool ongoing;
    public bool canAgentDouble = true;


    private Dealer dealer;
    private QLearning ql;

    private void Awake() {
        if (bj != null && bj != this) {
            Destroy(this.gameObject);
        }
        else {
            bj = this;
        }

        dealer = Dealer.dealer;
        ql = QLearning.ql;
    }

    private void Start() {
        lowLowerEnd = ql.lowLowerEnd;
        lowHigherEnd = ql.lowHigherEnd;
        highLowerEnd = ql.highLowerEnd;
    }

    // Deal hands to agent and dealer
    public void DealHands() {

        Card cardDrawn;

        // Deal two cards to agent
        for (int i = 0; i < 2; i++) {

            // Deal hand to the agent
            cardDrawn = dealer.DrawCard();

            if (cardDrawn.cardValue >= lowLowerEnd && cardDrawn.cardValue <= lowHigherEnd) {
                low--;
            }
            else if (cardDrawn.cardValue >= highLowerEnd) {
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
        if (cardDrawn.cardValue >= lowLowerEnd && cardDrawn.cardValue <= lowHigherEnd){
            low--;
        }
        else if (cardDrawn.cardValue >= highLowerEnd) {
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

            if (drawnCard.cardValue >= lowLowerEnd && drawnCard.cardValue <= lowHigherEnd) {
                low--;
            }
            else if (drawnCard.cardValue >= highLowerEnd) {
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
                ongoing = false;
                agentWin = false;
                return -agentCurrentBet;
            }
            else {
                return agentCurrentBet * 0.1f;
            }
        }
        // Stay and let dealer hit
        else if (decision == "stay" || agentScore == 21) {
            Card cardDrawn = dealer.DrawCard();
            if (cardDrawn.cardValue >= lowLowerEnd && cardDrawn.cardValue <= lowHigherEnd) {
                low--;
            }
            else if (cardDrawn.cardValue >= highLowerEnd) {
                high--;
            }

            for (int i = 0; i < 21; i++) {
                
                if (dealerScore < dealerStop && dealerScore < agentScore) {

                    Card cardDraw = dealer.DrawCard();

                    if (cardDraw.cardValue >= lowLowerEnd && cardDraw.cardValue <= lowHigherEnd) {
                        low--;
                    }
                    else if (cardDrawn.cardValue >= highLowerEnd) {
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
                ongoing = false;
                agentWin = true;
                ql.wins++;
                return agentCurrentBet * bettingOdds;
            }
            // Dealer and agent tie
            else if (dealerScore == agentScore) {
                ongoing = false;
                agentWin = false;
                return 0;
            }
            // Dealer beats agent
            else if (dealerScore > agentScore) {

                //for (int i = 0; i < agentHand.Count; i++) {
                //    print(agentHand[i].cardName);
                //}

                ongoing = false;
                agentWin = false;
                return -agentCurrentBet;
            }
            // Agent beats dealer
            else {
                ongoing = false;
                agentWin = true;
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
        agentScore = 0;
        dealerScore = 0;
        agentHand.Clear();
        dealerHand.Clear();
        agentCurrentBet = startingBet;
        agentWin = false;
        ongoing = true;
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
