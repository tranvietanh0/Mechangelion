---

origin: theonekit-unity
repository: The1Studio/theonekit-unity
module: unity-base
protected: false
---
---

# Anti-Patterns to Avoid

| Anti-Pattern | Correct Pattern |
|-------------|-----------------|
| `GameObject.Find("name")` | Find by component type or cache reference |
| `GetComponent<T>()` in Update | Cache in Start/OnCreate |
| `SystemAPI.HasComponent<T>(entity)` in foreach | Cache `ComponentLookup<T>` |
| Inline `"shader_name"` string | Named constant |
| `if (x == 0.5f)` | `if (x == SomeThreshold)` |
| `new Material(Shader.Find(...))` repeated | Create once, cache/reuse |
| `string` concatenation in hot paths | `FixedString` or StringBuilder |
| `FindObjectOfType<T>()` | Dependency injection or singleton pattern |
| `GetComponent<T>() ?? AddComponent<T>()` | `var c = Get..; if (c == null) c = Add..;` — `??`/`?.` bypass Unity's null override |
| Type name collision (e.g. `InventoryGridCell` in 2 namespaces) | `using GridCellUI = DOTSUI.InventoryGridCell` to disambiguate |
