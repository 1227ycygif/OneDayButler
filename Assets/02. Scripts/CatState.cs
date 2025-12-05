using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class CatStatus
{
    [Header("고양이 상태")]
    [Range(0, 10)] public int hunger = 5;      // 포만감
    [Range(0, 10)] public int affection = 5;   // 친밀도
    [Range(0, 10)] public int happiness = 5;   // 행복
    [Range(0, 10)] public int sleepiness = 5;  // 스태미너(졸림)
}

public class CatState : MonoBehaviour
{
    [SerializeField]
    public CatStatus catState { get; set; }
    [SerializeField] public CatPersonality personality;

    [Header("상태 플래그")]
    [Tooltip("고양이 상태 Bool 변수")]
    // 상태 플래그
    public bool isHungry;     // 배고픈지
    public bool isBoring;     // 심심한지
    public bool isSleepy;     // 졸린지
    public bool isExcrete;  // 배설 여부
    public bool isHappy;      // 행복한지
    public bool isRoaming;    // 로밍 중인지
    public bool nonRoaming; // 로밍 대기 중인지

    public bool isShower;     // 샤워 중인지

    public bool isStand;        // 서 있는지
    public string previousState;// 이전 상태

    int hungryStack = 0;
    int sleepStack = 0;


void Awake()
{
    if (catState == null) catState = new CatStatus();
    if (personality == null)
        personality = new CatPersonality { type = CatPersonality.PersonalityType.온순 };
}


    void Start()
    {
        CheckHungry();
        CheckHappy();
        StartCoroutine(CatSimulator());
        StartCoroutine(CatStateCheck());
    }

    IEnumerator CatStateCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            CheckHungry();
            CheckHappy();
            CheckSleep();
            CheckShower();
        }
    }

    IEnumerator CatSimulator()
    {

        while (true)
        {

            yield return new WaitForSeconds(30.0f);
            int howFull = catState.hunger; // Hungry() 들어가기전 저장 후 Affection()에서 계산
            catState.hunger = Hungry();
            catState.sleepiness = Sleepiness();
            catState.affection = Affection(howFull);
            catState.happiness = Happy(howFull);
        }
    }

    protected int Hungry()
    {
        if (catState.hunger > 0) { catState.hunger -= 1; }
        else if (catState.hunger <= 0 && hungryStack < 2) { hungryStack++; catState.affection -= 1; }
        else if (catState.hunger <= 0 && hungryStack >= 2) { catState.affection -= 1; catState.happiness -= 1; }
        if (catState.hunger >= 1 && hungryStack >= 2) { hungryStack++; }
        ClampAll();
        return catState.hunger;
    }

    protected int Sleepiness()
    {
        sleepStack++;

        if (sleepStack == 2) { catState.sleepiness -= 1; sleepStack = 0; }
        else if (catState.sleepiness <= 0)
        {
            /*강제슬립*/
            sleepStack = 0;
        }
        ClampAll();
        return catState.sleepiness;
    }
    protected int Affection(int howFull)
    {
        if (catState.hunger - 4 >= howFull)
        {
            catState.affection += 1;
        }
        if (catState.affection <= 0)
        {
            catState.happiness -= 1;
        }
        ClampAll();
        return catState.affection;
    }
    protected int Happy(int howFull)
    {
        if (catState.hunger == 10)
        {
            catState.happiness += 1;
        }
        ClampAll();
        return catState.happiness;
    }

    void CheckHungry()
    {
        int h = catState.hunger;

        switch (personality.type)
        {
            case CatPersonality.PersonalityType.순종:
            if (h <= 2){isHungry = true; } else {isHungry = false;}
                break;
            case CatPersonality.PersonalityType.온순:
            if (h <= 3){isHungry = true; } else {isHungry = false;}
                break;
            case CatPersonality.PersonalityType.고집:
            if (h <= 4){isHungry = true; } else {isHungry = false;}
                break;
        }
    }

    void CheckHappy()
    {
        int h = catState.happiness;

        switch (personality.type)
        {
            case CatPersonality.PersonalityType.순종:
                if (h >= 4){isHappy = true; } else {isHappy = false;}
                break;
            case CatPersonality.PersonalityType.온순:
                if (h >= 6){isHappy = true; } else {isHappy = false;}
                break;
            case CatPersonality.PersonalityType.고집:
                if (h >= 9){isHappy = true; } else {isHappy = false;}
                break;
        }
    }
     void CheckSleep()
    {
        if (catState.sleepiness <= 0)
        {
            isSleepy = true;
        }
        else
        {
            isSleepy = false;
        }
    }

    void CheckShower()
    {
        
    }
    
    public void ClampAll()
    {
        catState.hunger = Mathf.Clamp(catState.hunger, 0, 10);
        catState.affection = Mathf.Clamp(catState.affection, 0, 10);
        catState.happiness = Mathf.Clamp(catState.happiness, 0, 10);
        catState.sleepiness = Mathf.Clamp(catState.sleepiness, 0, 10);
    }

    public int DontPoop(AudioClip sfx)
    {
        Debug.Log("화장실 못 가서 화냄");
        // Toilet Angry
        if (sfx != null) // 화낼 때, Meow1
        {
            AudioSource.PlayClipAtPoint(sfx, transform.position);
        }

        return catState.happiness - 2;
    }
}
