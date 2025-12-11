using System.Linq;

public class ActionResolver
{
    private readonly ActionDefinition basicSet;
    private readonly RagInventory ragInventory;

    public ActionResolver(ActionDefinition basicActions, RagInventory inventory)
    {
        basicSet = basicActions;
        ragInventory = inventory;
    }

    public ActionVariant Resolve(ActionKey key, ActionDirection direction, ActionContext context)
    {
        ActionDefinition set = key switch
        {
            ActionKey.Basic => basicSet,
            ActionKey.Special => ragInventory != null ? ragInventory.GetSpecial(context) : null,
            ActionKey.Trick => ragInventory != null ? ragInventory.GetTrick(context) : null,
            _ => null
        };

        if (set == null || set.variants == null) return null;

        return set.variants.FirstOrDefault(v => v != null && v.context == context && v.direction == direction);
    }
}
