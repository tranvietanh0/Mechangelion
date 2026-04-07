# Phase 07: Animation/VFX Feature

**Effort:** M (2 days)
**Dependencies:** Phase 1 (Core signals)
**Blocked By:** Phase 1
**Can Run Parallel With:** Phase 4, 5, 6

---

## Objective

Implement animation services và VFX: tween service, VFX pooling, effects.

---

## File Ownership

```
Assets/Scripts/Features/Animation/
├── Interfaces/
│   ├── ITweenTarget.cs             [NEW]
│   └── IVFXEmitter.cs              [NEW]
│
├── Services/
│   ├── TweenService.cs             [NEW]
│   └── VFXService.cs               [NEW]
│
└── Effects/
    ├── HitEffect.cs                [NEW]
    ├── SlowMoEffect.cs             [NEW]
    ├── StatusEffect.cs             [NEW]
    └── ScreenShakeEffect.cs        [NEW]
```

**Total Files:** 8

---

## Implementation Details

### 1. Interfaces

```csharp
// ITweenTarget.cs
public interface ITweenTarget
{
    Transform Transform { get; }
    bool IsActive { get; }
}

// IVFXEmitter.cs
public interface IVFXEmitter
{
    void EmitAt(Vector3 position);
    void EmitAt(Vector3 position, Quaternion rotation);
    void Stop();
}
```

### 2. Tween Service

```csharp
// TweenService.cs
public class TweenService : ITickable
{
    private readonly List<TweenInstance> activeTweens = new();
    private readonly Queue<TweenInstance> pendingRemoval = new();
    
    #region Public API
    
    public TweenHandle MoveTo(Transform target, Vector3 endPosition, float duration, EaseType ease = EaseType.OutQuad)
    {
        var tween = new TweenInstance
        {
            Target = target,
            TweenType = TweenType.Position,
            StartValue = target.position,
            EndValue = endPosition,
            Duration = duration,
            Ease = ease,
            StartTime = Time.time
        };
        
        this.activeTweens.Add(tween);
        return new TweenHandle(tween);
    }
    
    public TweenHandle RotateTo(Transform target, Quaternion endRotation, float duration, EaseType ease = EaseType.OutQuad)
    {
        var tween = new TweenInstance
        {
            Target = target,
            TweenType = TweenType.Rotation,
            StartRotation = target.rotation,
            EndRotation = endRotation,
            Duration = duration,
            Ease = ease,
            StartTime = Time.time
        };
        
        this.activeTweens.Add(tween);
        return new TweenHandle(tween);
    }
    
    public TweenHandle ScaleTo(Transform target, Vector3 endScale, float duration, EaseType ease = EaseType.OutQuad)
    {
        var tween = new TweenInstance
        {
            Target = target,
            TweenType = TweenType.Scale,
            StartValue = target.localScale,
            EndValue = endScale,
            Duration = duration,
            Ease = ease,
            StartTime = Time.time
        };
        
        this.activeTweens.Add(tween);
        return new TweenHandle(tween);
    }
    
    public TweenHandle ValueTo(float startValue, float endValue, float duration, Action<float> onUpdate, EaseType ease = EaseType.OutQuad)
    {
        var tween = new TweenInstance
        {
            TweenType = TweenType.Value,
            StartFloat = startValue,
            EndFloat = endValue,
            Duration = duration,
            Ease = ease,
            StartTime = Time.time,
            OnUpdate = onUpdate
        };
        
        this.activeTweens.Add(tween);
        return new TweenHandle(tween);
    }
    
    public void Cancel(TweenHandle handle)
    {
        if (handle?.Instance != null)
        {
            this.pendingRemoval.Enqueue(handle.Instance);
        }
    }
    
    public void CancelAll(Transform target)
    {
        foreach (var tween in this.activeTweens.Where(t => t.Target == target))
        {
            this.pendingRemoval.Enqueue(tween);
        }
    }
    
    #endregion
    
    #region ITickable
    
    public void Tick()
    {
        // Remove pending
        while (this.pendingRemoval.Count > 0)
        {
            var tween = this.pendingRemoval.Dequeue();
            this.activeTweens.Remove(tween);
        }
        
        // Update active tweens
        float time = Time.time;
        foreach (var tween in this.activeTweens.ToList())
        {
            if (tween.Target == null && tween.TweenType != TweenType.Value)
            {
                this.pendingRemoval.Enqueue(tween);
                continue;
            }
            
            float elapsed = time - tween.StartTime;
            float t = Mathf.Clamp01(elapsed / tween.Duration);
            float easedT = ApplyEase(t, tween.Ease);
            
            ApplyTween(tween, easedT);
            
            if (t >= 1f)
            {
                tween.OnComplete?.Invoke();
                this.pendingRemoval.Enqueue(tween);
            }
        }
    }
    
    #endregion
    
    #region Private
    
    private void ApplyTween(TweenInstance tween, float t)
    {
        switch (tween.TweenType)
        {
            case TweenType.Position:
                tween.Target.position = Vector3.Lerp(tween.StartValue, tween.EndValue, t);
                break;
            case TweenType.Rotation:
                tween.Target.rotation = Quaternion.Slerp(tween.StartRotation, tween.EndRotation, t);
                break;
            case TweenType.Scale:
                tween.Target.localScale = Vector3.Lerp(tween.StartValue, tween.EndValue, t);
                break;
            case TweenType.Value:
                float value = Mathf.Lerp(tween.StartFloat, tween.EndFloat, t);
                tween.OnUpdate?.Invoke(value);
                break;
        }
    }
    
    private float ApplyEase(float t, EaseType ease)
    {
        return ease switch
        {
            EaseType.Linear => t,
            EaseType.InQuad => t * t,
            EaseType.OutQuad => 1 - (1 - t) * (1 - t),
            EaseType.InOutQuad => t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2,
            EaseType.InQuint => t * t * t * t * t,
            EaseType.OutQuint => 1 - Mathf.Pow(1 - t, 5),
            EaseType.InOutQuint => t < 0.5f ? 16 * t * t * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 5) / 2,
            _ => t
        };
    }
    
    #endregion
}

public class TweenInstance
{
    public Transform Target;
    public TweenType TweenType;
    public Vector3 StartValue;
    public Vector3 EndValue;
    public Quaternion StartRotation;
    public Quaternion EndRotation;
    public float StartFloat;
    public float EndFloat;
    public float Duration;
    public EaseType Ease;
    public float StartTime;
    public Action OnComplete;
    public Action<float> OnUpdate;
}

public class TweenHandle
{
    internal TweenInstance Instance;
    
    public TweenHandle(TweenInstance instance) => Instance = instance;
    
    public TweenHandle OnComplete(Action callback)
    {
        Instance.OnComplete = callback;
        return this;
    }
}

public enum TweenType { Position, Rotation, Scale, Value }
public enum EaseType { Linear, InQuad, OutQuad, InOutQuad, InQuint, OutQuint, InOutQuint }
```

