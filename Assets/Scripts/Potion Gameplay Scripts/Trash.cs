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
                Vector3 spawnPosition = collision.contactCount > 0
                    ? collision.GetContact(0).point
                    : collision.transform.position;

                GameObject smokeFX = Instantiate(
                    trashSmokeFXPrefab,
                    spawnPosition,
                    trashSmokeFXPrefab.transform.rotation);

                foreach (ParticleSystem particleSystem in smokeFX.GetComponentsInChildren<ParticleSystem>())
                {
                    particleSystem.Play(true);
                }
            }

            ingredient.transform.SetParent(null);
            ingredient.ConsumedIngredient();
        }
    }
}
