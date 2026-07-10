using UnityEngine;
using System.Collections.Generic;

public class Cauldron : MonoBehaviour
{
    [SerializeField] private RecipeSystem recipeSystem;
    
    private RecipeSystem.PotionOrder currentOrder;
    [SerializeField] private List<string> ingredientsInside = new List<string>();
    private IngredientSpawner ingredientSpawner;

    void Start()
    {
        ingredientSpawner = GameObject.Find("Ingredient Spawner").GetComponent<IngredientSpawner>();

        if (recipeSystem == null) 
        {
            recipeSystem = GetComponent<RecipeSystem>();
        }
        
        RequestNewOrder();
    }

    void RequestNewOrder()
    {
        currentOrder = recipeSystem.GenerateRandomOrder();
        Debug.Log($"[NEW ORDER]: {currentOrder.potionName}");
        Debug.Log($"Required ingredients: {string.Join(", ", currentOrder.requiredIngredients)}");
    }
    private Ingredient ingredient;

    void OnCollisionEnter(Collision collision)
    {
        ingredient = collision.gameObject.GetComponent<Ingredient>();

        if (ingredient != null)
        {
            ingredientsInside.Add(ingredient.ingredientName);
            
            AudioManager.Instance.PlaySFXCauldron(); 
            ingredient.transform.SetParent(null);
            ingredient.ConsumedIngredient();

            if (ingredientsInside.Count >= 3)
            {
                EvaluatePotion();
            }
        }
    }

    void EvaluatePotion()
    {
        bool hasSameIngredients = recipeSystem.AreSameIngredients(ingredientsInside, currentOrder.requiredIngredients);

        if (hasSameIngredients == true)
        {
            Debug.Log("Success!");
            AudioManager.Instance.PlaySFXPotionSuccess();
            //AQUI SE AGREGA +1 AL CONTADOR DE LA UI

        } else
        {
            Debug.Log("Failure.");
            AudioManager.Instance.PlaySFXPotionFailure();
        }

        ingredientsInside.Clear();
        RequestNewOrder();
        ingredientSpawner.RespawnEverythingInNewPositions();
    }
}
