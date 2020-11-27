using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Card", order = 51)]
public class CardData : ScriptableObject
{
    [SerializeField]
    public string cardName;
    [SerializeField]
    public int value;
    [SerializeField]
    public GameObject cardPrefab;
    [SerializeField]
    public bool isAce;
}
