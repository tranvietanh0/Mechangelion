# Phase 09A: Boss Extension

**Status:** Deferred until after M1 playable ordinary loop
**Depends On:** stable M1 combat, battle orchestration, presentation

---

## Objective

Add boss-specific behavior on top of the shared combat and battle foundations created in M1.

Boss should extend, not fork, the M1 architecture.

---

## Must Reuse From M1

- existing `GameStateMachine`
- shared battle coordinator / result flow
- shared HUD and input contracts
- shared combat services
- enemy spawning/factory extension points

---

## Boss-Specific Additions

- boss runtime subclass / strategy
- optional module damage system
- optional rage / phase logic
- boss config entries
- boss presentation additions

---

## Guardrails

- do not make boss systems mandatory for ordinary battles
- do not rewrite battle orchestration around boss-only assumptions
- module/rage systems should remain optional layers
