using UnityEngine;

public class Ingredient : MonoBehaviour
{
    public string ingredientName;
    
    public Transform spawnPosition; // Se asigna por el IngredientSpawner
    private IngredientSpawner ingredientSpawner;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ingredientSpawner = GameObject.Find("Ingredient Spawner").GetComponent<IngredientSpawner>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void ConsumedIngredient()
    {
        if (ingredientSpawner != null)
        {
            ingredientSpawner.ReportIngredientDestroyed(spawnPosition);
        }
        else
        {
            Debug.LogWarning($"{ingredientName}: ingredientSpawner es null, no se puede reportar destrucción.");
        }

        Destroy(gameObject);
    }
}