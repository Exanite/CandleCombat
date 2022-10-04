using System.Collections;
using UnityEngine;

public class DissolveShader : MonoBehaviour
{
    private Material material;
    [SerializeField]
    private SkinnedMeshRenderer smRenderer;

    [SerializeField]
    public float Duration = 1;

    private void Start()
    {
        material = smRenderer.material;
    }

    public void StartDissolve()
    {
        StartCoroutine(Dissolve());
    }

    private IEnumerator Dissolve()
    {
        var timer = 0f;
        while (timer < Duration)
        {
            timer += Time.deltaTime;

            var dissolveValue = Mathf.Lerp(1, 0, timer / Duration);
            material.SetFloat("_DissolveAmount", dissolveValue);

            yield return null;
        }
    }
}