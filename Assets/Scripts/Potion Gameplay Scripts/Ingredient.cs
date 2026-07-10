using UnityEngine;

public class Ingredient : MonoBehaviour
{
    public string ingredientName;
    public Transform spawnPosition; // Se asigna por el IngredientSpawner
    private IngredientSpawner ingredientSpawner;

    void Start()
    {
        ingredientSpawner = GameObject.Find("Ingredient Spawner").GetComponent<IngredientSpawner>();
    }

    //Método ConsumedIngredient: Indica la posición del objeto y lo destrulle
    public void ConsumedIngredient()
    {
        if (ingredientSpawner != null)
        {
            ingredientSpawner.ReportIngredientDestroyed(spawnPosition);
        }
        else
        {
            Debug.LogWarning($"{ingredientName}: Null IngredientSpawner");
        }

        Destroy(gameObject);
    }
}