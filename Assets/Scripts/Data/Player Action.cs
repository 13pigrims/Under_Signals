using UnityEngine;

[System.Serializable]
public class PlayerAction
{
    public string submittedHullId;       // 玩家输入的船籍
    public int statusIndex;              // 0:NOMINAL, 1:DAMAGE, 2:MEDICAL
    public int cargoIndex;               // 0:ORE, 1:FUEL, 2:FOOD, 3:EMPTY
    public int intentIndex;              // 0:RESUPPLY, 1:OFFLOAD, 2:EMERGENCY
    public bool isApproved;              // true=Submit & Clear, false=Report & Reject
    public bool isCorrect;               // 是否译电正确（由GameManager计算）
}