using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RecipeSystem : MonoBehaviour
{
    //Script de sistema de creación de pedidos/recetas y nombre de pociones
    private List<string> liquids = new List<string> {"Green Liquid","Blue Liquid"};
    private List<string> solids = new List<string> {"Goat's Eye", "Hawk Feather", "Dragon's Scale"};
    
    //Estructura de datos: nombre + lista de 3 ingredientes
    public struct PotionOrder
    {
        public string potionName;
        public List<string> requiredIngredients;
    }
    
    //Crea la poción de forma aleatoria mezclando las listad de ingredientes 1 liquido + 2 sólidos (pueden repetirse) 
    public PotionOrder GenerateRandomOrder()
    {
        PotionOrder newOrder;
        newOrder.requiredIngredients = new List<string>();

        string randomLiquid = liquids[Random.Range(0, liquids.Count)];
        newOrder.requiredIngredients.Add(randomLiquid);

        string solid1 = solids[Random.Range(0, solids.Count)];
        string solid2 = solids[Random.Range(0, solids.Count)];
        
        newOrder.requiredIngredients.Add(solid1);
        newOrder.requiredIngredients.Add(solid2);

        newOrder.potionName = BuildPotionName(randomLiquid, solid1, solid2);
        Debug.Log($"New Potion Order: {newOrder.potionName}");

        return newOrder;
    }

    //Método para crear el nombre de la poción con base en la orden generada
    public string BuildPotionName(string liquid, string solid1, string solid2)
    {
        string generatedName = (liquid == "Green Liquid") ? "Tonic" : "Potion";

        string adjective1 = GetIngredientAdjective(solid1,1);
        string adjective2 = GetIngredientAdjective(solid1,2);;

        return $"{adjective1} {adjective2} {generatedName}";
    }

    //Método para obtener el adjetivo según la posición del ingrediente sólido
    private string GetIngredientAdjective(string ingredientName, int propertyIndex)
    {
        switch (ingredientName)
        {
            case "Hawk Feather":
                return (propertyIndex == 1) ? "Bitter" : "Dry";
            case "Goat's Eye":
                return (propertyIndex == 1) ? "Sweet" : "Gooey";
            case "Dragon's Scale":
                return (propertyIndex == 1) ? "Salty" : "Crunchy";
            default:
                return "Mysterious ingredient?";
        }
    }

    //Método compara si son iguales las listas de ingredientes de la orden y el caldero
    public bool AreSameIngredients(List<string> cauldronIngredients, List<string> orderIngredients)
    {
        if (cauldronIngredients.Count != orderIngredients.Count)
        {
            return false;
        }

        List<string> sortedCauldron = cauldronIngredients.OrderBy(x => x).ToList();
        List<string> sortedOrder = orderIngredients.OrderBy(x => x).ToList();

        bool hasLiquid = sortedCauldron.Any(item => liquids.Contains(item));        
        if (!hasLiquid)
        {
            Debug.Log("A liquid is missing");
            return false;
        }

        for(int i = 0; i < sortedCauldron.Count; i++)
        {
            if (sortedCauldron[i] != sortedOrder[i])
            {
                Debug.Log("An ingredient is wrong or missing.");
                return false;
            }
        }

        return true;
    }
}
