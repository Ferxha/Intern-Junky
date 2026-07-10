using UnityEngine;
using System.Collections.Generic;
using System;

public class Cauldron : MonoBehaviour
{
    //Script del caldero: Valida que ingredientes han entrado al caldero y compara con la orden del momento 
    [SerializeField] private GameObject cauldronSmokeFXPrefab;
    [SerializeField] private RecipeSystem recipeSystem;
    [SerializeField] private List<string> ingredientsInside = new List<string>();
    private RecipeSystem.PotionOrder currentOrder;
    private IngredientSpawner ingredientSpawner;
    //AQUI SE AGREGARIA LA LOGICA DE LA DIFICULTAD DE NIVEL Y QUE CUMPLA CON X CANTIDAD DE PEDIDOS ANTES DE QUE EL TIEMPO ACABE
    //O EN UN GAME MANAGER MANDAR A LLAMAR SOLO LA VARIABLE DE orderCount
    [SerializeField] private int ordersTotal; //Afectado por la dificultad, está variable se va a localizar donde se asigne el método donde compare contidad de órdenes con las pendientes y el tiempo
    private int orderCount = 0;

    void Start()
    {
        ingredientSpawner = GameObject.Find("Ingredient Spawner").GetComponent<IngredientSpawner>();

        if (recipeSystem == null) 
        {
            recipeSystem = GetComponent<RecipeSystem>();
        }
        
        RequestNewOrder();
    }

    //Solicita nueva orden
    void RequestNewOrder()
    {
        currentOrder = recipeSystem.GenerateRandomOrder();
        Debug.Log($"[NEW ORDER]: {currentOrder.potionName}");
        Debug.Log($"Required ingredients: {string.Join(", ", currentOrder.requiredIngredients)}");
    }
    private Ingredient ingredient;

    //Cuando un objeto colisiona con el caldero actualiza la lista y desaparece
    void OnCollisionEnter(Collision collision)
    {
        ingredient = collision.gameObject.GetComponent<Ingredient>();

        if (ingredient != null)
        {
            ingredientsInside.Add(ingredient.ingredientName);
            
            AudioManager.Instance.PlaySFXCauldron(); 
            if (cauldronSmokeFXPrefab != null)
            {
                // Spawnear el humo justo donde chocó el ingrediente
                Instantiate(cauldronSmokeFXPrefab, collision.contacts[0].point, cauldronSmokeFXPrefab.transform.rotation);
            }
            ingredient.transform.SetParent(null);
            ingredient.ConsumedIngredient();

            if (ingredientsInside.Count >= 3)
            {
                EvaluatePotion();
            }
        }
    }

    //Método para evaluar si los ingredientes son los de la orden, indica éxito o fracaso
    void EvaluatePotion()
    {
        bool hasSameIngredients = recipeSystem.AreSameIngredients(ingredientsInside, currentOrder.requiredIngredients);

        if (hasSameIngredients == true)
        {
            Debug.Log("Success!");
            AudioManager.Instance.PlaySFXPotionSuccess();
            orderCount++;
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
