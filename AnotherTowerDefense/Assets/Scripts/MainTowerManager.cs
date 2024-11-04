using UnityEngine;

public class MainTowerManager : MonoBehaviour{
    public static MainTowerManager Instance { get; private set; }  // Singleton reference
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    private void Awake() {
        // Set this instance as the singleton instance
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject); // Prevent multiple instances
        }
        currentHealth = maxHealth;
    }
    public void TakeDamage(int damageAmount){
        currentHealth -= damageAmount;
        Debug.Log("Tower took damage. Current health: " + currentHealth);
        if (currentHealth <= 0){
            DestroyTower();
        }
    }
    public void HealTower(int healAmount){
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        Debug.Log("Tower healed. Current health: " + currentHealth);
    }
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    private void DestroyTower(){
        Debug.Log("Tower destroyed!");
        // Insert logic here for game over
        Destroy(gameObject);
    }
}