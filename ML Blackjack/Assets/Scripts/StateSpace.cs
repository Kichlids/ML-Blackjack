using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateSpace : MonoBehaviour
{
    public static StateSpace ss;
    private void Awake() {
        if (ss != null && ss != this) {
            Destroy(this.gameObject);
        }
        else {
            ss = this;
        }
    }

    private void Start() {
        GenerateStateSpace("large");
    }
    public List<Dictionary<string, float>> GenerateStateSpace(string size) {

        if (size == "large") {
            return GenerateLargeStateSpace();
        }
        else if (size == "medHigh") {
            return GenerateMediumHighStateSpace();
        }
        else if (size == "medLow") {
            return GenerateMediumLowStateSpace();
        }
        else {
            return GenerateSmallStateSpace();
        }
    }

    private List<Dictionary<string, float>> GenerateLargeStateSpace() {
        List<Dictionary<string, float>> stateSpace = new List<Dictionary<string, float>>();

        for (int size = 0; size < 21; size++) {
            for (int low = 0; low < 21; low++) {
                for (int high = 0; high < 21; high++) {
                    for (int ace = 0; ace < 2; ace++) {
                        for (int dealerHand = 0; dealerHand < 12; dealerHand++) {
                            Dictionary<string, float> state = new Dictionary<string, float>() {
                                { "double", 0 },
                                { "hit", 0 },
                                { "stay", 0 }
                            };

                            stateSpace.Add(state);
                        }
                    }
                }
            }
        }

        for (int i = 0; i < 2; i++) {
            Dictionary<string, float> state = new Dictionary<string, float>() {
                                { "double", 0 },
                                { "hit", 0 },
                                { "stay", 0 }
            };
            stateSpace.Add(state);
        }

        return stateSpace;
    }

    private List<Dictionary<string, float>> GenerateMediumHighStateSpace() {
        List<Dictionary<string, float>> stateSpace = new List<Dictionary<string, float>>();

        for (int size = 0; size < 21; size++) {
            for (int high = 0; high < 21; high++) {
                for (int ace = 0; ace < 2; ace++) {
                    for (int dealerHand = 0; dealerHand < 12; dealerHand++) {
                        Dictionary<string, float> state = new Dictionary<string, float>() {
                            { "double", 0 },
                            { "hit", 0 },
                            { "stay", 0 }
                        };

                        stateSpace.Add(state);
                    }
                }
            }
        }

        for (int i = 0; i < 2; i++) {
            Dictionary<string, float> state = new Dictionary<string, float>() {
                                { "double", 0 },
                                { "hit", 0 },
                                { "stay", 0 }
            };
            stateSpace.Add(state);
        }

        return stateSpace;
    }

    private List<Dictionary<string, float>> GenerateMediumLowStateSpace() {
        List<Dictionary<string, float>> stateSpace = new List<Dictionary<string, float>>();

        for (int size = 0; size < 21; size++) {
            for (int low = 0; low < 21; low++) {
                for (int ace = 0; ace < 2; ace++) {
                    for (int dealerHand = 0; dealerHand < 12; dealerHand++) {
                        Dictionary<string, float> state = new Dictionary<string, float>() {
                            { "double", 0 },
                            { "hit", 0 },
                            { "stay", 0 }
                        };

                        stateSpace.Add(state);
                    }
                }
            }
        }

        for (int i = 0; i < 2; i++) {
            Dictionary<string, float> state = new Dictionary<string, float>() {
                                { "double", 0 },
                                { "hit", 0 },
                                { "stay", 0 }
            };
            stateSpace.Add(state);
        }

        return stateSpace;
    }

    private List<Dictionary<string, float>> GenerateSmallStateSpace() {
        List<Dictionary<string, float>> stateSpace = new List<Dictionary<string, float>>();

        for (int size = 0; size < 21; size++) {
            for (int ace = 0; ace < 2; ace++) {
                for (int dealerHand = 0; dealerHand < 12; dealerHand++) {
                    Dictionary<string, float> state = new Dictionary<string, float>() {
                        { "double", 0 },
                        { "hit", 0 },
                        { "stay", 0 }
                    };

                    stateSpace.Add(state);
                }
            }
        }

        for (int i = 0; i < 2; i++) {
            Dictionary<string, float> state = new Dictionary<string, float>() {
                                { "double", 0 },
                                { "hit", 0 },
                                { "stay", 0 }
            };
            stateSpace.Add(state);
        }

        return stateSpace;
    }

    public int IdentifySpace(string size, int playerScore, int low, int high, int usableAce, List<Card> dealerHand) {
        if (size == "large") {
            return IdentifyLargeStateSpace(playerScore, low, high, usableAce, dealerHand);
        }
        else if (size == "medHigh") {
            return IdentifyMediumHighStateSpace(playerScore, high, usableAce, dealerHand);
        }
        else if (size == "medLow") {
            return IdentifyMediumLowStateSpace(playerScore, low, usableAce, dealerHand);
        }
        else {
            return IdentifySmallStateSpace(playerScore, usableAce, dealerHand);
        }
    }

    private int IdentifyLargeStateSpace(int score, int low, int high, int ace, List<Card> dealerHand) {
        // 21 * 21 * 21 * 2 * 12 = 222264
        // Plus two more state space for 21 and bust = 222266

        int spaceIndex;

        if (score < 21) {
            spaceIndex = score * 10584 + low * 504 + high * 24 + ace * 12 + dealerHand[0].cardValue;
        }
        else if (score == 21) {
            spaceIndex = 222264;
        }
        else {
            spaceIndex = 222265;
        }

        return spaceIndex;
    }

    private int IdentifyMediumHighStateSpace(int score, int high, int ace, List<Card> dealerHand) {
        // 21 * 21 * 2 * 12 = 10584
        // Plus two more state space for 21 and bust = 10586

        int spaceIndex;

        if (score < 21) {
            spaceIndex = score * 504 + high * 24 + ace * 12 + dealerHand[0].cardValue;
        }
        else if (score == 21) {
            spaceIndex = 10584;
        }
        else {
            spaceIndex = 10585;
        }

        return spaceIndex;
    }

    private int IdentifyMediumLowStateSpace(int score, int low, int ace, List<Card> dealerHand) {
        // 21 * 21 * 2 * 12 = 10584
        // Plus two more state space for 21 and bust = 10586

        int spaceIndex;

        if (score < 21) {
            spaceIndex = score * 504 + low * 24 + ace * 12 + dealerHand[0].cardValue;
        }
        else if (score == 21) {
            spaceIndex = 10584;
        }
        else {
            spaceIndex = 10585;
        }

        return spaceIndex;
    }

    private int IdentifySmallStateSpace(int score, int ace, List<Card> dealerHand) {
        // 21 * 2 * 12 = 504
        // Plus two more state space for 21 and bust = 506

        int spaceIndex;

        if (score < 21) {
            spaceIndex = score * 24 + ace * 12 + dealerHand[0].cardValue;
        }
        else if (score == 21) {
            spaceIndex = 504;
        }
        else {
            spaceIndex = 505;
        }

        return spaceIndex;
    }
}
