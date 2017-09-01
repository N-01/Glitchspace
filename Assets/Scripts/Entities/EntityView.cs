using UnityEngine;

public class EntityView : MonoBehaviour {
    public Entity entity; //data model bound to view
    public string prefabName; //can be replaced with some integer id, but ugly

    public virtual void SetVisible(bool state)
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        entity.OnCollision(other.tag);
    }
}