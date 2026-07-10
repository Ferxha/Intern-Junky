using UnityEngine;

public class Trash : MonoBehaviour
{
    //Script para eliminar ingredientes al interactuar con la basura 

    [SerializeField] private GameObject trashSmokeFXPrefab;
    private Ingredient ingredient;

    void OnCollisionEnter(Collision collision)
    {
        ingredient = collision.gameObject.GetComponent<Ingredient>();

        if (ingredient != null)
        {
            Debug.Log($"Ingredient {ingredient.ingredientName} drop to the trash.");
            AudioManager.Instance.PlaySFXTrash(); 
            if (trashSmokeFXPrefab != null)
            {
                Instantiate(trashSmokeFXPrefab, transform.position, trashSmokeFXPrefab.transform.rotation);
            }

            ingredient.transform.SetParent(null);
            ingredient.ConsumedIngredient();
        }
    }
}