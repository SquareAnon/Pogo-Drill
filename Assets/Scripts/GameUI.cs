using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ScoreAddTYpe { gold, depth, blocks, diamond, returned};
public class GameUI : MonoBehaviour
{
    public AnimationCurve UIScaleCurveX, UIScaleCurveY;
    public float animDuration = .3f;
    public float floatBarUpdateDelay = .3f;
    public float floatBarSpeed = 60;
    public static GameUI _;
    public TextMeshProUGUI hiScore;
   

    [Header("Health")]
    public RectTransform hp;
    public Image hpBar, hpFloatBar;
    public float hpFloatValue;
    public float lastHPUpdateTime;
    public float hpUIScaleLerp = 1;

    [Header("Fuel")]
    public RectTransform fuel;
    public Image fuelBar, fuelFloatBar;
    public float fuelFloatValue;
    public float lastFuelUpdateTime;
    public float fuelUIScaleLerp = 1;

    [Header("Gold")]
    public RectTransform gold;
    public TextMeshProUGUI goldTotal, goldAdded;
    public float lastGoldUpdateTime;
    public float goldUIScaleLerp = 1;
    public AnimationCurve goldAddScaleCurve;
    public float goldAddScaleLerp = 1;
    int totalGold;
    float currentGold;

    [Header("Air")]
    public RectTransform air;
    public Image airBar;
    public RectTransform airWarning;
    public AnimationCurve airWarningCurve;
    bool danger;
    float airWarningScaleLerp;

    [Header("Jetpack")]
    public RectTransform jp;
    public Image jetpackBar;
    public Image jetpackFloatBar;
    public float jetpackFloatValue;
    public float lastJetpackUpdateTime;
    public float jetpackUIScaleLerp = 1;

    [Header("Depth")]
    public TextMeshProUGUI depthText;

    [Header("General")]
    public Animator uiAnimator;
    public GameObject menuCam;

    [Header("Game End")]
    public TextMeshProUGUI gameOverText;
    public RectTransform scoreT;
    public RectTransform scoreAddT;
    public TextMeshProUGUI finalGoldText, finalDepthText, blocksDrilledText, returnedText, diamondText, finalScoreText, scoreAddText;
    float scoreLerp;
    float scoreAddLerp;
    public AnimationCurve scoreAddCurve;
    public int score;

    public bool restart;



    public void StartGame()
    {
        uiAnimator.SetInteger("UI STATE", 1);
    }

    public void QuitGame()
    {
        uiAnimator.SetInteger("UI STATE", 2);
    }

    public void Start_Game()
    {
        Time.timeScale = 1;
    }

    public void SetGameOverText()
    {
        gameOverText.text = "P\nO\nGAME\nOVER!";
    }

    public void UpdateScore(ScoreAddTYpe t)
    {
        int scoreAdd= 0; 
        switch (t)
        {
            case ScoreAddTYpe.gold:
               scoreAdd = Pogo._.gold * 100;
                scoreAddText.text = "+" + scoreAdd;
                score += scoreAdd;
                break;
            case ScoreAddTYpe.depth:
                scoreAdd = (int)(Mathf.Abs( Pogo._.maxDepth) * 10);
                score += scoreAdd;
                scoreAddText.text = "+" + scoreAdd;
                break;
            case ScoreAddTYpe.blocks:
                scoreAdd = Cave._.blocksDrilled * 5;
                score += scoreAdd;
                scoreAddText.text = "+" + scoreAdd;
                break;
            case ScoreAddTYpe.diamond:
                scoreAdd = Pogo._.gotDiamond ? 2000 : 0;
                score += scoreAdd;
                scoreAddText.text = "+" + scoreAdd;
                break;
            case ScoreAddTYpe.returned:
                scoreAdd = Pogo._.returned ? 1 : 0;
                score *= Pogo._.returned ? 2 : 1;
                scoreAddText.text = "x2";
                break;
            default:
                break;
        }
       
        
        if(scoreAdd > 0 )
        {
            SoundEffectManager._.CreateSound("Ping");
            scoreLerp = 0;
            scoreAddLerp = 0;
        }
       
        finalScoreText.text = "Score: " + score;
        PlayerPrefs.SetInt("high score", score);
    }


    public void EndGame(bool r)
    {
        restart = r;
        uiAnimator.SetInteger("UI STATE", 4);
    }

    public void PlaySound(string soundName)
    {
        SoundEffectManager._.CreateSound(soundName);
    }

    public void End_Game()
    {
        if (restart) UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        else Application.Quit();
    }
    public void MenuCamOff()
    {
        menuCam.SetActive(false);
    }

    public void ShowGameOverScreen()
    {
        finalGoldText.text = "$" + Pogo._.gold;
        finalDepthText.text = "Max Depth: " + Pogo._.maxDepth.ToString("F2");
        blocksDrilledText.text = "Drilled Blocks: " + Cave._.blocksDrilled.ToString();
        uiAnimator.SetInteger("UI STATE", 3);
        diamondText.text = Pogo._.gotDiamond ? "Found the diamond!" : "No diamond";
        returnedText.text = Pogo._.returned ? "Made it back up!" : "Missing In Action";


    }

