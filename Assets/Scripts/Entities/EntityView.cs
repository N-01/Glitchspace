using UnityEngine;

public class EntityView : MonoBehaviour {
    [HideInInspector]
    public Entity entity; //data model bound to view
    public string prefabName; //can be replaced with some integer id, but ugly

    public void SetVisible(bool state)
    {
        gameObject.SetActive(state);
    }

    public void OnTriggerEnter(Collider other)
    {
        entity.OnCollision(other.tag);
    }
}