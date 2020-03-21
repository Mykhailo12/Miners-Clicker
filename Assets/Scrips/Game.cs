using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System;

public class Game : MonoBehaviour
{
    [Header("Текст, отвечающий за отображение денег")]
    public Text scoreText;
    [Header("Магазин")]
    public List<Item> shopItems = new List<Item>();
    [Header("Текст на кнопках товаров")]
    public Text[] shopItemsText;
    [Header("Кнопки товаров")]
    public Button[] shopBttns;
    [Header("Панелька магазина")]
    public GameObject shopPan;

    private int score; //Игровая валюта
    private int scoreIncrease = 1; //Бонус при клике
    private Save sv = new Save();

    private void Awake()
    {
        if (PlayerPrefs.HasKey("SV"))
        {
            int totalBonusPS = 0;
            sv = JsonUtility.FromJson<Save>(PlayerPrefs.GetString("SV"));
            score = sv.score;
            for (int i = 0; i < shopItems.Count; i++)
            {
                shopItems[i].levelOfItem = sv.levelOfItem[i];
                shopItems[i].bonusCounter = sv.bonusCounter[i];
                if (shopItems[i].needCostMultiplier) shopItems[i].cost *= (int)Mathf.Pow(shopItems[i].costMultiplier, shopItems[i].levelOfItem);
                if (shopItems[i].bonusIncrease != 0 && shopItems[i].levelOfItem != 0) scoreIncrease += (int)Mathf.Pow(shopItems[i].bonusIncrease, shopItems[i].levelOfItem);
                totalBonusPS += shopItems[i].bonusPerSec * shopItems[i].bonusCounter;
            }
            DateTime dt = new DateTime(sv.date[0], sv.date[1], sv.date[2], sv.date[3], sv.date[4], sv.date[5]);
            TimeSpan ts = DateTime.Now - dt;
            int offlineBonus = (int)ts.TotalSeconds * totalBonusPS;
            score += offlineBonus;
            print("Вы отсутствовали: " + ts.Days + "Д. " + ts.Hours + "Ч. " + ts.Minutes + "М. " + ts.Seconds + "S.");
            print("Ваши рабочие заработали: " + offlineBonus + "$");
        }
    }

    private void Start()
    {
        updateCosts(); //Обновить текст с ценами
        StartCoroutine(BonusPerSec()); //Запустить просчёт бонуса в секунду
    }

    private void Update()
    {
        scoreText.text = score + "$"; //Отображаем деньги
    }

    public void BuyBttn(int index) //Метод при нажатии на кнопку покупки товара (индекс товара)
    {
        int cost = shopItems[index].cost * shopItems[shopItems[index].itemIndex].bonusCounter; //Рассчитываем цену в зависимости от кол-ва рабочих (к примеру)
        if (shopItems[index].itsBonus && score >= cost) //Если товар нажатой кнопки - это бонус, и денег >= цены(е)
        {
            if (cost > 0) // Если цена больше чем 0, то:
            {
                score -= cost; // Вычитаем цену из денег
                StartCoroutine(BonusTimer(shopItems[index].timeOfBonus, index)); //Запускаем бонусный таймер
            }
            else print("Нечего улучшать то!"); // Иначе выводим в консоль текст.
        }
        else if (score >= shopItems[index].cost) // Иначе, если товар нажатой кнопки - это не бонус, и денег >= цена
        {
            if (shopItems[index].itsItemPerSec) shopItems[index].bonusCounter++; // Если нанимаем рабочего (к примеру), то прибавляем кол-во рабочих
            else scoreIncrease += shopItems[index].bonusIncrease; // Иначе бонусу при клике добавляем бонус товара
            score -= shopItems[index].cost; // Вычитаем цену из денег
            if (shopItems[index].needCostMultiplier) shopItems[index].cost *= shopItems[index].costMultiplier; // Если товару нужно умножить цену, то умножаем на множитель
            shopItems[index].levelOfItem++; // Поднимаем уровень предмета на 1
        }
        else print("Не хватает денег!"); // Иначе если 2 проверки равны false, то выводим в консоль текст.
        updateCosts(); //Обновить текст с ценами
    }
    private void updateCosts() // Метод для обновления текста с ценами
    {
        for (int i = 0; i < shopItems.Count; i++) // Цикл выполняется, пока переменная i < кол-ва товаров
        {
            if (shopItems[i].itsBonus) // Если товар является бонусом, то:
            {
                int cost = shopItems[i].cost * shopItems[shopItems[i].itemIndex].bonusCounter; // Рассчитываем цену в зависимости от кол-ва рабочих (к примеру)
                shopItemsText[i].text = shopItems[i].name + "\n" + cost + "$"; // Обновляем текст кнопки с рассчитанной ценой
            }
            else shopItemsText[i].text = shopItems[i].name + "\n" + shopItems[i].cost + "$"; // Иначе если товар не является бонусом, то обновляем текст кнопки с обычной ценой
        }
    }

