using UnityEngine;

[ExecuteInEditMode]
public class CameraShaderFollow : MonoBehaviour
{
    public Material effectMaterial;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (effectMaterial != null)
            Graphics.Blit(source, destination, effectMaterial);
        else
            Graphics.Blit(source, destination);
    }
}