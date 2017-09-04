using UnityEngine;

public class EntityView : MonoBehaviour {
    [HideInInspector]
    public Entity entity; //data model bound to view

    public Renderer renderer;

    public string prefabName; //can be replaced with some integer id, faster but harder to maintain

    public void SetVisible(bool state)
    {
        //you can actually avoid SetActive through enablind/disabling components
        //but i kept it simple
        gameObject.SetActive(state);
        damageBlink = 0;
    }

    private float damageBlink = 0;

    public void Tick(float dt)
    {
        transform.position = entity.position;
        transform.rotation = Quaternion.Euler(entity.rotation);

        if (renderer != null)
        {
            renderer.material.SetColor("_EmissionColor", new Color(1, 0.1f, 0.2f) * damageBlink);
            damageBlink = Mathf.Max(damageBlink - dt * 8, 0);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (entity.OnCollision(other.tag))
            damageBlink = 1;
    }
}