### 3. VFX Service

```csharp
// VFXService.cs
public class VFXService : IInitializable, IDisposable
{
    private readonly IGameAssets gameAssets;
    private readonly Dictionary<string, Queue<ParticleSystem>> pools = new();
    private readonly Dictionary<string, ParticleSystem> prefabs = new();
    private Transform poolRoot;
    
    private const int InitialPoolSize = 5;
    
    public VFXService(IGameAssets gameAssets)
    {
        this.gameAssets = gameAssets;
    }
    
    public void Initialize()
    {
        this.poolRoot = new GameObject("VFX_Pool").transform;
        UnityEngine.Object.DontDestroyOnLoad(this.poolRoot.gameObject);
    }
    
    public void Dispose()
    {
        if (this.poolRoot != null)
            UnityEngine.Object.Destroy(this.poolRoot.gameObject);
    }
    
    public async UniTask PreloadAsync(string vfxId)
    {
        if (this.prefabs.ContainsKey(vfxId)) return;
        
        var prefab = await this.gameAssets.LoadAssetAsync<ParticleSystem>($"VFX/{vfxId}");
        this.prefabs[vfxId] = prefab;
        this.pools[vfxId] = new Queue<ParticleSystem>();
        
        // Pre-warm pool
        for (int i = 0; i < InitialPoolSize; i++)
        {
            var instance = CreateInstance(vfxId);
            Return(vfxId, instance);
        }
    }
    
    public ParticleSystem Spawn(string vfxId, Vector3 position, Quaternion rotation)
    {
        if (!this.pools.TryGetValue(vfxId, out var pool) || pool.Count == 0)
        {
            // Create new if pool empty
            if (!this.prefabs.ContainsKey(vfxId))
            {
                Debug.LogWarning($"VFX not preloaded: {vfxId}");
                return null;
            }
            
            pool = this.pools[vfxId];
            if (pool.Count == 0)
            {
                var newInstance = CreateInstance(vfxId);
                pool.Enqueue(newInstance);
            }
        }
        
        var vfx = pool.Dequeue();
        vfx.transform.SetPositionAndRotation(position, rotation);
        vfx.gameObject.SetActive(true);
        vfx.Play();
        
        // Auto-return when done
        StartAutoReturn(vfxId, vfx);
        
        return vfx;
    }
    
    public void Return(string vfxId, ParticleSystem vfx)
    {
        if (vfx == null) return;
        
        vfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        vfx.gameObject.SetActive(false);
        vfx.transform.SetParent(this.poolRoot);
        
        if (this.pools.TryGetValue(vfxId, out var pool))
            pool.Enqueue(vfx);
    }
    
    private ParticleSystem CreateInstance(string vfxId)
    {
        var prefab = this.prefabs[vfxId];
        var instance = UnityEngine.Object.Instantiate(prefab, this.poolRoot);
        instance.gameObject.SetActive(false);
        return instance;
    }
    
    private async void StartAutoReturn(string vfxId, ParticleSystem vfx)
    {
        // Wait for particle system to finish
        await UniTask.WaitUntil(() => !vfx.isPlaying);
        Return(vfxId, vfx);
    }
}
```

