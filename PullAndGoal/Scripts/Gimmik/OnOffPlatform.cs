using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparentPlatformManager : MonoBehaviour
{
    public List<GameObject> groupA = new List<GameObject>();
    public List<GameObject> groupB = new List<GameObject>();

    private List<SpriteRenderer> groupARenderers = new List<SpriteRenderer>();
    private List<SpriteRenderer> groupBRenderers = new List<SpriteRenderer>();

    private List<Collider2D> groupAColliders = new List<Collider2D>();
    private List<Collider2D> groupBColliders = new List<Collider2D>();

    public float switchInterval = 5f; // time in seconds before switching groups

    void Start()
    {
        InitRenderers();
        InitColliders();
        
        StartCoroutine(SwitchGroups());
    }

    private void InitColliders()
    {
        foreach (GameObject obj in groupA)
        {
            Collider2D collider = obj.GetComponent<Collider2D>();
            if (collider!= null)
            {
                groupAColliders.Add(collider);
            }
        }

        foreach (GameObject obj in groupB)
        {
            Collider2D collider = obj.GetComponent<Collider2D>();
            if (collider!= null)
            {
                groupBColliders.Add(collider);
            }
        }
    }

    private void InitRenderers()
    {
        foreach (GameObject obj in groupA)
        {
            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
            if (renderer!= null)
            {
               groupARenderers.Add(renderer);
            }
        }

        foreach (GameObject obj in groupB)
        {
            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
            if (renderer!= null)
            {
              groupBRenderers.Add(renderer);
            }
        }
    }

    IEnumerator SwitchGroups()
    {
        while (true)
        {
            SetGroupTransparent(1.0f, 0.4f);
            SetCollidersEnabled(true);
            
            yield return new WaitForSeconds(switchInterval);

            SetGroupTransparent(0.4f, 1.0f);
            SetCollidersEnabled(false);

            yield return new WaitForSeconds(switchInterval);
        }
    }

    private void SetGroupTransparent(float aGroupTransparency, float bGroupTransparency)
    {
        foreach (var renderer in groupARenderers)
        {
            Color color = renderer.color;
            renderer.color = new Color(color.r, color.g, color.b, aGroupTransparency);
        }

        foreach (var renderer in groupBRenderers)
        {
            Color color = renderer.color;
            renderer.color = new Color(color.r, color.g, color.b, bGroupTransparency);
        }
    }
        

    private void SetCollidersEnabled(bool isAGroupActive)
    {
        if (isAGroupActive)
        {
            foreach (var collider in groupBColliders)
            {
                collider.enabled = false;
            }
            foreach (var collider in groupAColliders)
            {
                collider.enabled = true;
            }
        }
        else
        {
            foreach (var collider in groupBColliders)
            {
                collider.enabled = true;
            }
            foreach (var collider in groupAColliders)
            {
                collider.enabled = false;
            }
        }
    }
}