using UnityEngine;

public class Trash : MonoBehaviour
{
    private Ingredient ingredient;

    void OnCollisionEnter(Collision collision)
    {
        ingredient = collision.gameObject.GetComponent<Ingredient>();

        if (ingredient != null)
        {
            Debug.Log($"Ingredient {ingredient.ingredientName} drop to the trash.");
            AudioManager.Instance.PlaySFXTrash(); 
            ingredient.transform.SetParent(null);
            ingredient.ConsumedIngredient();
        }
    }
}