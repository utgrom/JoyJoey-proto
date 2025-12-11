using UnityEngine;

[CreateAssetMenu(fileName = "RagProfile", menuName = "JoyJoey/Rag Profile")]
public class RagProfile : ScriptableObject
{
    public string ragId;
    public Sprite icon;

    [Header("Actions")]
    public ActionDefinition specialGround;
    public ActionDefinition specialAir;
    public ActionDefinition trickGround;
    public ActionDefinition trickAir;
}
