using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RecipeSystem : MonoBehaviour
{
    private List<string> liquids = new List<string> {"Green Liquid","Blue Liquid"};
    private List<string> solids = new List<string> {"Goat's Eye", "Hawk Feather", "Dragon's Scale"};
    
    public struct PotionOrder
    {
        public string potionName;
        public List<string> requiredIngredients;
    }
    
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

    public string BuildPotionName(string liquid, string solid1, string solid2)
    {
        string generatedName = (liquid == "Green Liquid") ? "Tonic" : "Potion";

        string adjective1 = GetIngredientAdjective(solid1,1);
        string adjective2 = GetIngredientAdjective(solid1,2);;

        return $"{adjective1} {adjective2} {generatedName}";
    }
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
