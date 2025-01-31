using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class Manager : MonoBehaviour
{
    public static Manager Instance { get; private set;}
    private FiniteStateMachine fsm;

    public Dictionary<Vector3Int, Tile> level = new Dictionary<Vector3Int, Tile>();

    [Header ("Prefabs")]
    public GameObject pathPrefab;
    public GameObject wallPrefab;
    public GameObject enemyPrefab;

    [Header("UI Menu's")]
    public GameObject startMenu;
    public GameObject buildingMenu;
    public GameObject gameOverMenu;
    public GameObject coinCounter;
    public GameObject buildTimer;

    public EnemyManager enemyManager = new EnemyManager(50);

    [Header("Level Settings")]
    [SerializeField] private string levelPath;
    [SerializeField] public float buildTime;
    private float buildTimeLeft;
    public int startCoins = 800;
    public int amountOfCoins;
    public float startHealth;
    [HideInInspector]
    public float health;
    public Transform mainCamera;


    [Header("Key Bindings")]
    //Customizable keybindings
    [SerializeField] private KeyCode buildKey = KeyCode.B;
    [SerializeField] private KeyCode undoKey = KeyCode.U;

    //Dependencies
    private LevelGenerator generator = new LevelGenerator();
    public BuildingManager buildingManager = new BuildingManager();
    private InputHandler inputHandler = new InputHandler();
    private KeyBinder keyBinder;

    //States
    private BaseState[] states = new BaseState[] { new StartState(), new BuildingState(), new AttackState(), new GameOverState()} ;

    private void Awake()
    {
        if(Instance != null && Instance != this) Destroy(this);
        else Instance = this;

        EventHandler.Subscribe(EventType.COINS_CHANGED, UpdateCoinCounter);
        buildingManager.OnAwake();
        keyBinder = new KeyBinder(buildingManager, inputHandler, buildKey, undoKey);
        amountOfCoins = startCoins;
        buildTimeLeft = buildTime;
    }

    private void Start()
    {
        health = startHealth;
        fsm = new FiniteStateMachine(typeof(StartState),states);

        level = generator.Generate(levelPath);
        SetCameraPosition();
        EventHandler.RaiseEvent(EventType.COINS_CHANGED, amountOfCoins);

        buildingManager.OnStart(generator.levelSize);
        enemyManager.OnStart();
    }

    private void Update()
    {
        fsm.OnUpdate();
        buildingManager.OnUpdate();
        inputHandler.HandleInput();
        enemyManager.OnUpdate();

        if (buildTimer.activeSelf)
            UpdateBuildTimer();
        else
            buildTimeLeft = buildTime;
    }

    private void OnDestroy()
    {
        EventHandler.Unsubscribe(EventType.COINS_CHANGED, UpdateCoinCounter);
        buildingManager.OnDestroy();
    }

    private void SetCameraPosition()
    {
        mainCamera.position = new Vector3(generator.levelSize.y / 2, mainCamera.position.y, generator.levelSize.x / 2 + 1);
    }

    public void UpdateCoinCounter(int _value)
    {
        amountOfCoins = _value;
        coinCounter.GetComponentInChildren<TMP_Text>().text = $"€{amountOfCoins},-";
    }

    public void UpdateBuildTimer()
    {
        buildTimeLeft -= Time.deltaTime;
        buildTimer.GetComponentInChildren<TMP_Text>().text = buildTimeLeft.ToString("F2");
    }

    public void StartButton()
    {
        fsm.SwitchState(typeof(BuildingState));
    }

    public void StartOverButton()
    {
        fsm.SwitchState(typeof(StartState));
    }
}