    IEnumerator BonusPerSec() // Бонус в секунду
    {
        while (true) // Бесконечный цикл
        {
            for (int i = 0; i < shopItems.Count; i++) score += (shopItems[i].bonusCounter * shopItems[i].bonusPerSec); // Добавляем к игровой валюте бонус рабочих (к примеру)
            yield return new WaitForSeconds(1); // Делаем задержку в 1 секунду
        }
    }
    IEnumerator BonusTimer(float time, int index) // Бонусный таймер (длительность бонуса (в сек.),индекс товара)
    {
        shopBttns[index].interactable = false; // Выключаем кликабельность кнопки бонуса
        shopItems[shopItems[index].itemIndex].bonusPerSec *= 2; // Удваиваем бонус рабочих в секунду (к примеру)
        yield return new WaitForSeconds(time); // Делаем задержку на столько секунд, сколько указали в параметре
        shopItems[shopItems[index].itemIndex].bonusPerSec /= 2; // Возвращаем бонус в нормальное состояние
        shopBttns[index].interactable = true; // Включаем кликабельность кнопки бонуса
    }
#if UNITY_ANDROID && !UNITY_EDITOR
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            sv.score = score;
            sv.levelOfItem = new int[shopItems.Count];
            sv.bonusCounter = new int[shopItems.Count];
            for (int i = 0; i < shopItems.Count; i++)
            {
                sv.levelOfItem[i] = shopItems[i].levelOfItem;
                sv.bonusCounter[i] = shopItems[i].bonusCounter;
            }
            sv.date[0] = DateTime.Now.Year; sv.date[1] = DateTime.Now.Month; sv.date[2] = DateTime.Now.Day; sv.date[3] = DateTime.Now.Hour; sv.date[4] = DateTime.Now.Minute; sv.date[5] = DateTime.Now.Second;
            PlayerPrefs.SetString("SV", JsonUtility.ToJson(sv));
        }
    }
#else
    private void OnApplicationQuit()
    {
        sv.score = score;
        sv.levelOfItem = new int[shopItems.Count];
        sv.bonusCounter = new int[shopItems.Count];
        for (int i = 0; i < shopItems.Count; i++)
        {
            sv.levelOfItem[i] = shopItems[i].levelOfItem;
            sv.bonusCounter[i] = shopItems[i].bonusCounter;
        }
        sv.date[0] = DateTime.Now.Year; sv.date[1] = DateTime.Now.Month; sv.date[2] = DateTime.Now.Day; sv.date[3] = DateTime.Now.Hour; sv.date[4] = DateTime.Now.Minute; sv.date[5] = DateTime.Now.Second;
        PlayerPrefs.SetString("SV", JsonUtility.ToJson(sv));
    }
#endif

    public void showShopPan() // Рассказывал в уроке #2
    {
        shopPan.SetActive(!shopPan.activeSelf); // Рассказывал в уроке #2
    }

    public void OnClick() // Рассказывал в уроке #1
    {
        score += scoreIncrease; // К игровой валюте прибавляем бонус при клике
    }

}
[Serializable]
public class Item // Класс товара
{
    [Tooltip("Название используется на кнопках")]
    public string name;
    [Tooltip("Цена товара")]
    public int cost;
    [Tooltip("Бонус, который добавляется к бонусу при клике")]
    public int bonusIncrease;
    [HideInInspector]
    public int levelOfItem; // Уровень товара
    [Space]
    [Tooltip("Нужен ли множитель для цены?")]
    public bool needCostMultiplier;
    [Tooltip("Множитель для цены")]
    public int costMultiplier;
    [Space]
    [Tooltip("Этот товар даёт бонус в секунду?")]
    public bool itsItemPerSec;
    [Tooltip("Бонус, который даётся в секунду")]
    public int bonusPerSec;
    [HideInInspector]
    public int bonusCounter; // Кол-во рабочих (к примеру)
    [Space]
    [Tooltip("Это временный бонус?")]
    public bool itsBonus;
    [Tooltip("Множитель товара, который управляется бонусом (Умножается переменная bonusPerSec)")]
    public int itemMultiplier;
    [Tooltip("Индекс товара, который будет управляться бонусом (Умножается переменная bonusPerSec этого товара)")]
    public int itemIndex;
    [Tooltip("Длительность бонуса")]
    public float timeOfBonus;
}
[Serializable]
public class Save
{
    public int score;
    public int[] levelOfItem;
    public int[] bonusCounter;
    public int[] date = new int[6];
}