### 4. Effects

```csharp
// HitEffect.cs
public class HitEffect
{
    private readonly VFXService vfxService;
    private readonly TweenService tweenService;
    
    private const string HitVFXId = "hit_impact";
    private const string CriticalVFXId = "hit_critical";
    
    public HitEffect(VFXService vfxService, TweenService tweenService)
    {
        this.vfxService = vfxService;
        this.tweenService = tweenService;
    }
    
    public void Play(Vector3 position, bool isCritical = false)
    {
        string vfxId = isCritical ? CriticalVFXId : HitVFXId;
        this.vfxService.Spawn(vfxId, position, Quaternion.identity);
    }
}

// SlowMoEffect.cs
public class SlowMoEffect
{
    private readonly TweenService tweenService;
    private TweenHandle currentTween;
    
    private const float SlowTimeScale = 0.2f;
    private const float TransitionDuration = 0.1f;
    
    public SlowMoEffect(TweenService tweenService)
    {
        this.tweenService = tweenService;
    }
    
    public void Activate(float duration)
    {
        // Slow down
        this.currentTween = this.tweenService.ValueTo(1f, SlowTimeScale, TransitionDuration, 
            value => Time.timeScale = value, EaseType.OutQuad);
        
        this.currentTween.OnComplete(async () =>
        {
            await UniTask.Delay(TimeSpan.FromSeconds(duration * SlowTimeScale));
            Resume();
        });
    }
    
    public void Resume()
    {
        this.tweenService.Cancel(this.currentTween);
        this.tweenService.ValueTo(Time.timeScale, 1f, TransitionDuration,
            value => Time.timeScale = value, EaseType.InQuad);
    }
}

// StatusEffect.cs
public class StatusEffect
{
    private readonly VFXService vfxService;
    
    public StatusEffect(VFXService vfxService)
    {
        this.vfxService = vfxService;
    }
    
    public void PlayFreeze(Transform target)
    {
        this.vfxService.Spawn("status_freeze", target.position, Quaternion.identity);
    }
    
    public void PlayDamageBoost(Transform target)
    {
        this.vfxService.Spawn("status_damage_boost", target.position, Quaternion.identity);
    }
    
    public void PlayShieldBoost(Transform target)
    {
        this.vfxService.Spawn("status_shield_boost", target.position, Quaternion.identity);
    }
}

// ScreenShakeEffect.cs
public class ScreenShakeEffect
{
    private readonly TweenService tweenService;
    private Camera camera;
    private Vector3 originalPosition;
    
    public ScreenShakeEffect(TweenService tweenService)
    {
        this.tweenService = tweenService;
    }
    
    public void SetCamera(Camera camera)
    {
        this.camera = camera;
        this.originalPosition = camera.transform.localPosition;
    }
    
    public void Shake(float intensity = 0.3f, float duration = 0.2f)
    {
        if (this.camera == null) return;
        
        this.tweenService.ValueTo(intensity, 0f, duration, value =>
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * value;
            float y = UnityEngine.Random.Range(-1f, 1f) * value;
            this.camera.transform.localPosition = this.originalPosition + new Vector3(x, y, 0);
        }, EaseType.OutQuad).OnComplete(() =>
        {
            this.camera.transform.localPosition = this.originalPosition;
        });
    }
}
```

---

## DI Registration

```csharp
// MainSceneScope.Configure()
builder.Register<TweenService>(Lifetime.Scoped).AsInterfacesAndSelf();
builder.Register<VFXService>(Lifetime.Scoped).AsInterfacesAndSelf();
builder.Register<HitEffect>(Lifetime.Scoped);
builder.Register<SlowMoEffect>(Lifetime.Scoped);
builder.Register<StatusEffect>(Lifetime.Scoped);
builder.Register<ScreenShakeEffect>(Lifetime.Scoped);
```

---

## Verification Checklist

- [ ] TweenService updates all active tweens
- [ ] VFXService pools particles correctly
- [ ] Effects play at correct positions
- [ ] SlowMo affects Time.timeScale correctly
- [ ] Screen shake resets to original position

---

## Notes

- TweenService is custom implementation (can swap to DOTween later)
- VFX uses object pooling for performance
- Effects are stateless services, inject dependencies
- Addressables paths: `VFX/{id}` for VFX prefabs
