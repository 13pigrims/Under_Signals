using UnityEngine;

// 枚举定义，顺序必须与下拉菜单选项顺序一致
public enum ShipStatus { Nominal, Damage, Medical }      // 0,1,2
public enum ShipCargo { Ore, Fuel, Food, Empty }         // 0,1,2,3
public enum ShipIntent { Resupply, Offload, Emergency }  // 0,1,2

[System.Serializable]
public class ShipData
{
    public int day;
    public string shipId;               // 例如 "K-002"
    public string hullCode;             // Q1 原始编码
    public string hullRoute;            // "K"

    // Q2 原始颜色值
    public string q2_hull;
    public string q2_life;
    public string q2_comms;

    // Q3 原始货物值
    public string q3_bayA;
    public string q3_bayB;
    public string q3_bayC;

    // Q4 原始波形和意图
    public string q4_waveform;
    public string q4_intent;

    // 警告代码
    public string warningCode;
    public string warningCodeMeaning;

    // 真实状态（用于比对译电正确性）
    public ShipStatus status;
    public ShipCargo cargo;
    public ShipIntent intent;

    public bool isMimic;
    public string anomalyExplanation;

    // 图片路径（可选）
    public string morseImage;
    public string colorsImage;
    public string shapesImage;
    public string waveformImage;
    public string centerIconImage;

    // 终端显示文本
    [TextArea(3, 10)]
    public string terminalRevealText;
}