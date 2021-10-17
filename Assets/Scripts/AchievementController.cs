using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementController : MonoBehaviour
{
    //Intance ini mirip seperti pada GameManager, fungsinya adalah membuat sistem singleton
    //Untuk memudahkan pemanggilan script yang bersifat manager dari script lain

    private static AchievementController _instance = null;
    public static AchievementController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AchievementController>();
            }

            return _instance;
        }
    }

    [SerializeField] private Transform _popUpTransform;
    [SerializeField] private Text _popUpText;
    [SerializeField] private float _popUpShowDuration = 3f;
    [SerializeField] private List<AchievementData> _achievementList;

    private float _popUpShowDurationCounter;

    // Update is called once per frame
    void Update()
    {
        if (_popUpShowDurationCounter > 0)
        {
            //Kurangi durasi ketika pop up durasi lebih dari 0
            _popUpShowDurationCounter -= Time.unscaledDeltaTime;

            //Lerp adalah fungsi linear interpolation, digunakan untuk mengubah value secara perlahan.
            _popUpTransform.localScale = Vector3.LerpUnclamped(_popUpTransform.localScale, Vector3.one, 0.5f);
        }
        else
        {
            _popUpTransform.localScale = Vector3.LerpUnclamped(_popUpTransform.localScale, Vector3.right, 0.5f);
        }
    }

    public void UnlockAchivement(AchievementType type, string value, bool isString = true)
    {
        //Mencari data Achievement
        AchievementData achievement = null;
        if (isString)
        {
            achievement = _achievementList.Find(a => a.type == type && a.value == value);
        }
        else
        {
            achievement = _achievementList.Find(a => a.type == type && int.Parse(value) >= int.Parse(a.value));
        }
        
        if (achievement != null && !achievement.isUnlocked)
        {
            achievement.isUnlocked = true;
            ShowAchievementPopUp(achievement);
        }
    }

    private void ShowAchievementPopUp(AchievementData achievement)
    {
        _popUpText.text = achievement.title;
        _popUpShowDurationCounter = _popUpShowDuration;
        _popUpTransform.localScale = Vector2.right;
    }
}

//System.Serializable digunakan agar object dari script bisa di-serialize
//dan bisa di-inputkan dari inspector, jika tidak terdapat ini, maka variabel
//tidak akan muncul di inspector
[System.Serializable]
public class AchievementData
{
    public string title;
    public AchievementType type;
    public string value;
    public bool isUnlocked;
}

public enum AchievementType
{
    UnlockResource,
    TotalGold
}
