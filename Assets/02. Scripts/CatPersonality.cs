using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CatPersonality
{
    public enum PersonalityType
    {
        순종,
        온순,
        고집
    }

    [Header("고양이 성격")]
    public PersonalityType type;
}

[CreateAssetMenu(menuName = "Cat/Personality Preset")]
public class CatPersonalityPreset : ScriptableObject
{
    public CatPersonality.PersonalityType type;
}