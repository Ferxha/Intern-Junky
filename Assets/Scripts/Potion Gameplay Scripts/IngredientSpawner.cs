using UnityEngine;
using System.Collections.Generic;

public class IngredientSpawner : MonoBehaviour
{
    //Script controlador de la lista de ingredientes a spawnear y lista de sus ubicaciones
    [SerializeField] private List<GameObject> ingredientPrefabs;
    [SerializeField] private List<Transform> spawnPositions;

    // Diccionario para saber qué punto de spawn está ocupado por cuál ingrediente
    private Dictionary<Transform, GameObject> activeIngredients = new Dictionary<Transform, GameObject>();

    void Awake()
    {
        foreach (Transform position in spawnPositions)
        {
            activeIngredients[position] = null;
        }
    }

    void Start()
    {
        ShufflePositions();
        SpawnAllIngredients();
    }

    // Spawnea ingredientes en todos los puntos que estén vacíos
    public void SpawnAllIngredients()
    {
        if (spawnPositions.Count == 0 || ingredientPrefabs.Count == 0) return; 

        for (int i = 0; i < ingredientPrefabs.Count; i++)
        {
            if(i >= spawnPositions.Count){break;} 

            Transform currentPosition = spawnPositions[i];

            if (!activeIngredients.ContainsKey(currentPosition) || activeIngredients[currentPosition] == null)
            {
                GameObject newIngredient = Instantiate(ingredientPrefabs[i], currentPosition.position, currentPosition.rotation);

                Ingredient ingredientScript = newIngredient.GetComponent<Ingredient>();
                if (ingredientScript != null)
                {
                    ingredientScript.spawnPosition = currentPosition;
                }

                activeIngredients[currentPosition] = newIngredient;
            }
        }
        
    }


    // Se llama cuando un ingrediente va a la basura o al caldero
    public void ReportIngredientDestroyed(Transform position)
    {
        if (activeIngredients.ContainsKey(position))
        {
            activeIngredients[position] = null;
        }
        
        SpawnAllIngredients();
    }

    // Limpia la mesa por completo para barajar nuevas posiciones (al terminar la poción)
    public void RespawnEverythingInNewPositions()
    {
        foreach (var ingredientInTable in activeIngredients)
        {
            if (ingredientInTable.Value != null) {
                Destroy(ingredientInTable.Value);
            }
        }
        activeIngredients.Clear(); //Limpia el diccionario de ingredientes activos

        ShufflePositions();
        SpawnAllIngredients();
    }

    // Mezcla de posiciones
    public void ShufflePositions()
    {
        for (int i = 0; i < spawnPositions.Count; i++)
        {
            Transform temp = spawnPositions[i]; 
            int randomIndex = Random.Range(i, spawnPositions.Count); 
            spawnPositions[i] = spawnPositions[randomIndex]; 
            spawnPositions[randomIndex] = temp; 
        }
    }
}