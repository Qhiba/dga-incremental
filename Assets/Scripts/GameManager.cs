using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance = null;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
            }
            return _instance;
        }
    }

    //Fungsi [Range (min, max)] ialah menjaga value agar tetap berada di antara min dan max-nya.
    [Range(0f, 1f)]
    public float autoCollectPercentage = 0.1f;
    public ResourceConfig[] resourceConfigs;
    public Sprite[] resourcesSprites;

    public Transform resourcesParent;
    public ResourceController resourcePrefab;
    public TapText tapTextPrefab;

    public Transform coinIcon;
    public Text goldInfo;
    public Text autoCollectInfo;

    private List<ResourceController> _activeResources = new List<ResourceController>();
    private List<TapText> _tapTextPool = new List<TapText>();
    private float _collectSecond;

    public double TotalGold { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        AddAllResources();
    }

    // Update is called once per frame
    void Update()
    {
        //Fungsi untuk selalu mengeksekusi CollectPerSecond setiap detik
        _collectSecond += Time.unscaledDeltaTime;
        if(_collectSecond >= 1f)
        {
            CollectPerSecond();
            _collectSecond = 0f;
        }

        CheckResourceCost();

        coinIcon.transform.localScale = Vector3.LerpUnclamped(coinIcon.transform.localScale, Vector3.one * 2f, 0.15f);
        coinIcon.transform.Rotate(0f, 0f, Time.deltaTime * -100f);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void AddAllResources()
    {
        bool showResources = true;
        foreach (ResourceConfig config in resourceConfigs)
        {
            GameObject obj = Instantiate(resourcePrefab.gameObject, resourcesParent, false);
            ResourceController resource = obj.GetComponent<ResourceController>();

            resource.SetConfig(config);
            obj.gameObject.SetActive(showResources);

            if(showResources && !resource.isUnlocked)
            {
                showResources = false;
            }

            _activeResources.Add(resource);
        }
    }

    public void ShowNextResource()
    {
        foreach(ResourceController resource in _activeResources)
        {
            if (!resource.gameObject.activeSelf)
            {
                resource.gameObject.SetActive(true);
                break;
            }
        }
    }

    private void CheckResourceCost()
    {
        foreach (ResourceController resource in _activeResources)
        {
            bool isBuyable = false;
            if (resource.isUnlocked)
            {
                isBuyable = TotalGold >= resource.GetUpgradeCost();
            }
            else
            {
                isBuyable = TotalGold >= resource.GetUnlockCost();
            }   

            resource.resourceImage.sprite = resourcesSprites[isBuyable ? 1 : 0];
        }
    }

    private void CollectPerSecond()
    {
        double output = 0;
        foreach (ResourceController resource in _activeResources)
        {
            if (resource.isUnlocked)
            {
                output += resource.GetOutput();
            }
        }

        output *= autoCollectPercentage;

        //Fungsi ToString("F1") ialah membulatkan angka menjadi desimal yang memiliki 1 angka di belakang koma
        autoCollectInfo.text = $"Auto Collect: {output.ToString("F1")} / second";

        AddGold(output);
    }

    public void AddGold(double value)
    {
        TotalGold += value;
        goldInfo.text = $"Gold: {TotalGold.ToString("0")}";

        AchievementController.Instance.UnlockAchivement(AchievementType.TotalGold, TotalGold.ToString("0"), false);
    }

    public void CollectByTap(Vector3 tapPosition, Transform parent)
    {
        double output = 0;
        foreach (ResourceController resource in _activeResources)
        {
            if (resource.isUnlocked)
            {
                output += resource.GetOutput();
            }
        }

        TapText tapText = GetOrCreateTapText();
        tapText.transform.SetParent(parent, false);
        tapText.transform.position = tapPosition;

        tapText.text.text = $"+{output.ToString("0")}";
        tapText.gameObject.SetActive(true);
        coinIcon.transform.localScale = Vector3.one * 1.75f;

        AddGold(output);
    }

    private TapText GetOrCreateTapText()
    {
        TapText tapText = _tapTextPool.Find(t => !t.gameObject.activeSelf);
        if(tapText == null)
        {
            tapText = Instantiate(tapTextPrefab).GetComponent<TapText>();
            _tapTextPool.Add(tapText);
        }

        return tapText;
    }
}

//Fungsi System.Serializeable adalah agar object bisa diserialize dan value dapat di-set dari inspector
[System.Serializable]
public struct ResourceConfig
{
    public string name;
    public double unlockCost;
    public double upgradeCost;
    public double output;
}
