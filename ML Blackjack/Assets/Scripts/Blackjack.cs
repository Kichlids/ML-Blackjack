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

    public int dealerStop;
    public float odds;
    public float bet;
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

    // Deal hands to agent and dealer
    public void DealHands() {

        Card cardDrawn;

        // Deal two cards to agent
        for (int i = 0; i < 2; i++) {

            // Deal hand to the agent
            cardDrawn = dealer.DrawCard();

            if (cardDrawn.cardValue >= 2 && cardDrawn.cardValue <= 6) {
                low--;
            }
            else if (cardDrawn.cardValue >= 10) {
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
        if (cardDrawn.cardValue >= 2 && cardDrawn.cardValue <= 6){
            low--;
        }
        else if (cardDrawn.cardValue >= 10) {
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

            if (drawnCard.cardValue >= 2 && drawnCard.cardValue <= 6) {
                low--;
            }
            else if (drawnCard.cardValue >= 10) {
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
            if (cardDrawn.cardValue >= 2 && cardDrawn.cardValue <= 6) {
                low--;
            }
            else if (cardDrawn.cardValue >= 10) {
                high--;
            }

            for (int i = 0; i < 21; i++) {
                
                if (dealerScore < dealerStop && dealerScore < agentScore) {

                    Card cardDraw = dealer.DrawCard();

                    if (cardDraw.cardValue >= 2 && cardDraw.cardValue <= 6) {
                        low--;
                    }
                    else if (cardDrawn.cardValue >= 10) {
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
                return agentCurrentBet * odds;
            }
            // Dealer and agent tie
            else if (dealerScore == agentScore) {
                ongoing = false;
                agentWin = false;
                return 0;
            }
            // Dealer beats agent
            else if (dealerScore > agentScore) {
                print("here1");
                print(agentScore);
                print(dealerScore);

                for (int i = 0; i < agentHand.Count; i++) {
                    print(agentHand[i].cardName);
                }

                ongoing = false;
                agentWin = false;
                return -agentCurrentBet;
            }
            // Agent beats dealer
            else {
                //print("here2");
                //print(agentScore);
                //print(dealerScore);
                ongoing = false;
                agentWin = true;
                ql.wins++;
                return agentCurrentBet * odds;
            }
        }
        // double
        else {
            DoubleBet();
            return 0;
        }
    }
    private IEnumerator DealerHit() {
        while (dealerScore < dealerStop && dealerScore < agentScore) {
            Card cardDrawn = dealer.DrawCard();

            if (cardDrawn.cardValue >= 2 && cardDrawn.cardValue <= 6) {
                low--;
            }
            else if (cardDrawn.cardValue >= 10) {
                high--;
            }

            dealerHand.Add(cardDrawn);
            dealerScore = dealer.ComputeScore(dealerHand);
        }

        yield return null;
    }

    public void Reset(bool hardReset) {
        agentScore = 0;
        dealerScore = 0;
        agentHand.Clear();
        dealerHand.Clear();
        agentCurrentBet = bet;
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
