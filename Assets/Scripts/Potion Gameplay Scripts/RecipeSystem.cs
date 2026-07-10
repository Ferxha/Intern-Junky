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
        string generatedName = (liquid.Trim() == "Green Liquid") ? "Tonico" : "Pocion";

        string adjective1 = GetIngredientAdjective(solid1,1);
        string adjective2 = GetIngredientAdjective(solid2,2);

        return $"{generatedName} {adjective1} {adjective2}";
    }

    //Método para obtener el adjetivo según la posición del ingrediente sólido
    private string GetIngredientAdjective(string ingredientName, int propertyIndex)
    {
        switch (ingredientName.Trim())
        {
            case "Hawk Feather":
                return (propertyIndex == 1) ? "Amargo" : "Seco";
            case "Goat's Eye":
                return (propertyIndex == 1) ? "Dulce" : "Viscoso";
            case "Dragon's Scale":
                return (propertyIndex == 1) ? "Salado" : "Crocante";
            default:
                return "Misterioso";
        }
    }

    //Método compara si son iguales las listas de ingredientes de la orden y el caldero
    public bool AreSameIngredients(List<string> cauldronIngredients, List<string> orderIngredients)
    {
        if (cauldronIngredients.Count != orderIngredients.Count)
        {
            return false;
        }

        List<string> sortedCauldron = cauldronIngredients.Select(x => x.Trim()).OrderBy(x => x).ToList();
        List<string> sortedOrder = orderIngredients.Select(x => x.Trim()).OrderBy(x => x).ToList();

        bool hasLiquid = sortedCauldron.Any(item => liquids.Any(liquid => liquid.Trim() == item));
        if (!hasLiquid)
        {
            Debug.Log($"A liquid is missing. Cauldron: {string.Join(", ", sortedCauldron)} | Order: {string.Join(", ", sortedOrder)}");
            return false;
        }

        for(int i = 0; i < sortedCauldron.Count; i++)
        {
            if (sortedCauldron[i] != sortedOrder[i])
            {
                Debug.Log($"An ingredient is wrong or missing. Cauldron: {string.Join(", ", sortedCauldron)} | Order: {string.Join(", ", sortedOrder)}");
                return false;
            }
        }

        return true;
    }
}
