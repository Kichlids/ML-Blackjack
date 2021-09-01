using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State { DEAL_HANDS, AGENT_PLAYS, PLAYER_PLAYS, DEALER_PLAYS }

public class Blackjack : MonoBehaviour {

    public static Blackjack bj;

    public State state;

    private int maxIndex;

    private QLearning ql;
    private BlackjackLib bjLib;
    private StateSpace ss;
    private Dealer dealer;
    private VisualManager visual;


    public string playerEndGameState;
    public string agentEndGameState;
    public bool gameOver;

    // Goes in this order:
    // Deal hands -> agent plays -> player plays -> dealer plays -> Deal hands

    private void Awake() {
        if (bj != null && bj != this) {
            Destroy(this.gameObject);
        }
        else {
            bj = this;
        }
    }

    private void Start() {
        bjLib = BlackjackLib.bjLib;
        ql = QLearning.ql;
        ss = StateSpace.ss;
        dealer = Dealer.dealer;
        visual = VisualManager.visual;
    }

    public void PlayRound() {

        gameOver = false;

        print("Start a new round");

        visual.ClearCardsOnPlay();

        bjLib.isRealGame = true;
        bjLib.Reset(true);

        state = State.DEAL_HANDS;

        StartCoroutine(DealHands());
    }

    private IEnumerator DealHands() {

        print("Dealing hands");

        // Shuffle cards when needed
        bool needsReset = bjLib.GetCurrentDeckSize() < ql.shuffleDeck;
        bjLib.Reset(needsReset);

        // Deal hands to player, agent, and dealer

        Card cardDrawn;

        // Deal two cards to player
        for (int i = 0; i < 2; i++) {

            // Deal hand to the agent
            cardDrawn = dealer.DrawCard();

            if (cardDrawn.cardValue >= ql.lowLowerEnd && cardDrawn.cardValue <= ql.lowHigherEnd) {
                bjLib.low--;
            }
            else if (cardDrawn.cardValue >= ql.highLowerEnd) {
                bjLib.high--;
            }
            bjLib.playerHand.Add(cardDrawn);

            visual.PlayerPlayCard(cardDrawn);
        }
        bjLib.playerScore = dealer.ComputeScore(bjLib.playerHand);

        // Deal two cards to agent
        for (int i = 0; i < 2; i++) {

            // Deal hand to the agent
            cardDrawn = dealer.DrawCard();

            if (cardDrawn.cardValue >= ql.lowLowerEnd && cardDrawn.cardValue <= ql.lowHigherEnd) {
                bjLib.low--;
            }
            else if (cardDrawn.cardValue >= ql.highLowerEnd) {
                bjLib.high--;
                if (cardDrawn.isAce) {
                    bjLib.usableAce = 1;
                }
            }
            bjLib.agentHand.Add(cardDrawn);

            visual.AgentPlayCard(cardDrawn);
        }
        bjLib.agentScore = dealer.ComputeScore(bjLib.agentHand);

        // Deal a card to dealer
        cardDrawn = dealer.DrawCard();
        if (cardDrawn.cardValue >= ql.lowLowerEnd && cardDrawn.cardValue <= ql.lowHigherEnd) {
            bjLib.low--;
        }
        else if (cardDrawn.cardValue >= ql.highLowerEnd) {
            bjLib.high--;
        }
        bjLib.dealerHand.Add(cardDrawn);

        visual.DealerPlayCard(cardDrawn);

        bjLib.dealerScore = dealer.ComputeScore(bjLib.dealerHand);


        print("Agent score: " + bjLib.agentScore);
        print("Player score: " + bjLib.playerScore);
        print("Dealer score: " + bjLib.dealerScore);

        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(AgentPlays(ql.stateSpaceSize));
    }
    
    private IEnumerator AgentPlays(string stateSize) {
        print("Agent's turn to play");

        state = State.AGENT_PLAYS;

        bool endTurn = false;
        while (!endTurn) {

            // Get the current state
            int currentState = ss.IdentifySpace(stateSize, bjLib.agentScore, bjLib.low, bjLib.high, bjLib.usableAce, bjLib.dealerHand);

            List<float> choiceValues = new List<float>();
            choiceValues.Add(ql.stateSpace[currentState][ql.choices[0]]);
            choiceValues.Add(ql.stateSpace[currentState][ql.choices[1]]);

            if (bjLib.canAgentDouble) {
                choiceValues.Add(ql.stateSpace[currentState][ql.choices[2]]);
            }

            print(ql.stateSpace[currentState][ql.choices[0]]);
            print(ql.stateSpace[currentState][ql.choices[1]]);
            print(ql.stateSpace[currentState][ql.choices[2]]);

            // Find the best choice
            float maxVal = -Mathf.Infinity;//choiceValues[0];
            maxIndex = 0;
            for (int i = 0; i < choiceValues.Count; i++) {
                if (choiceValues[i] > maxVal) {
                    maxVal = choiceValues[i];
                    maxIndex = i;
                }
            }
            print("Agent choice: " + maxIndex);

            // stay
            if (maxIndex == 1 || bjLib.agentScore == 21) {
                print("Agent stays");
                endTurn = true;
            }
            // hit
            else if (maxIndex == 0) {
                print("Agent hits");

                bjLib.canAgentDouble = false;
                Card drawnCard = dealer.DrawCard();

                print("Drew: " + drawnCard.cardName);

                if (drawnCard.cardValue >= ql.lowLowerEnd && drawnCard.cardValue <= ql.lowHigherEnd) {
                    bjLib.low--;
                }
                else if (drawnCard.cardValue >= ql.highLowerEnd) {
                    bjLib.high--;
                }

                bjLib.agentHand.Add(drawnCard);

                visual.AgentPlayCard(drawnCard);


                int oldScore = bjLib.agentScore;
                bjLib.agentScore = dealer.ComputeScore(bjLib.agentHand);

                print("Agent score: " + bjLib.agentScore);

                if (drawnCard.isAce && oldScore <= 10) {
                    bjLib.usableAce = 1;
                }
                if (oldScore > bjLib.agentScore) {
                    bjLib.usableAce = 0;
                }

                if (bjLib.agentScore > 21) {
                    bjLib.agentNotBust = false;

                    endTurn = true;
                }
            }
            // double
            else {
                print("Agent doubles");
                bjLib.agentCurrentBet *= 2;
                bjLib.canAgentDouble = false;
            }

            yield return new WaitForSeconds(0.5f);
        }

        state = State.PLAYER_PLAYS;
    }

