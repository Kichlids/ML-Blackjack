using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QLearning : MonoBehaviour {

    public int wins = 0;

    public float learningRate = 0.05f;
    public float discountRate = 0.9f;
    public float epsilon = 0.1f;
    public float bettingOdds = 1f;
    public int shuffleDeck = 26;
    public string stateSpaceSize = "small";
    public List<Dictionary<string, float>> stateSpace;
    public int currentState;
    public int dealerStop = 16;
    public int episodes = 1;

    public string[] choices = { "hit", "stay", "double" };
    public string[] policy = { "greedy", "explore" };

    private Blackjack bj;
    private StateSpace ss;

    public static QLearning ql;

    private void Awake() {
        if (ql != null && ql != this) {
            Destroy(this.gameObject);
        }
        else {
            ql = this;
        }
    }

    private void Start() {
        bj = Blackjack.bj;
        bj.bet = 1;
        bj.odds = bettingOdds;
        bj.dealerStop = dealerStop;

        ss = StateSpace.ss;

        stateSpace = ss.GenerateStateSpace(stateSpaceSize);

        RunQLearning();
    }


    // Return "hit", "stay", "double"
    public string MakeChoice(int currentState) {
        if (bj.agentScore == 21) {
            return "stay";
        }

        string chosenPolicy;
        float rand = Random.Range(0f, 1f);
        if (rand <= 1-epsilon) {
            chosenPolicy = policy[0];
        }
        else {
            chosenPolicy = policy[1];
        }

        string decision;

        // greedy
        if (chosenPolicy == policy[0]) {
            decision = "hit";

            if (bj.canAgentDouble) {
                if (stateSpace[currentState]["hit"] < stateSpace[currentState]["stay"] && 
                    stateSpace[currentState]["double"] < stateSpace[currentState]["stay"]) {
                    decision = "stay";
                }
                else if (stateSpace[currentState]["hit"] < stateSpace[currentState]["double"]) {
                    decision = "double";
                }
            }
            else {
                if (stateSpace[currentState]["hit"] < stateSpace[currentState]["stay"]) {
                    decision = "stay";
                }
            }
        }
        else {
            if (bj.canAgentDouble) {
                int rand2 = Random.Range(0, 3);
                decision = choices[rand2];
            }
            else {
                int rand2 = Random.Range(0, 2);
                decision = choices[rand2];
            }
        }

        return decision;
    }

    public void RunEpisode() {
        bool reset = bj.GetCurrentDeckSize() < shuffleDeck;
        bj.Reset(reset);

        bj.DealHands();

        //StartCoroutine(RunSingleTurn());

        for (int i = 0; i < 21; i++) {
            if (bj.ongoing) {
                currentState = ss.IdentifySpace(stateSpaceSize, bj.agentScore, bj.low, bj.high, bj.usableAce, bj.dealerHand);

                string action = MakeChoice(currentState);

                float reward = bj.DealCards(action);

                if (action == "double") {
                    bj.canAgentDouble = false;
                }

                float Q = stateSpace[currentState][action];

                int nextState = ss.IdentifySpace(stateSpaceSize, bj.agentScore, bj.low, bj.high, bj.usableAce, bj.dealerHand);

                float QNextMax = Mathf.Max(stateSpace[nextState]["hit"], stateSpace[nextState]["stay"]);

                float QNew = Q + learningRate * (reward + discountRate * QNextMax - Q);
                stateSpace[currentState][action] = QNew;
            }
            else {
                break;
            }
        }
    }

    public void RunQLearning() {

        StartCoroutine(RunEpisodes());

        
    }

    private IEnumerator RunSingleTurn() {
        while (bj.ongoing) {
            currentState = ss.IdentifySpace(stateSpaceSize, bj.agentScore, bj.low, bj.high, bj.usableAce, bj.dealerHand);

            string action = MakeChoice(currentState);

            float reward = bj.DealCards(action);

            if (action == "double") {
                bj.canAgentDouble = false;
            }

            float Q = stateSpace[currentState][action];

            int nextState = ss.IdentifySpace(stateSpaceSize, bj.agentScore, bj.low, bj.high, bj.usableAce, bj.dealerHand);

            float QNextMax = Mathf.Max(stateSpace[nextState]["hit"], stateSpace[nextState]["stay"]);

            float QNew = Q + learningRate * (reward + discountRate * QNextMax - Q);
            stateSpace[currentState][action] = QNew;

            yield return null;
        }
    }

    private IEnumerator RunEpisodes() {

        int iteration = 0;

        while (iteration < episodes) {
            RunEpisode();

            iteration++;

            //epsilon -= (1 / episodes * 10);

            if (iteration % 100 == 0) {
                //print(iteration);
            }

            yield return null;
        }
    }
}

