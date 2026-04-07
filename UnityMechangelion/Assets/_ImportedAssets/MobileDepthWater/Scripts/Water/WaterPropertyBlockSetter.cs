namespace Assets.Scripts.Water
{
    using UnityEngine;

    /// <summary>
    /// This class helps you to set water properties for a lot of materials at the same time. 
    /// So you don't have to make it for each independently.
    /// Put it on the scene, add renderers and set up your water.
    /// </summary>
    [ExecuteInEditMode]
    public class WaterPropertyBlockSetter : MonoBehaviour
    {
        [SerializeField] private Renderer[] waterRenderers;

        [Space]
        [SerializeField] private Color waterColor;
        [SerializeField] private Texture waterTex;
        [SerializeField] private Vector2 waterTile;
        [Range(0, 1)][SerializeField] private float textureVisibility;

        [Space]
        [SerializeField] private Texture distortionTex;
        [SerializeField] private Vector2 distortionTile;

        [Space]
        [SerializeField] private float waterHeight;
        [SerializeField] private float waterDeep;
        [Range(0, 0.1f)][SerializeField] private float waterDepthParam;
        [Range(0, 1)][SerializeField] private float waterMinAlpha;

        [Space]
        [SerializeField] private Color borderColor;
        [Range(0, 1)][SerializeField] private float borderWidth;

        [Space]
        [SerializeField] private Vector2 moveDirection;

        private MaterialPropertyBlock materialPropertyBlock;

        public MaterialPropertyBlock MaterialPropertyBlock
        {
            get { return this.materialPropertyBlock; }
        }

        public void Awake()
        {
            this.materialPropertyBlock = new MaterialPropertyBlock();
            this.SetUpPropertyBlock(this.materialPropertyBlock);

            if (this.waterRenderers != null)
            {
                for (var i = 0; i < this.waterRenderers.Length; i++)
                {
                    this.waterRenderers[i].SetPropertyBlock(this.materialPropertyBlock);
                }
            }
        }

#if UNITY_EDITOR
        public void OnEnable()
        {
            this.materialPropertyBlock = new MaterialPropertyBlock();
            this.SetUpPropertyBlock(this.materialPropertyBlock);
        }

        public void Update()
        {
            this.SetUpPropertyBlock(this.materialPropertyBlock);

            if (this.waterRenderers != null)
            {
                for (var i = 0; i < this.waterRenderers.Length; i++)
                {
                    this.waterRenderers[i].SetPropertyBlock(this.materialPropertyBlock);
                }
            }
        }
#endif

        private void SetUpPropertyBlock(MaterialPropertyBlock propertyBlock)
        {
            propertyBlock.SetColor("_WaterColor", this.waterColor);
            propertyBlock.SetColor("_BorderColor", this.borderColor);

            propertyBlock.SetVector("_Tiling", this.waterTile);
            propertyBlock.SetVector("_DistTiling", this.distortionTile);
            propertyBlock.SetVector("_MoveDirection", new Vector4(this.moveDirection.x, 0f, this.moveDirection.y, 0f));

            if (this.waterTex != null)
            {
                propertyBlock.SetTexture("_WaterTex", this.waterTex);
            }

            if (this.distortionTex != null)
            {
                propertyBlock.SetTexture("_DistTex", this.distortionTex);
            }

            propertyBlock.SetFloat("_TextureVisibility", this.textureVisibility);
            propertyBlock.SetFloat("_WaterHeight", this.waterHeight);
            propertyBlock.SetFloat("_WaterDeep", this.waterDeep);
            propertyBlock.SetFloat("_WaterDepth", this.waterDepthParam);
            propertyBlock.SetFloat("_WaterMinAlpha", this.waterMinAlpha);
            propertyBlock.SetFloat("_BorderWidth", this.borderWidth);
        }
    }
}
