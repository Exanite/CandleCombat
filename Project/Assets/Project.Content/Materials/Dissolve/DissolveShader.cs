using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveShader : MonoBehaviour
{
    private Material material;
    [SerializeField] private SkinnedMeshRenderer smRenderer = null;
  
    private float dissolveValue = 1f;
    private float timer;
    [SerializeField] private float duration = 0f;

    private void Start()
    {
        material = smRenderer.material;
        
    }

    private void Update()
    {
       // material.SetFloat("_DissolveAmount", dissolveValue);
    }

    public void DissolveStart()
    {
        StartCoroutine(Dissolve());
        
    }
    private IEnumerator Dissolve()
    {
        while(timer < duration)
        {
            timer += Time.deltaTime;

            dissolveValue = Mathf.Lerp(1, 0, timer / duration);

            material.SetFloat("_DissolveAmount", dissolveValue);

            yield return null;
        }

        
    }
}
