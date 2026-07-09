using UnityEngine;

public class Trash : MonoBehaviour
{
    private Ingredient ingredient;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ingredient = GameObject.Find("Ingredient").GetComponent<Ingredient>();
    }
    private void OnTriggerEnter(Collider other)
    {
        
        // Si cae un ingrediente y no está en la mano del jugador
        if (ingredient != null && other.transform.parent == null)
        {
            Debug.Log($"Ingrediente {ingredient.ingredientName} enviado a la basura.");
            AudioManager.Instance.PlaySFXTrash(); // Sonido de basura
            ingredient.ConsumedIngredient(); // Desaparece y spawnea otro en su base
        }
    }
}