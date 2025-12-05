using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class csStatUpdate : MonoBehaviour
{
    public GameObject cat;
    private CatState state;

    [SerializeField]
    private float _hunger;
    [SerializeField]
    private float _sleepiness;
    [SerializeField]
    private float _happiness;

    public Slider slHunger;
    public Slider slSleepiness;
    public Slider slHappiness;

    public Toggle tgHunger;
    public Toggle tgSleepiness;
    public Toggle tgHappiness;

    public TMPro.TMP_Text txtPersona;
    public CatPersonality.PersonalityType _personality;
    public string personality;


    private void OnEnable()
    {
        cat = GameObject.FindWithTag("Player");
        if (cat != null) state = cat.GetComponent<CatState>();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene sc, LoadSceneMode mode)
    {
        Debug.Log(sc.name + "씬 로드됨");

        if(cat != null)
        {
            state.personality.type = _personality;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (cat == null)
        {
            Debug.Log("cat 없어");
            return;
        }
        // 내부 변수로 가져오기
        _hunger = state.catState.hunger / 10.0f;
        _sleepiness = state.catState.sleepiness / 10.0f;
        _happiness = state.catState.happiness / 10.0f;

        // 슬라이더에 값 설정
        slHunger.value = _hunger;
        slSleepiness.value = _sleepiness;
        slHappiness.value = _happiness;

        // 토글에 값 설정
        tgHunger.isOn = state.isHungry;
        tgSleepiness.isOn = state.isSleepy;
        tgHappiness.isOn = state.isHappy;


        // 성격 설정
        state.personality.type = _personality;
        txtPersona.text = personality;
    }

    public void SetStat(int num)
    {
        switch (num)
        {
            case 0:
                _personality = CatPersonality.PersonalityType.순종;
                personality = "순종";
                break;
            case 1:
                _personality = CatPersonality.PersonalityType.온순;
                personality = "온순";
                break;
            case 2:
                _personality = CatPersonality.PersonalityType.고집;
                personality = "고집";
                break;
        }
    }
}
