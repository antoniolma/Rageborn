using System;
using UnityEngine;

public class PlayerCurrency : MonoBehaviour
{
    public static PlayerCurrency Instance { get; private set; }

    public static Action<int> OnCoinsChanged;

    [SerializeField] private int startingCoins = 0;
    private int coins;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // opcional: manter moeda persistente entre cenas
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        coins = startingCoins;
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        OnCoinsChanged?.Invoke(coins);
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0) return false;
        if (coins < amount) return false;
        coins -= amount;
        OnCoinsChanged?.Invoke(coins);
        return true;
    }

    public int GetCoins() => coins;
}
