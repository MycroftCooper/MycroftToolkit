using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillData {
    public string Id;
    public string Name;
    public string Description;
    public string Icon;
    public string Type;
    
    public float CastingTime;// 释放时间
    public float Duration;// 持续时间
    public float Cooldown;
    public float Cost;

    public string Animation;
    public Dictionary<string, string> VisualEffects;
    public Dictionary<string, string> SoundEffects;
    
    public Dictionary<string, object> CustomData;
}
