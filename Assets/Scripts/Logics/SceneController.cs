using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SceneController : MonoBehaviour {

    private List<EntityView> activeViews = new List<EntityView>();

    public Renderer bgRenderer;
    public PingPongQuad quad;
 
    private float scrollAmount = 0;
    public float scrollSpeed = 0.002f;

    //pool to reuse gameobjects from
    private List<EntityView> recycledViews = new List<EntityView>();
    private Dictionary<string, EntityView> prefabs = new Dictionary<string, EntityView>();

    public void UpdateViews(float dt)
    {
        foreach (var view in activeViews)
        {
            view.Tick(dt);

            if (view.entity.health.value <= 0 && !view.entity.dead)
            {
                var explosion = SpawnPrefab("Explosion");
                explosion.transform.position = view.entity.position;
                RecycleDelayed(explosion, 0.3f);
            }
        }

        scrollAmount = Utils.WrapFloat(scrollAmount + scrollSpeed * dt, 0, float.MaxValue);
        bgRenderer.material.SetFloat("scroll", scrollAmount);
        quad.Swap();
    }

    public EntityView SpawnPrefab(string name)
    {
        var view = recycledViews.FirstOrDefaultFast(v => v.prefabName == name);

        if (view != null)
        {
            recycledViews.Remove(view);
        }
        else
        {
            if (prefabs.ContainsKey(name) == false)
                prefabs[name] = Resources.Load<EntityView>("Prefabs/" + name);

            view = Instantiate(prefabs[name]);
            view.transform.SetParent(this.transform);
            
            view.prefabName = name;
        }

        view.SetVisible(true);
        return view;
    }

    public EntityView ShowEntity(Entity entity, string prefabName)
    {
        var view = SpawnPrefab(prefabName);

        view.entity = entity;
        view.transform.localScale = new Vector3(entity.scale, entity.scale, entity.scale);
        view.transform.position = entity.position;

        activeViews.Add(view);
        return view;
    }

    public void Recycle(EntityView view)
    {
        view.SetVisible(false);
        activeViews.Remove(view);
        recycledViews.Add(view);
    }

    public void Recycle(Entity e)
    {
        var target = activeViews.FirstOrDefaultFast(v => v.entity == e);
        if(target != null)
            Recycle(target);
    }

    public void RecycleDelayed(EntityView view, float delay)
    {
        StartCoroutine(DeathCoroutine(view, delay));
    }

    public void RecycleDelayed(Entity entity, float delay)
    {
        var target = activeViews.FirstOrDefaultFast(v => v.entity == entity);
        Recycle(target);
    }

    
    private IEnumerator DeathCoroutine(EntityView view, float delay)
    {
        yield return new WaitForSeconds(delay);
        Recycle(view);
    }

    public void Reset()
    {
        foreach (var v in activeViews) {
            DestroyObject(v);
        }

        activeViews.Clear();
        recycledViews.Clear();
    }
}