    public void OnPlayerHitButton() {

        if (state == State.PLAYER_PLAYS) {

            print("Player hits");

            bjLib.canPlayerDouble = false;

            Card drawnCard = dealer.DrawCard();

            print("Drew: " + drawnCard.cardName);

            if (drawnCard.cardValue >= ql.lowLowerEnd && drawnCard.cardValue <= ql.lowHigherEnd) {
                bjLib.low--;
            }
            else if (drawnCard.cardValue >= ql.highLowerEnd) {
                bjLib.high--;
            }

            bjLib.playerHand.Add(drawnCard);

            visual.PlayerPlayCard(drawnCard);

            bjLib.playerScore = dealer.ComputeScore(bjLib.playerHand);

            print("Player score: " + bjLib.playerScore);

            if (bjLib.playerScore > 21) {
                bjLib.playerNotBust = false;

                state = State.DEALER_PLAYS;

                StartCoroutine(DealerPlays());
            }
        }
    }

    public void OnPlayerStayButton() {
        if (state == State.PLAYER_PLAYS) {
            print("Player stays");
            state = State.DEALER_PLAYS;

            StartCoroutine(DealerPlays());
        }
    }

    public void OnPlayerDoubleButton() {
        if (state == State.PLAYER_PLAYS && bjLib.canPlayerDouble) {
            print("Player doubles");
            bjLib.playerCurrentBet *= 2;
            bjLib.canPlayerDouble = false;
        }
    }

    private IEnumerator DealerPlays() {

        print("Dealer plays");

        // Deal cards if either player or agent has not bust
        if (bjLib.playerNotBust || bjLib.agentNotBust) {

            while (bjLib.dealerScore < bjLib.dealerStop     // Deawl until dealer accumulated at least the min dealing score
                    && (bjLib.dealerScore < bjLib.agentScore || !bjLib.agentNotBust) // Deal until higher than agent unless agent bust
                    && (bjLib.dealerScore < bjLib.playerScore || !bjLib.playerNotBust)) { // Deal until higher than player unless player bust

                Card cardDrawn = dealer.DrawCard();

                print("Drew: " + cardDrawn.cardName);

                if (cardDrawn.cardValue >= ql.lowLowerEnd && cardDrawn.cardValue <= ql.lowHigherEnd) {
                    bjLib.low--;
                }
                else if (cardDrawn.cardValue >= ql.highLowerEnd) {
                    bjLib.high--;
                }
                bjLib.dealerHand.Add(cardDrawn);
                visual.DealerPlayCard(cardDrawn);
                bjLib.dealerScore = dealer.ComputeScore(bjLib.dealerHand);

                print("Dealer score: " + bjLib.dealerScore);

                yield return new WaitForSeconds(0.5f);
            }
        } 

        // Player bust
        if (!bjLib.playerNotBust) {
            bjLib.playerMoney -= bjLib.playerCurrentBet;

            print("Player lost");
            playerEndGameState = "Player lost";
        }
        // Dealer busts
        else if (bjLib.dealerScore > 21) {
            bjLib.playerMoney += bjLib.playerCurrentBet * bjLib.bettingOdds;

            print("Player won");
            playerEndGameState = "Player won";
        }
        // Dealer beats player
        else if (bjLib.dealerScore > bjLib.playerScore) {
            bjLib.playerMoney -= bjLib.playerCurrentBet;

            print("Player lost");
            playerEndGameState = "Player lost";
        }
        else if (bjLib.dealerScore == bjLib.playerScore) {
            print("Player tied");
            playerEndGameState = "Player tied with dealer";
        }
        // player beats dealer
        else {
            bjLib.playerMoney += bjLib.playerCurrentBet * bjLib.bettingOdds;

            print("Player won");
            playerEndGameState = "Player won";
        }


        // Agent bust
        if (!bjLib.agentNotBust) {
            bjLib.agentMoney -= bjLib.agentCurrentBet;

            print("Agent lost");
            agentEndGameState = "Agent lost";
        }
        // Dealer busts
        else if (bjLib.dealerScore > 21) {
            bjLib.agentMoney += bjLib.agentCurrentBet * bjLib.bettingOdds;

            print("Agent won");
            agentEndGameState = "Agent won";
        }
        // Dealer beats agent
        else if (bjLib.dealerScore > bjLib.agentScore) {
            bjLib.agentMoney -= bjLib.agentCurrentBet;

            print("Agent lost");
            agentEndGameState = "Agent lost";
        }
        else if (bjLib.dealerScore == bjLib.agentScore) {
            print("Agent tied");
            agentEndGameState = "Agent tied with dealer";
        }
        // player beats dealer
        else {
            bjLib.agentMoney += bjLib.agentCurrentBet * bjLib.bettingOdds;

            print("Agent won");
            agentEndGameState = "Agent won";
        }

        gameOver = true;
    }
}
