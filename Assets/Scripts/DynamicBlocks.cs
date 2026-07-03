// using UnityEngine;
// using System.Collections;

// public class DynamicIconBlock : MonoBehaviour
// {
//     private SpriteRenderer spriteRenderer;
//     private MaterialPropertyBlock propBlock;
//     private bool isBlasting = false;

//     public float blastDuration = 0.25f; 

//     [Header("Sand Physics FX")]
//     // 在 Inspector 里把刚刚做好的 SandBlastParticle 预制件拖到这里
//     // Drag your SandBlastParticle prefab here via the Inspector
//     public GameObject sandParticlePrefab; 

//     void Awake()
//     {
//         spriteRenderer = GetComponent<SpriteRenderer>();
//         propBlock = new MaterialPropertyBlock();
//     }

//     public void Setup(Sprite iconSprite, Color neonColor)
//     {
//         if (spriteRenderer != null)
//         {
//             spriteRenderer.sprite = iconSprite;
//             spriteRenderer.GetPropertyBlock(propBlock);
//             propBlock.SetColor("_Color", neonColor);
//             propBlock.SetFloat("_Progress", 0f);
//             spriteRenderer.SetPropertyBlock(propBlock);
//         }
//     }

//     public void TriggerClickBlast()
//     {
//         if (isBlasting) return;
//         isBlasting = true;

//         Collider2D col = GetComponent<Collider2D>();
//         if (col != null) col.enabled = false;

//         // 【核心新增】在自毁前，释放物理飞砂！
//         // [Core New] Spawn physical sand explosion before destruction
//         SpawnSandExplosion();

//         StartCoroutine(BlastRoutine());
//     }

//     void SpawnSandExplosion()
//     {
//         if (sandParticlePrefab == null || spriteRenderer == null) return;

//         // 1. 在当前方块的中心点生成粒子系统
//         // Spawn particle at current block position
//         GameObject particleObj = Instantiate(sandParticlePrefab, transform.position, Quaternion.identity);
//         ParticleSystem ps = particleObj.GetComponent<ParticleSystem>();

//         if (ps != null)
//         {
//             // 2. 动态提取当前图标的纹理，注入粒子的 Texture Sheet / Shape 模块
//             // Dynamic texture injection into the particle emitter
//             var textureModule = ps.textureSheetAnimation;
//             textureModule.enabled = true;
//             textureModule.mode = ParticleSystemAnimationMode.Sprites;
//             // 强行把当前图标的 Sprite 塞进去，让碎屑长得和图标一模一样
//             textureModule.AddSprite(spriteRenderer.sprite); 

//             // 3. 动态提取当前的霓虹色彩，注入粒子的初始颜色
//             // Inherit the exact neon color from the parent icon
//             var mainModule = ps.main;
            
//             // 获取当前 PropertyBlock 里的颜色
//             spriteRenderer.GetPropertyBlock(propBlock);
//             Color currentNeonColor = propBlock.GetColor("_Color");
            
//             // 赋予粒子，并保持超高强度的自发光色
//             mainModule.startColor = new ParticleSystem.MinMaxGradient(currentNeonColor);

//             // 4. 彻底引爆！
//             ps.Play();
//         }
//     }

//     IEnumerator BlastRoutine()
//     {
//         float elapsed = 0f;
//         while (elapsed < blastDuration)
//         {
//             elapsed += Time.deltaTime;
//             float progress = Mathf.Clamp01(elapsed / blastDuration);

//             if (spriteRenderer != null)
//             {
//                 spriteRenderer.GetPropertyBlock(propBlock);
//                 propBlock.SetFloat("_Progress", progress);
//                 spriteRenderer.SetPropertyBlock(propBlock);
//             }
//             yield return null;
//         }
//         Destroy(gameObject);
//     }
// }