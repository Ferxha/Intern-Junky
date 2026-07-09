using UnityEngine;
using System.Collections.Generic;

public class IngredientSpawner : MonoBehaviour
{
    
    [SerializeField] private List<GameObject> ingredientPrefabs;
    [SerializeField] private List<Transform> spawnPositions; //ACTUALIZAR CUANDO SE TENGA LA MESA DE TRABAJO DEFINIDA

    // Diccionario para saber qué punto de spawn está ocupado por cuál ingrediente
    private Dictionary<Transform, GameObject> activeIngredients = new Dictionary<Transform, GameObject>();

    void Awake()
    {
        //Limpiar las posiciones de spawn para evitar ingredientes activos sorpresa
        foreach (Transform position in spawnPositions)
        {
            activeIngredients[position] = null;
        }
    }

    void Start()
    {
        SpawnAllIngredients();
    }

    // Spawnea ingredientes en todos los puntos que estén vacíos
    public void SpawnAllIngredients()
    {
        foreach (Transform position in spawnPositions)
        {
            if (!activeIngredients.ContainsKey(position) || activeIngredients[position] == null)
            {
                SpawnIngredient(position);
            }
        }
    }

    private void SpawnIngredient(Transform position)
    {
        if (ingredientPrefabs.Count != 0)
        {

        int randomPrefabIndex = Random.Range(0, ingredientPrefabs.Count);
        GameObject newIngredient = Instantiate(ingredientPrefabs[randomPrefabIndex], position.position, position.rotation);

        // Le avisamos al ingrediente cuál es su punto de origen para cuando muera, así sabe donde regresar
        Ingredient ingredient = newIngredient.GetComponent<Ingredient>();
        if (ingredient != null)
        {
            ingredient.spawnPosition = position;
        }

        // Registrar en el diccionario, estos son los ingredientes activos en la mesa
        activeIngredients[position] = newIngredient;
        }
    }

    // Se llama cuando un ingrediente va a la basura o al caldero
    public void ReportIngredientDestroyed(Transform position)
    {
        if (activeIngredients.ContainsKey(position))
        {
            activeIngredients.Remove(position);
        }
        
        // Reaparece uno nuevo inmediatamente en ese hueco libre
        SpawnIngredient(position);
    }

    // Limpia la mesa por completo para barajar nuevas posiciones (al terminar la poción)
    public void RespawnEverythingInNewPositions()
    {
        // Liimpia la mesa de ingredientes
        foreach (var pair in activeIngredients)
        {
            if (pair.Value != null) {
                Destroy(pair.Value);
            }
        }
        activeIngredients.Clear(); //Limpia el diccionario de ingredientes activos

        // Mezclar la lista de puntos de spawn de forma aleatoria
        for (int i = 0; i < spawnPositions.Count; i++)
        {
            Transform temp = spawnPositions[i]; //Variable temporal para cambaiar de lugar
            int randomIndex = Random.Range(i, spawnPositions.Count); //Indice aleatorio entre i y el final de la lista
            spawnPositions[i] = spawnPositions[randomIndex]; //Actualiza el valor con el de otro de la lista
            spawnPositions[randomIndex] = temp; //Catafixia el valor que se cambio con el temporal, así evitamos repetición y no perdemos ninguna posición
        }

        // Volver a llenar todos los puntos ahora que están mezclados
        SpawnAllIngredients();
    }
}