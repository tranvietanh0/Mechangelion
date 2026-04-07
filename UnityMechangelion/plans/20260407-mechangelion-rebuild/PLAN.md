# Implementation Plan: Mechangelion Rebuild

**Created:** 2026-04-07
**Architecture:** Feature-Based Modular (MonoBehaviour + VContainer)
**Design Doc:** [docs/architecture-design.md](../../docs/architecture-design.md)

---

## Overview

Rebuild Mechangelion game từ đầu theo SOLID principles sử dụng VContainer DI, UniTask async, và SignalBus (MessagePipe).

## Phases Summary

| Phase | Name | Effort | Dependencies | Files |
|-------|------|--------|--------------|-------|
| 1 | Core + Infrastructure | S (1d) | None | 15 |
| 2 | Progression Feature | M (2d) | Phase 1 | 12 |
| 3 | Combat - Models & Services | S (1d) | Phase 1 | 10 |
| 4 | Combat - Player | M (3d) | Phase 2, 3 | 8 |
| 5 | Combat - Enemy | L (4d) | Phase 3 | 10 |
| 6 | Input Feature | S (1d) | Phase 4 | 5 |
| 7 | Animation/VFX Feature | M (2d) | Phase 1 | 8 |
| 8 | Battle Feature | M (3d) | Phase 4, 5, 6 | 10 |
| 9 | UI | M (3d) | Phase 8 | 10 |
| 10 | Integration | M (2d) | All | 3 |

**Total Estimated Effort:** ~22 days (4-5 weeks)

---

## Risk Assessment

| Risk | L | I | Score | Mitigation |
|------|---|---|-------|------------|
| Scope creep từ source game | 4 | 4 | 16 | Strict feature parity checklist mỗi phase |
| VContainer DI complexity | 3 | 3 | 9 | Unit test services trước khi wire |
| Animation system mismatch | 3 | 3 | 9 | Abstract TweenService, swap impl later |
| Enemy AI complexity | 4 | 3 | 12 | Start simple, add behaviors incrementally |
| Cross-feature coupling | 3 | 4 | 12 | SignalBus only, no direct references |

**High Risk (>=15):** Scope creep - mandate feature checklist review mỗi phase end.

---

## Critical Path

```
Phase 1 → Phase 2 ─┬→ Phase 4 ─┬→ Phase 6 ─┐
         Phase 3 ──┘           │           │
                   Phase 5 ────┴→ Phase 8 ─┼→ Phase 9 → Phase 10
                   Phase 7 ───────────────┘
```

**Parallel Opportunities:**
- Phase 2 và 3 có thể chạy song song sau Phase 1
- Phase 5 và 7 có thể chạy song song
- Phase 6 phụ thuộc Phase 4 (input cần player controller)

---

## Agent Routing

| Phase | Role | Agent |
|-------|------|-------|
| 1-10 | implementer | unity-developer |
| All | tester | dots-tester |
| All | reviewer | dots-reviewer |

**Note:** Dùng `unity-developer` thay vì `dots-implementer` vì project này là MonoBehaviour-based.

---

## Phase Files

- [Phase 01: Core + Infrastructure](phase-01-core-infrastructure.md)
- [Phase 02: Progression Feature](phase-02-progression.md)
- [Phase 03: Combat Models & Services](phase-03-combat-services.md)
- [Phase 04: Combat Player](phase-04-combat-player.md)
- [Phase 05: Combat Enemy](phase-05-combat-enemy.md)
- [Phase 06: Input Feature](phase-06-input.md)
- [Phase 07: Animation/VFX](phase-07-animation-vfx.md)
- [Phase 08: Battle Feature](phase-08-battle.md)
- [Phase 09: UI](phase-09-ui.md)
- [Phase 10: Integration](phase-10-integration.md)

---

## Verification Gates

Mỗi phase phải pass:
1. **Compilation:** Zero errors, zero warnings
2. **Unit Tests:** Có tests cho services (nếu applicable)
3. **Code Review:** SOLID compliance check
4. **Feature Checklist:** Verify against gameplay docs

---

## Commands

```bash
# Start implementation
/t1k:cook plans/20260407-mechangelion-rebuild/

# After each phase
/t1k:test
/t1k:review
```
