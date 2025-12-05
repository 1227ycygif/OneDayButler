using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animation))]
public class CatAnimation : MonoBehaviour
{
    // ====== 외부 연동(선택) ======
    public CatState cat;           // 있으면 연동, 없으면 내부 수치 사용
    public CatStatus catStatus;
    public csSelectCat SelectCat;

    public csBowl bowl;            // 선택: bowl.checkEmpty() 사용 가능
    public csToilet toilet;     // 선택: toilet.checkEmpty(), toilet.poop()
    public Wash wash;

    // ====== 목표 지점 ======
    [Header("Targets (NavMesh 위에 배치)")]
    public Transform bowlPoint;
    public Transform bedPoint;
    public Transform toiletPoint;

    [Header("Roaming Points Root (자식 Transform들 사용)")]
    public Transform roamingRoot;

    // ====== 이동/판정 ======
    [Header("Nav")]
    public Vector2 moveClipRange = new Vector2(0f, 10f);
    [Min(0.05f)] private float arriveRadius = 0.25f;
    [Min(0.1f)] private float sampleRadius = 2.5f;   // NavMesh.SamplePosition 반경
    [Min(0.1f)] private float rotateLerp = 6f;


    private float walkSpeed = .5f;
    private float runSpeed = 3.0f;

    [Header("Rules")]
    private Vector2 excreteDelayRange = new Vector2(40f, 70f); // 먹은 후 배설 트리거까지
    public Vector2 roamMoveTime = new Vector2(6f, 12f);
    private Vector2 roamIdleTime = new Vector2(5f, 7f);

    // ====== 애니메이션(레거시) ======
    [System.Serializable]
    public class Clips
    {
        public AnimationClip idle, walk, run;
        public AnimationClip eating;
        public AnimationClip sit, seated, stand;
        public AnimationClip belly_sit, belly_seated, belly_sleep;
        public AnimationClip happy1, happy2, lick, scratch;
        public AnimationClip jump;
    }

    [Header("Animation Clips")]
    public Clips anims;

    // ====== 내부 ======
    enum State { Idle, Move, Eat, Sleep, Excrete, Wash, Select, None }
    State state = State.None;
    Coroutine action;
    NavMeshAgent agent;
    Animation anim;
    Transform tr;
    AudioSource audio;
    AnimationClip moveClip;

    GameObject VRPlayer;    // 플레이어
    public csSoundManager mixer;

    Transform[] roamPoints;
    int lastRoamIdx = -1;

    // 트리거/플래그
    bool wantWash;
    Vector3 washDest;

    bool catSelect;

    bool jumping;

    Vector3 endWashDest;
    float nextExcreteAt = -1f; // Time.time 기준. <0 이면 예약 없음.

    [Space(20)]
    [Header("사운드")]
    // 고양이 소리
    public AudioClip Eatsfx; // 먹을 때 
    public AudioClip Meow2sfx; // Happy할 때 
    public AudioClip Sitsfx; // 자러서 앉을 때
    public AudioClip Sleepsfx; // 잘 때 
    public AudioClip Walksfx; // 걸어갈 때
    public AudioClip Runsfx; // 걸어갈 때
    public AudioClip WashMove; // 씻기기 이동할 때 나는 소리
    public AudioClip Angrysfx;
    public AudioClip Toiletsfx;


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animation>();
        audio = GetComponent<AudioSource>();
        tr = transform;

        agent.updateRotation = false;
        agent.stoppingDistance = arriveRadius;
        agent.speed = walkSpeed;
        agent.autoTraverseOffMeshLink = false;

        ForceLoop(anims.idle); ForceLoop(anims.walk); ForceLoop(anims.run);
        ForceOnce(anims.eating);
        ForceOnce(anims.sit); ForceLoop(anims.seated); ForceOnce(anims.stand);
        ForceOnce(anims.belly_sit); ForceLoop(anims.belly_seated); ForceLoop(anims.belly_sleep);
        ForceLoop(anims.happy1); ForceLoop(anims.happy2); ForceLoop(anims.lick); ForceLoop(anims.scratch);

        if (anims.idle)
        {
            anim.clip = anims.idle; anim.Play();
        }

