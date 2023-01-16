
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GemRequirementObject")]
public class GemRequirementData : ScriptableObject
{
    public bool OneAndAutoRequirement = true;
    public List<Gem.GemTypes> requirementList = new List<Gem.GemTypes>();

    public Gem.GemTypes[] GetReqiredGemType(Gem.GemTypes type)
    {
        Gem.GemTypes requiredType = Gem.GemTypes.Blue;
        switch (type)
        {
            case Gem.GemTypes.Blue:
                requiredType = Gem.GemTypes.Red;
                break;
            case Gem.GemTypes.Green:
                requiredType = Gem.GemTypes.Yellow;
                break;
            case Gem.GemTypes.Red:
                requiredType = Gem.GemTypes.Green;
                break;
            case Gem.GemTypes.Yellow:
                requiredType = Gem.GemTypes.Blue;
                break;
        }
        if (OneAndAutoRequirement)
            return new Gem.GemTypes[] { requiredType };
        else
            return requirementList.ToArray();
    }

}
