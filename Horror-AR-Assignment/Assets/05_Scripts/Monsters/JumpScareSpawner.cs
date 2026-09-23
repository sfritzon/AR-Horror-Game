using System.Collections;
using UnityEngine;

public class JumpScareSpawner : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private bool spawnOnStartGame = true;

    [Header("Timing")]
    [SerializeField] private float firstScareDelay = 10f;
    [SerializeField] private float minSecondsBetweenScares = 15f;
    [SerializeField] private float maxSecondsBetweenScares = 25f;

    [Header("Jump Behaviour")]
    [SerializeField] private float spawnDistance = 2.5f;
    [SerializeField] private float riseHeight = 1.2f; 
    [SerializeField] private float riseDuration = 0.4f; 
    [SerializeField] private float lungeDistance = 0.6f;
    [SerializeField] private float lungeDuration = 0.35f;
    [SerializeField] private float lingerTime = 0.9f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    private bool running;

    void Start()
    {
        if (spawnOnStartGame && GameManager.Instance != null)
            GameManager.Instance.OnGameStarted += StartSpawning;
        else if (!spawnOnStartGame)
            StartSpawning();
    }

    public void StartSpawning()
    {
        if (!running) StartCoroutine(ScareLoop());
    }

    public void StopSpawning() => running = false;

    private IEnumerator ScareLoop()
    {
        running = true;
        yield return new WaitForSeconds(firstScareDelay);

        while (running)
        {
            DoJumpScare();
            yield return new WaitForSeconds(Random.Range(minSecondsBetweenScares, maxSecondsBetweenScares));
        }
    }

private void DoJumpScare()
{
    Camera cam = Camera.main;
    
    if (cam == null || monsterPrefab == null) return;

    Vector3 camPos = cam.transform.position;
    Vector3 lookDir = cam.transform.forward.normalized;
    Vector3 flatFwd = lookDir; flatFwd.y = 0f;
    if (flatFwd.sqrMagnitude < 0.001f) flatFwd = Vector3.forward;
    flatFwd.Normalize();

    Vector3 spawnPos = camPos + flatFwd * spawnDistance;
    spawnPos.y -= riseHeight;

    GameObject monster = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
    monster.transform.LookAt(camPos);

    StartCoroutine(AnimateScare(monster, cam, lookDir));
}

private IEnumerator AnimateScare(GameObject monster, Camera cam, Vector3 lookDir)
{
    Vector3 startPos = monster.transform.position;
    
    Vector3 riseTarget = cam.transform.position + lookDir * spawnDistance;
    
    Vector3 faceTarget = cam.transform.position + lookDir * lungeDistance;

    // Phase 1: rise
    float t = 0f;
    while (t < riseDuration)
    {
        t += Time.deltaTime;
        monster.transform.position = Vector3.Lerp(startPos, riseTarget, t / riseDuration);
        yield return null;
    }
    
    Vector3 lungeStart = monster.transform.position;
    t = 0f;
    while (t < lungeDuration)
    {
        t += Time.deltaTime;
        monster.transform.position = Vector3.Lerp(lungeStart, faceTarget, t / lungeDuration);
        monster.transform.LookAt(cam.transform.position);
        yield return null;
    }
    
    yield return new WaitForSeconds(lingerTime);

    t = 0f;
    Vector3 sinkPos = faceTarget - new Vector3(0f, riseHeight, 0f);
    Vector3 startScale = monster.transform.localScale;
    while (t < fadeOutDuration)
    {
        t += Time.deltaTime;
        float k = t / fadeOutDuration;
        monster.transform.position = Vector3.Lerp(faceTarget, sinkPos, k);
        monster.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, k);
        yield return null;
    }

    Destroy(monster);
}

    private Vector3 FlatForward(Camera cam)
    {
        Vector3 f = cam.transform.forward; f.y = 0f;
        return f.sqrMagnitude < 0.001f ? Vector3.forward : f.normalized;
    }
}