        // Roaming points
        if (roamingRoot)
        {
            roamPoints = roamingRoot.GetComponentsInChildren<Transform>();
        }
    }

    void Start()
    {
        ChangeState(State.Idle);
        StartCoroutine(CatLoop());

        VRPlayer = Camera.main.transform.parent.parent.gameObject;
        mixer = FindAnyObjectByType<csSoundManager>();
    }

    void Update()
    {
        catStatus = cat.catState;

        if (!cat.isShower)
        {
            // 회전 보정
            if (!agent.isStopped && agent.hasPath && agent.velocity.sqrMagnitude > 0.001f)
            {
                Vector3 dir = agent.steeringTarget - tr.position;
                if (dir.sqrMagnitude > 0.001f)
                {
                    Quaternion rot = Quaternion.LookRotation(dir);
                    tr.rotation = Quaternion.Slerp(tr.rotation, rot, Time.deltaTime * rotateLerp);
                }
            }
        }
        else if (cat.isShower)
        {
            return;
        }
    }
    // ===== 외부 API =====
    public void CatSelect()
    {
        catSelect = true;
        Debug.Log("고양이 선택 : " + (catSelect ? true : false));

    }
    public void CatSelectQuit()
    {
        catSelect = false;
        Debug.Log("고양이 선택 : " + (catSelect ? true : false));
    }
    public void StartWash(Transform washPoint)
    {
        if (washDest == null) return;

        wantWash = true;
        washDest = washPoint.position;
        transform.rotation = Quaternion.identity;
        
        wash.WashStart();
        // 지금 액션이 길더라도 Urgent 취급: 다음 틱에서 Wash로 선점

        cat.isShower = true;
    }
    public void EndWash(Transform endWashPoint)
    {
        wantWash = false;
        endWashDest = endWashPoint ? endWashPoint.position : tr.position;
        cat.isShower = false;
        VRPlayer.transform.position = SelectCat.endPlayerPoint.position;

    } // 플레이어 완료 신호

    // ====== 메인 루프 (우선순위 판단) ======
    IEnumerator CatLoop()
    {
        while (true)
        {
            // 최우선 선택
            if (catSelect)
            {
                ChangeState(State.Select);
                yield return RunSelect();
                continue;
            }
            // 1. 씻기기
            if (wantWash)
            {
                ChangeState(State.Wash);
                yield return RunWash(); // 완료/타임아웃까지 점유
                continue;
            }

            // 2. 배설 예약 시점 도달
            if (HasCat() && cat.isExcrete && (nextExcreteAt > 0 && Time.time >= nextExcreteAt))
            {
                // 화장실 가능 여부 확인(선택)
                if (!toilet || toilet.CheckEmpty())
                {
                    yield return new WaitForSeconds(1.0f);

                    ChangeState(State.Excrete);
                    yield return RunExcrete();
                    continue;
                }
                else
                {
                    // 혼잡/더러움 → 한 번만 화냄(스팸 방지)
                    LogOnce(ref toiletAnnounceCooldown, 8f, "화장실 혼잡/더러움: 잠시 후 재시도");
                    if (HasCat())
                    {
                        cat.DontPoop(Angrysfx);
                    }
                    // 재시도: 타이머 유지 → 다음 loop에서 다시 판단
                }
            }

            // 3. 배고픔
            if (cat.isHungry)
            {
                // 그릇 준비 확인(선택)
                if (!bowl || !bowl.CheckEmpty())
                {
                    yield return new WaitForSeconds(1.0f);

                    ChangeState(State.Eat);
                    yield return RunEat();
                    continue;
                }
                // 그릇 준비 안됨 → 대기/로밍
                else
                {
                    mixer.PlaySFXAtPoint(Angrysfx, cat.transform.position);
                }
            }

            // 4. 졸림
            if (cat.isSleepy)
            {
                yield return new WaitForSeconds(1.0f);

                ChangeState(State.Sleep);
                yield return RunSleep();
                continue;
            }

            // 5. 로밍
            ChangeState(State.Move);
            yield return RunRoamOnce();
        }
    }

    // ====== 액션 ======
    IEnumerator RunSelect()
    {
        if (catSelect == false)
        {
            agent.isStopped = true;
            agent.ResetPath();

            Quaternion rot = Quaternion.LookRotation(VRPlayer.transform.position);
            tr.rotation = Quaternion.LookRotation(VRPlayer.transform.position);
            yield return new WaitUntil(() => !catSelect);
        }
        if (catSelect == true)
        {
            Debug.Log("이미 catSelect == true");
        }
    }
    IEnumerator RunWash(float hardTimeout = 120f)
    {
        Debug.Log("RunWash");
        agent.isStopped = true; agent.ResetPath(); agent.Warp(washDest);

        Play(anims.idle, 0.1f, 1f, false);
        cat.catState.happiness = -1;

        float until = Time.time + hardTimeout;

        // 플레이어가 EndWash() 호출할 때까지 대기
        AudioSource.PlayClipAtPoint(WashMove, new Vector3 (39.51f, 2f, -11.47f));
        VRPlayer.transform.position = SelectCat.washPlayerPoint.position;
        VRPlayer.transform.rotation = SelectCat.washPlayerPoint.rotation;
        yield return new WaitUntil(() => !wantWash || Time.time > until);

        // 종료
        agent.isStopped = false; agent.Warp(endWashDest);
        Play(anims.idle, 0.1f, 1f, false);
        yield return null;
    }

    IEnumerator RunEat()
    {
        if (!bowlPoint)
        {
            yield break;
        }
        yield return EnsureAt(bowlPoint.position, walkSpeed);

        // 먹기
        Debug.Log("RunEat");
        agent.isStopped = true; agent.ResetPath();
        Play(anims.eating, 0.12f, 1f, false);

        if (Eatsfx != null) // 먹을 때
        {
            mixer.PlaySFXAtPoint(Eatsfx, transform.position);
        }
        yield return WaitClip(anims.eating);

        Play(anims.idle, 0.18f, 1f, false);

        // 수치 갱신
        if (HasCat())
        {
            cat.catState.hunger = Mathf.Min(cat.catState.hunger + 4, 10);
            cat.catState.affection = Mathf.Min(cat.catState.affection + 1, 10);
            cat.ClampAll();

            // 배설 예약은 CatState를 쓰든 여기서 쓰든 한 곳만 책임지자. 여기서 예약:
            nextExcreteAt = Time.time + Random.Range(excreteDelayRange.x, excreteDelayRange.y); // 시간 설정
            cat.isExcrete = true; 
        }
        else
        {
            cat.catState.hunger = Mathf.Min(cat.catState.hunger + 4, 10);
            cat.catState.happiness = Mathf.Min(cat.catState.happiness + 1, 10);
            nextExcreteAt = Time.time + Random.Range(excreteDelayRange.x, excreteDelayRange.y);
        }
        bowl.Eat();
        yield return new WaitForSeconds(Random.Range(1.2f, 2.0f));
    }

    IEnumerator RunExcrete()
    {
        if (!toiletPoint)
        {
            yield break;
        }
        yield return EnsureAt(toiletPoint.position, walkSpeed);

        Debug.Log("RunExcrete");
        agent.isStopped = true; agent.ResetPath();
        if (anims.sit)
        {
            Play(anims.sit, 0.12f, 1f, false);
            yield return WaitClip(anims.sit);
        }

        if (anims.seated)
        {
            Play(anims.seated, 0.12f, 1f, false);
            yield return WaitClip(anims.seated);
        }

        if (anims.stand)
        {
            Play(anims.stand, 0.12f, 1f, false);
            yield return WaitClip(anims.stand);
        }
        Play(anims.idle, 0.18f, 1f, false);

        if (toilet)
        {
            toilet.Poop();
        }

        if (HasCat())
        {
            cat.isExcrete = false;
        }
        nextExcreteAt = -1f;

        yield return new WaitForSeconds(Random.Range(1.0f, 2.0f));
    }

    IEnumerator RunSleep()
    {
        if (!bedPoint)
        {
            yield break;
        }

        yield return EnsureAt(bedPoint.position, walkSpeed);

        agent.isStopped = true; agent.ResetPath();
        Debug.Log("RunSleep");

        if (anims.belly_sit)
        {
            Play(anims.belly_sit, 0.12f, 1f, false);

            if (Sitsfx != null) // 자러 가서 앉을 때
            {
                mixer.PlaySFXAtPoint(Sitsfx, transform.position);
            }
            yield return WaitClip(anims.belly_sit);
        }

        if (anims.belly_seated)
        {
            Play(anims.belly_seated, 0.12f, 1f, false);
            yield return WaitClip(anims.belly_seated);
        }

        float sleepSec = Random.Range(8f, 14f);

        if (anims.belly_sleep)
        {
            Play(anims.belly_sleep, 0.12f, 1f, false);
        }

        if (Sleepsfx != null)
        {
            mixer.PlaySFXAtPoint(Sleepsfx, transform.position);
        }

        yield return new WaitForSeconds(sleepSec);

        // 회복
        if (HasCat())
        {
            cat.catState.sleepiness = Mathf.Min(cat.catState.sleepiness + Mathf.RoundToInt(sleepSec * 0.5f), 10);
            cat.ClampAll();
        }
        else
        {
            cat.catState.sleepiness = Mathf.Min(cat.catState.sleepiness + Mathf.RoundToInt(sleepSec * 0.5f), 10);
        }

        Play(anims.idle, 0.18f, 1f, false);
        yield return new WaitForSeconds(1.0f);
    }

    IEnumerator RunRoamOnce()
    {
        // 포인트 없으면 Idle 잠깐
        if (roamPoints == null || roamPoints.Length <= 1)
        {
            Play(anims.idle, 0.18f, 1f, false);
            yield return new WaitForSeconds(Random.Range(roamIdleTime.x, roamIdleTime.y));
            yield break;
        }

        // 다음 포인트 선택(최근과 너무 가까우면 재추첨)
        int tries = 0; int idx = lastRoamIdx;

        while (tries++ < 8)
        {
            int cand = Random.Range(1, roamPoints.Length);
            if (roamPoints.Length == 2 && cand == lastRoamIdx)
            {
                cand = (cand == 1) ? 2 : 1;
            }
            if (cand == lastRoamIdx)
            {
                continue;
            }

            Vector3 p = roamPoints[cand].position;
            if (Vector3.Distance(tr.position, p) < 0.5f)
            {
                continue;
            }
            idx = cand;
            break;
        }
        lastRoamIdx = idx;

        // 이동
        Vector3 dst = roamPoints[idx].position;
        float hold = Random.Range(roamMoveTime.x, roamMoveTime.y);     
        
        // 오디오 믹서랑 연결된 싱글톤 오브젝트(Sound Manager) 이용중
        yield return MoveWithRetry(dst, hold);
        mixer.PlaySFXAtPoint(Walksfx, transform.position, true);

        // 잠깐 Idle
        Play(anims.idle, 0.18f, 1f, false);

        // 다 이동했으면 걷기, 뛰기 클립 재생 해제
        mixer.SFXAllStop();
        
        yield return new WaitForSeconds(Random.Range(roamIdleTime.x, roamIdleTime.y));
    }
    // ====== 이동/보조 ======
    IEnumerator EnsureAt(Vector3 dst, float speed)
    {
        // 걷기, 뛰기 클립 재생(확률 랜덤) 
        float runMoveProbability = Random.Range(moveClipRange.x, moveClipRange.y);
        if (runMoveProbability > 3.0f) 
        { 
            // 걷기 
            moveClip = anims.walk; 
            Play(moveClip, 0.18f, 1f, false); 
            agent.speed = walkSpeed;
        }
        else if (runMoveProbability <= 3.0f) 
        { 
            // 뛰기 
            moveClip = anims.run; 
            Play(moveClip, 0.18f, 1f, false); 
            agent.speed = runSpeed; 
        }

        Play(moveClip, 0.18f, 1f, false);

        if (!NavMesh.SamplePosition(dst, out var hit, sampleRadius, NavMesh.AllAreas))
        {
            yield break;
        }

        agent.isStopped = false;
        agent.ResetPath();
        agent.SetDestination(hit.position);

        // 정체 감지 기반 대기
        float stuck = 0f, lastRem = Mathf.Infinity;
        while (!Arrived())
        {
            float rem = agent.remainingDistance;
            if (Mathf.Abs(rem - lastRem) < 0.01f && agent.velocity.sqrMagnitude < 0.01f)
            {
                stuck += Time.deltaTime;
            }
            else 
            { 
                stuck = 0f; 
                lastRem = rem; 
            }

            if (stuck > 2.5f)
            {
                // 경로 탐색 재시도
                agent.ResetPath();
                agent.SetDestination(hit.position);
                stuck = 0f;
            }
            yield return null;
        }

        agent.isStopped = true;
        Play(anims.idle, 0.18f, 1f, false);
        yield return null;
    }

    IEnumerator MoveWithRetry(Vector3 dst, float maxDur)
    {
        float runMoveProbability = Random.Range(moveClipRange.x, moveClipRange.y);

        if (runMoveProbability > 3.0f) 
        { 
            moveClip = anims.walk; 
            Play(moveClip, 0.18f, 1f, false); 
            agent.speed = walkSpeed; 
        }
        else if (runMoveProbability <= 3.0f) 
        { 
            moveClip = anims.run; 
            Play(moveClip, 0.18f, 1.5f, false); 
            agent.speed = runSpeed; 
        }

        if (!NavMesh.SamplePosition(dst, out var hit, sampleRadius, NavMesh.AllAreas))
        {
            yield break;
        }

        agent.isStopped = false; agent.ResetPath(); agent.SetDestination(hit.position);

        float until = Time.time + maxDur + 6f; // 타임아웃 설정, 안전 여유
        float stuck = 0f, lastRem = Mathf.Infinity;

        while (!Arrived())
        {
            // 타임아웃 체크 
            if (Time.time > until)
            {
                break;
            }

            float rem = agent.remainingDistance;
            // 남은 거리, 속도 → 정체 감지
            if (Mathf.Abs(rem - lastRem) < 0.01f && agent.velocity.sqrMagnitude < 0.01f)
            {
                stuck += Time.deltaTime;
            }
            else
            {
                stuck = 0f;
                lastRem = rem;
            }

            if (stuck > 2.5f)
            {
                agent.ResetPath();
                agent.SetDestination(hit.position);
                stuck = 0f;
            }
            yield return null;
        }

        agent.isStopped = true;
        yield return null;
    }

    bool Arrived()
    {
        // 경로 계산 → 도착 X 
        if (agent.pathPending)
        {
            return false;
        }

        // 남은 거리 > 정지 거리 →도착 X 
        if (agent.remainingDistance > Mathf.Max(agent.stoppingDistance, arriveRadius))
        {
            return false;
        }

        // 경로 O, 이동 중 → 도착 X 
        if (agent.hasPath && agent.velocity.sqrMagnitude > 0.05f * 0.05f)
        {
            return false;
        }
        return true;
    }


    // ====== 유틸 ======
    void ChangeState(State s)
    {
        state = s;
        if (action != null)
        {
            StopCoroutine(action);
            action = null;
        }
        // 개별 액션은 CatLoop에서 직접 호출/대기하므로 여기선 코루틴 시작 안 함.
    }

    bool HasCat() => cat.catState != null;

    void Play(AnimationClip clip, float fade, float speed, bool queue)
    {
        if (!clip)
        {
            return;
        }

        var st = anim[clip.name];
        st.speed = speed;
        float safe = Mathf.Min(fade, Mathf.Max(0.01f, clip.length * 0.4f));

        if (queue)
        {
            if (!anim.isPlaying)
            {
                anim.CrossFade(clip.name, safe);
            }
            else
            {
                anim.CrossFadeQueued(clip.name, safe, QueueMode.CompleteOthers);
            }
        }
        else
        {
            anim.CrossFade(clip.name, safe); 
        }
    }

    IEnumerator WaitClip(AnimationClip c)
    {
        if (!c) yield break;
        float len = Mathf.Max(0.05f, c.length);
        yield return new WaitForSeconds(len);
    }

    void ForceLoop(AnimationClip c) 
    {
        if (c)
        {
            anim[c.name].wrapMode = WrapMode.Loop;
        }
    }

    void ForceOnce(AnimationClip c)
    {
        if (c)
        {
            anim[c.name].wrapMode = WrapMode.Once;
        }
    }

    float toiletAnnounceCooldown = 0f;

    void LogOnce(ref float cooldown, float interval, string msg)
    {
        if (Time.time >= cooldown)
        {
            Debug.Log(msg);
            cooldown = Time.time + interval;
        }
    }
}