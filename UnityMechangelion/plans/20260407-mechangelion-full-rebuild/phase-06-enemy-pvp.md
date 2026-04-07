# Phase 09B: PvP Extension

**Status:** Deferred until after M1 playable ordinary loop
**Depends On:** stable M1 combat, battle orchestration, meta services

---

## Objective

Add PvP opponent support by reusing the same combat platform built for M1.

PvP should primarily be a new opponent source + config/rules variant, not a separate gameplay architecture.

---

## Must Reuse From M1

- shared combat stats / damage / cooldown / defense flow
- shared battle coordinator and result flow
- shared HUD and input routing
- shared enemy spawning/factory/service boundaries

---

## PvP-Specific Additions

- mirrored or authored PvP opponent config
- PvP enemy resolver / factory branch
- PvP balancing and reward rules
- optional rival/profile metadata

---

## Guardrails

- do not hardcode ordinary enemy assumptions into M1 services
- do not create a separate PvP state machine
- keep PvP data/config isolated from ordinary enemy config where needed
