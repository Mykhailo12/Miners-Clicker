using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Game : MonoBehaviour
{
    public Text scoreText;
    private int score;
    private int bonus = 1;
    private int workersCount, workersBonus = 1;
    [Header("Магазин")]
    public int[] shopCosts;
    public int[] shopBonuses;
    public float[] bonusTime;
    public Text[] shopBttnsText;
    public Button[] shopBttns;
    public GameObject shopPan;

    private void Start() {
        StartCoroutine(BonusPerSec());
    }

    private void Update() {
        scoreText.text = score + "$";
    }

    public void shopPan_ShowAndHide() {
        shopPan.SetActive(!shopPan.activeSelf);
    }

    public void shopBttn_addBonus(int index) {
        if (score >= shopCosts[index]) {
            bonus += shopBonuses[index];
            score -= shopCosts[index];
            shopCosts[index] *= 2;
            shopBttnsText[index].text = "BUY UPGRADE\n" + shopCosts[index] + "$";
        } else {
            Debug.Log("Не хватает денег");
        }
    }

    public void HireWorker(int index) {
        if (score >= shopCosts[index]) {
            workersCount++;
            score -= shopCosts[index];
        }
    }

    public void startBonusTimer(int index) {
        int cost = 2 * workersCount;
        shopBttnsText[2].text = "BUY BEAR FOR WORKERS\n" + cost + " $";
        if (score >= cost) {
            StartCoroutine(bonusTimer(bonusTime[index], index));
        }
    }

    IEnumerator BonusPerSec() {
        while(true) {
            score += (workersCount * workersBonus);
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator bonusTimer(float time, int index) {
        shopBttns[index].interactable = false;
        if (index == 0 && workersCount > 0) {
            workersBonus *= 2;
            yield return new WaitForSeconds(time);
            workersBonus /= 2;
        }
        shopBttns[index].interactable = true;
    }

    public void OnClick() {
        score += bonus;
    }
}
