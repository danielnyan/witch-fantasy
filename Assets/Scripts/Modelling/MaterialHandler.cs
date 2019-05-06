using UnityEngine;

/// <summary>
/// Complements ModelOptions to facilitate swapping of materials
/// </summary>
public class MaterialHandler : MonoBehaviour
{
    [SerializeField]
    private ModelMaterials[] materials;

    public void UpdateMaterials(ModelMaterialOptions options)
    {
        Material[] selected = new Material[materials.GetLength(0)];
        for (int i = 0; i < materials.GetLength(0); i++)
        {
            selected[i] = options.Dispatcher(materials[i]);
        }
        Renderer r = GetComponent<Renderer>();
        r.materials = selected;
    }
}