    public void Quit_Game()
    {
        Application.Quit();
    }
    private void Awake()
    {
        Time.timeScale = 0;
        _ = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        Pogo._.OnHPChange += UpdateHPBar;
        Pogo._.OnFuelChange += UpdateFuelBar;
        Pogo._.OnGoldChange += UpdateGold;
        Pogo._.OnAirChange += UpdateAir;
        Pogo._.OnJetpackChange += UpdateJetpack;
        Pogo._.OnDepthChange += UpdateDepth;
        Pogo._.OnGameOver += ShowGameOverScreen;
        hiScore.text = "Hi-Score: " + (PlayerPrefs.HasKey("high score") ? PlayerPrefs.GetInt("high score") : 0);
    }

    public void ScaleUI(ref RectTransform t, float lerp)
    {
        t.localScale = new Vector3(UIScaleCurveX.Evaluate(lerp), UIScaleCurveY.Evaluate(lerp), 1);
    }
    // Update is called once per frame
    void Update()
    {
       
        float dt = Time.deltaTime;
        if (Time.time >= lastHPUpdateTime + floatBarUpdateDelay)
        hpFloatBar.fillAmount = Mathf.MoveTowards(hpFloatBar.fillAmount, hpFloatValue, dt * floatBarSpeed);
        //if (Pogo._.hp <= 0) return;
        if (Time.time >= lastFuelUpdateTime + floatBarUpdateDelay)
            fuelFloatBar.fillAmount = Mathf.MoveTowards(fuelFloatBar.fillAmount, fuelFloatValue, dt * floatBarSpeed);
        if (Time.time >= lastJetpackUpdateTime + floatBarUpdateDelay)
            jetpackFloatBar.fillAmount = Mathf.MoveTowards(jetpackFloatBar.fillAmount, jetpackFloatValue, dt * floatBarSpeed);

        if (Time.time >= lastGoldUpdateTime + floatBarUpdateDelay)
        {
            currentGold = Mathf.Lerp((float)currentGold, (float)totalGold, goldAddScaleLerp/10);
            goldTotal.text = "$ " + Mathf.RoundToInt(currentGold).ToString();
        }
        goldAdded.transform.localScale = Vector3.one * goldAddScaleCurve.Evaluate(goldAddScaleLerp);

        airWarning.gameObject.SetActive( danger);
       
        ScaleUI(ref air, airWarningScaleLerp);

        ScaleUI(ref hp, hpUIScaleLerp);
        ScaleUI(ref fuel, fuelUIScaleLerp);
        ScaleUI(ref gold, goldUIScaleLerp);
        ScaleUI(ref jp, jetpackUIScaleLerp);

        ScaleUI(ref scoreT, scoreLerp);
        ScaleUI(ref scoreAddT, scoreAddLerp);
        ScoreAdd(scoreAddLerp);



        hpUIScaleLerp += dt/animDuration;
        fuelUIScaleLerp += dt / animDuration;
        goldUIScaleLerp += dt / animDuration;
        goldAddScaleLerp += dt / animDuration;
        jetpackUIScaleLerp += dt / animDuration;
        scoreLerp += dt / animDuration;
        scoreAddLerp += dt / (animDuration *3);
        if (danger)
        {
            airWarningScaleLerp += dt / animDuration/2;
            if (airWarningScaleLerp >= 1f) airWarningScaleLerp = -2f;
        }

    }

    void ScoreAdd(float lerp)
    {
        Color c = scoreAddText.color;
        c.a = scoreAddCurve.Evaluate( lerp);
        scoreAddText.color = c;
    }

    public void UpdateHPBar(int hp, int max)
    {
        hpBar.fillAmount = ((float)hp / (float)max);
        hpFloatValue = ((float)hp / (float)max);
        lastHPUpdateTime = Time.time;
        hpUIScaleLerp = 0;

    }

    public void UpdateFuelBar(float fu, float max)
    {
        fuelBar.fillAmount = (fu / max);
        fuelFloatValue = (fu / max);
        lastFuelUpdateTime = Time.time;
        fuelUIScaleLerp = 0;
    }

    public void UpdateGold(int go, int add)
    {
        goldAddScaleLerp = 0;
        goldAdded.text = "+" + add;
        currentGold = totalGold;
        totalGold = go;
        lastGoldUpdateTime = Time.time;

        goldUIScaleLerp = 0;
    }

    public void UpdateAir(float ai, float max)
    {
        airBar.fillAmount = ai / max;
        danger = (ai / max <= .2f);
    }

    public void UpdateJetpack(float jp, float max)
    {
        jetpackBar.fillAmount = jp / max;
        jetpackFloatValue = jp / max;
        jetpackUIScaleLerp = 0;
        lastJetpackUpdateTime = Time.time;


    }

    public void UpdateDepth(float dep)
    {
        depthText.text = dep.ToString("N2");
    }

}
