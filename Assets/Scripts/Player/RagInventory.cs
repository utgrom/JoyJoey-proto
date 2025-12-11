using System;
using System.Collections.Generic;
using UnityEngine;

public class RagInventory : MonoBehaviour
{
    [SerializeField] private List<RagProfile> rags = new List<RagProfile>();
    private int index;

    public event Action<RagProfile> OnRagChanged;

    public RagProfile Current => rags.Count > 0 ? rags[index] : null;
    public int Count => rags.Count;

    public void AddRag(RagProfile rag)
    {
        if (rag == null || rags.Contains(rag)) return;
        rags.Add(rag);
        index = Mathf.Clamp(index, 0, rags.Count - 1);
        OnRagChanged?.Invoke(Current);
    }

    public void RotateNext()
    {
        if (rags.Count <= 1) return;
        index = (index + 1) % rags.Count;
        OnRagChanged?.Invoke(Current);
    }

    public void RotatePrev()
    {
        if (rags.Count <= 1) return;
        index = (index - 1 + rags.Count) % rags.Count;
        OnRagChanged?.Invoke(Current);
    }

    public ActionDefinition GetSpecial(ActionContext ctx)
    {
        if (Current == null) return null;
        return ctx == ActionContext.Ground ? Current.specialGround : Current.specialAir;
    }

    public ActionDefinition GetTrick(ActionContext ctx)
    {
        if (Current == null) return null;
        return ctx == ActionContext.Ground ? Current.trickGround : Current.trickAir;
    }
}
