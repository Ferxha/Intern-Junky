using UnityEngine;

public class Trash : MonoBehaviour
{
    private Ingredient ingredient;
    private void OnTriggerEnter(Collider other)
    {
        ingredient = other.GetComponent<Ingredient>();
        Debug.Log("Detect the trigger");

        if (ingredient != null)
        {
            Debug.Log($"Ingredient {ingredient.ingredientName} drop to the trash.");
            AudioManager.Instance.PlaySFXTrash(); 
            ingredient.transform.SetParent(null);
            ingredient.ConsumedIngredient();
        } 
    }
}