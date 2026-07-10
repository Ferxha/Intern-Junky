using UnityEngine;
using System.Collections.Generic;

public class Cauldron : MonoBehaviour
{
    // Valida los ingredientes que entran al caldero y los compara con la orden actual.
    [SerializeField] private GameObject cauldronSmokeFXPrefab;
    [SerializeField] private RecipeSystem recipeSystem;
    [SerializeField] private List<string> ingredientsInside = new List<string>();
    [SerializeField] private int ordersTotal;

    private RecipeSystem.PotionOrder currentOrder;
    private IngredientSpawner ingredientSpawner;
    private Ingredient ingredient;

    void Start()
    {
        GameObject ingredientSpawnerObject = GameObject.Find("Ingredient Spawner");
        if (ingredientSpawnerObject != null)
        {
            ingredientSpawner = ingredientSpawnerObject.GetComponent<IngredientSpawner>();
        }

        if (recipeSystem == null)
        {
            recipeSystem = FindFirstObjectByType<RecipeSystem>();
        }

        if (recipeSystem == null)
        {
            Debug.LogError("RecipeSystem no encontrado para Cauldron.");
            enabled = false;
            return;
        }

        RequestNewOrder();
    }

    void RequestNewOrder()
    {
        currentOrder = recipeSystem.GenerateRandomOrder();
        Debug.Log($"[NEW ORDER]: {currentOrder.potionName}");
        Debug.Log($"Required ingredients: {string.Join(", ", currentOrder.requiredIngredients)}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentOrder(currentOrder.potionName);
            GameManager.Instance.PresentCurrentOrder();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        ingredient = collision.gameObject.GetComponent<Ingredient>();

        if (ingredient != null)
        {
            ingredientsInside.Add(ingredient.ingredientName);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFXCauldron();
            }

            if (cauldronSmokeFXPrefab != null)
            {
                Vector3 spawnPosition = collision.contactCount > 0
                    ? collision.GetContact(0).point
                    : collision.transform.position;

                Instantiate(cauldronSmokeFXPrefab, spawnPosition, cauldronSmokeFXPrefab.transform.rotation);
            }

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
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFXPotionSuccess();
            }

            if (GameManager.Instance != null && !GameManager.Instance.CompleteCurrentOrder())
            {
                ingredientsInside.Clear();
                return;
            }
         
        }
        else
        {
            Debug.Log("Failure.");
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFXPotionFailure();
            }

            ingredientsInside.Clear();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOverWrongRecipe();
            }

            return;
        }

        ingredientsInside.Clear();
        if (ingredientSpawner != null)
        {
            ingredientSpawner.RespawnEverythingInNewPositions();
        }

        RequestNewOrder();
    }
}
