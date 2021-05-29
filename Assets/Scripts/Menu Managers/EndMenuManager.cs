using System;
using DeliveryRush;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EndMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject endMenuUI;
    [SerializeField] private Text coinsDisplay;
    [SerializeField] private MapBuilder mapBuilder;

    private const string MoneyBank = "MoneyBank";

    private void Start()
    {
        endMenuUI.SetActive(false);
    }
    
    
    // https://proglib.io/p/sohranenie-igrovyh-dannyh-v-unity-2020-04-17
    
    public void Activate(float parcelPreservationStatus)
    {
        Time.timeScale = 0;
        endMenuUI.SetActive(true);

        var moneyMade = (int) (parcelPreservationStatus * mapBuilder.currentMapLength);

        coinsDisplay.text = moneyMade + " r.";
        
        PlayerPrefs.SetInt(MoneyBank, moneyMade);
    }

    public void Restart()
    {
        throw new NotImplementedException();
    }

    public void SaveAndQuit()
    {
        throw new NotImplementedException();
    }
}
