using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QLearning : MonoBehaviour {

    public static QLearning ql;

    public int wins = 0;

    [Header("Q Learning params")]
    // Learning rate
    public float learningRate = 0.05f;
    // Discount factor
    public float discountRate = 0.9f;
    // Chance to explore or act greedily
    public float epsilon = 0.1f;
    // Number of episodes to train agent on
    public int episodes = 1;

    // Ratio of reward to betting
    public float bettingOdds = 1f;
    // Shuffle deck when deck count is less than this
    public int shuffleDeck = 26;

    // State-action space size
    public string stateSpaceSize = "small";
    // Actual state space
    public List<Dictionary<string, float>> stateSpace;
    // State space index
    public int currentState;

    // Dealer must hit if dealer score less than this
    public int dealerStop = 16;
    
    // 



    private string[] choices = { "hit", "stay", "double" };
    private string[] policy = { "greedy", "explore" };

    private Blackjack bj;
    private StateSpace ss;

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


    // Given current state, determine to hit, stay, or double
    public string MakeChoice(int currentState) {
        // If agent hit blackjack, stay
        if (bj.agentScore == 21) {
            return choices[1];
        }

        // Determine whether to act greedily or explore using epsilon greedy
        string chosenPolicy;
        float rand = Random.Range(0f, 1f);
        if (rand <= 1-epsilon) {
            chosenPolicy = policy[0];
        }
        else {
            chosenPolicy = policy[1];
        }

        string decision;

        // Act greedily
        if (chosenPolicy == policy[0]) {
            decision = choices[0];

            // Determine action to take using the Q table
            if (bj.canAgentDouble) {
                // hit vs stay, double vs stay
                if (stateSpace[currentState][choices[0]] < stateSpace[currentState][choices[1]] && 
                    stateSpace[currentState][choices[2]] < stateSpace[currentState][choices[1]]) {
                    decision = choices[1];
                }
                // hit vs double
                else if (stateSpace[currentState][choices[0]] < stateSpace[currentState][choices[2]]) {
                    decision = choices[2];
                }
            }
            else {
                // hit vs stay
                if (stateSpace[currentState][choices[0]] < stateSpace[currentState][choices[1]]) {
                    decision = choices[1];
                }
            }
        }
        // Randomly choose action to take
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

        for (int i = 0; i < episodes; i++) {
            RunEpisode();
        }
    }
}

