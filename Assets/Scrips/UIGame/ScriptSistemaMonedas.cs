using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoinManager : MonoBehaviour
{
    public TextMeshProUGUI coinText; 
    private int coins;
    private int currentLevelCoins;

    void Start()
    {
       
        coins = PlayerPrefs.GetInt("Coins", 0);
        currentLevelCoins = 0;
        UpdateCoinText();
    }

    public void AddCoins(int amount)
    {
        
        coins += amount;
        currentLevelCoins += amount; 
        UpdateCoinText();
    }

    public void PlayerDied()
    {
        
        coins -= currentLevelCoins;
        currentLevelCoins = 0; 
        UpdateCoinText();
    }

    public void SetCoins(int amount)
    {
        
        coins = amount;
        UpdateCoinText();
    }

    void UpdateCoinText()
    {
        
        coinText.text = coins.ToString();
    }

    void OnDestroy()
    {
        
        PlayerPrefs.SetInt("Coins", coins);
    }
    private void OnEnable()
    {
        Eventos.eve.coinsCount.AddListener(AddCoins);
        Eventos.eve.changeCoinCount.AddListener(SetCoins);
        Eventos.eve.resetCoinsInlvlDied.AddListener(PlayerDied);
    }
    private void OnDisable()
    {
        Eventos.eve.coinsCount.RemoveListener(AddCoins);
        Eventos.eve.changeCoinCount.RemoveListener(SetCoins);
        Eventos.eve.resetCoinsInlvlDied.RemoveListener(PlayerDied);
    }
}
