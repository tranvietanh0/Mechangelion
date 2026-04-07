# Ke hoach rebuild Mechangelion bam ref + map codebase hien tai / Ref-mapped rebuild plan

## Overview
- Muc tieu: thay the plan cu bang 1 plan bam gameplay/code cua `MechangelionCrazyGame` va map ro vao runtime shell hien tai.
- Codebase dich hien chi co bootstrap/runtime toi thieu: `Assets/Scripts/Scenes/GameLifetimeScope.cs`, `Assets/Scripts/Scenes/Main/MainSceneScope.cs`, `Assets/Scripts/Scenes/Screen/LoadingScreenView.cs`, `Assets/Scripts/StateMachines/Game/GameStateMachine.cs`, `Assets/Scripts/StateMachines/Game/States/GameHomeState.cs`, `Assets/Scripts/Models/UserLocalData.cs`.
- Ref gameplay that su nam trong: `Assets/Scripts/GameState/`, `Assets/Scripts/Player/`, `Assets/Scripts/AngryGuy/`, `Assets/Scripts/GUI/ControlButtons/`, `Assets/Scripts/Equipment/`, `Assets/Scripts/Background/`, `Assets/Scripts/Tween/`, `Assets/Scripts/VFX/`, `Assets/Scripts/PlayerPrefsController.cs`.
- Dinh huong: khong port nguyen xi singleton monolith tu ref; giu VContainer + UniTask + SignalBus + IGameAssets + screen MVP, chi port behavior can thiet.

## Review plan cu
- Dung: chia phase `core -> progression -> combat -> input -> battle -> UI -> integration` la hop ly.
- Thieu: plan cu qua greenfield, chua map chat voi entry points hien co va chua phan anh state flow that cua ref: `INIT -> PREPARE -> ACTIVE -> LEVEL_COMPLETE|LEVEL_LOST -> REVIVE|CLEANUP` trong `C:/Projects/TheOneProject/Unity/Outsources/MechangelionCrazyGame/Assets/Scripts/GameState/GameState.cs`.
- Thieu: chua phan loai ro phan nao can clone behavior, phan nao chi can clone contract.
- Quyet dinh: plan nay supersede `plans/20260407-mechangelion-rebuild/PLAN.md`.

## Ref findings
- `GameManager.cs`: god object init systems, preload addressables, state transition, startup UI, ads/IAP/income/PvP.
- `PlayerController.cs`: availability, prepare/release, punch/fire/block/dodge, damage mitigation, freeze, revive. Cac method behavior can preserve: `PrepareAttackHandR`, `PrepareAttackHandL`, `RequestReleaseAction`, `StartBlocking`, `EndBlocking`, `Dodge`, `DealDamage`.
- `EnemyController.cs`: enemy selection, addressable loading, cache, current enemy, combat FX, PVP enemy.
- `Enemy.cs`: difficulty scaling, health/damage limiters, mission hardness caps.
- `EnemyModule.cs`: detachable parts, highlight, damage percent, reload/toss hooks.
- `ControlButtonsController.cs`: combat HUD input presenter. Ref UI la scene-driven, khong duoc copy 1:1 sang screen MVP.
- `PlayerPrefsController.cs`: storage monolith; khong duoc clone nguyen file.

## Target architecture
- Giu boot flow: `0.LoadingScene -> user data load -> 1.MainScene`.
- Main gameplay flow de xuat: `HomeState -> BattlePrepareState -> BattleActiveState -> BattleLevelCompleteState/BattleLevelLostState -> HomeState`.
- Bounded contexts vua du: `Assets/Scripts/Features/Meta/`, `Assets/Scripts/Features/Combat/`, `Assets/Scripts/Features/Battle/`, `Assets/Scripts/Features/Input/`, `Assets/Scripts/Features/Presentation/`, `Assets/Scripts/UI/`.
- Khong mo rong `Core/Interfaces` qua som. Uu tien vertical slice playable truoc.

## Mapping
- `GameManager` -> `Scenes/*Scope` + `Features/Battle/*` + `Features/Meta/*` + `UI/*Presenter`
- `GameState` / `GameStateType` -> `Assets/Scripts/StateMachines/Game/States/*` + battle runtime states
- `PlayerController` -> `Assets/Scripts/Features/Combat/Player/*`
- `EnemyController` + `Enemy` -> `Assets/Scripts/Features/Combat/Enemy/*`
- `ControlButtonsController` -> `Assets/Scripts/UI/Battle/*` + `Assets/Scripts/Features/Input/*`
- `PlayerPrefsController` -> `Assets/Scripts/Features/Meta/Persistence/*`
- Tween/VFX/Background -> `Assets/Scripts/Features/Presentation/*` va `Assets/Scripts/Features/Battle/Environment/*`

## Requirements
### Functional
- M1 battle loop phai co: prepare-release attack, block timing, dodge left/right, player/enemy health, damage resolution, cooldown, battle win/loss, reward apply, save/load level-coins-cores-equip.
- Enemy M1: ordinary enemy truoc; boss/minion/small/module chi can contract va rollout tiered.
- UI M1: Home, Battle HUD, Result.

### Non-functional
- Khong sua shared framework neu chua that su can.
- Feature moi mac dinh nam trong `Assets/Scripts/`.
- Khong tao singleton gameplay moi.
- Addressables phai di qua `IGameAssets` hoac adapter noi bo.
- Pure logic phai co kha nang test doc lap.

## Implementation steps
1. Phase 0 - deprecated plan cu, chot plan nay la source of truth, khoa scope M1 chi ordinary battle + progression co ban.
2. Phase 1 - tao typed local save models va facade boc `IHandleUserDataServices`; map subset key ref: `s_id008`, `s_id010`, `s_id011cor`, selected equipment keys; hoan tat load/save thay cho `UserLocalData.Init()`.
3. Phase 2 - tao catalog/config cho enemy, equipment, mission, background, balance; trich constants punch/block/dodge/health/damage tu ref; uu tien ScriptableObject hoac BlueprintFlow.
4. Phase 3 - tao `CombatStats`, `DamageRequest`, `DamageResult`, `DamageCalculatorService`, `CooldownService`, `DefenseResolverService`, signals combat.
5. Phase 4 - tach ref `PlayerController` thanh `PlayerCombatController`, `PlayerActionRuntime`, `PlayerDefenseRuntime`, `PlayerCombatView`; preserve attack/block/dodge/revive behavior.
6. Phase 5 - tao `EnemyRuntimeBase`, `EnemySpawnerService`, `EnemySelectionService`, `EnemyDifficultyService`; M1 chi ordinary enemy; module system de hook truoc, lam that sau.
7. Phase 6 - tao `BattleCoordinator`, `BattleContext`, `BattleResult`, `RewardPayload`, `MissionSelectionContext`; tao `BattlePrepareState`, `BattleActiveState`, `BattleLevelCompleteState`, `BattleLevelLostState`, optional `BattleReviveState`; wire home -> battle -> result -> home.
8. Phase 7 - chuyen behavior `ControlButtonsController` sang battle HUD presenter/view; tach input intent khoi visual button state; support attack down/up, tap-hold block, tap dodge, cooldown visuals.
9. Phase 8 - animation adapter, VFX service, camera shake, slow-mo, background runtime loader.
10. Phase 9 - `RewardService`, `EquipmentInventoryService`, `LoadoutService`, `UpgradeService`; result screen apply reward, save progress, refresh home UI.
11. Phase 10 - deferred systems: PvP, boss-specific behaviors, small herd, detachable modules, tutorial chains, CrazyGames analytics, ads/IAP/income/patrol/liveops.

## Files to modify
- `Assets/Scripts/Scenes/GameLifetimeScope.cs`
- `Assets/Scripts/Scenes/Main/MainSceneScope.cs`
- `Assets/Scripts/Scenes/Screen/LoadingScreenView.cs`
- `Assets/Scripts/Models/UserLocalData.cs`
- `Assets/Scripts/StateMachines/Game/GameStateMachine.cs`
- `Assets/Scripts/StateMachines/Game/States/GameHomeState.cs`
- `docs/architecture-design.md`
- `docs/gameplay-documentation.md`
- `docs/project-structure-crazygame.md`

## Files to create
- `Assets/Scripts/Features/Meta/Models/PlayerProfileData.cs`
- `Assets/Scripts/Features/Meta/Models/CurrencyWalletData.cs`
- `Assets/Scripts/Features/Meta/Models/EquipmentInventoryData.cs`
- `Assets/Scripts/Features/Meta/Services/ProfileService.cs`
- `Assets/Scripts/Features/Meta/Services/CurrencyService.cs`
- `Assets/Scripts/Features/Meta/Services/EquipmentInventoryService.cs`
- `Assets/Scripts/Features/Meta/Services/LoadoutService.cs`
- `Assets/Scripts/Features/Meta/Persistence/ILocalSaveFacade.cs`
- `Assets/Scripts/Features/Meta/Persistence/LocalSaveFacade.cs`
- `Assets/Scripts/Features/Meta/Config/EquipmentCatalog.cs`
- `Assets/Scripts/Features/Meta/Config/EnemyCatalog.cs`
- `Assets/Scripts/Features/Meta/Config/MissionCatalog.cs`
- `Assets/Scripts/Features/Meta/Config/BalanceConfig.cs`
- `Assets/Scripts/Features/Combat/Models/CombatStats.cs`
- `Assets/Scripts/Features/Combat/Models/DamageRequest.cs`
- `Assets/Scripts/Features/Combat/Models/DamageResult.cs`
- `Assets/Scripts/Features/Combat/Services/DamageCalculatorService.cs`
- `Assets/Scripts/Features/Combat/Services/CooldownService.cs`
- `Assets/Scripts/Features/Combat/Services/DefenseResolverService.cs`
- `Assets/Scripts/Features/Combat/Player/PlayerCombatController.cs`
- `Assets/Scripts/Features/Combat/Player/PlayerActionRuntime.cs`
- `Assets/Scripts/Features/Combat/Player/PlayerDefenseRuntime.cs`
- `Assets/Scripts/Features/Combat/Player/PlayerCombatView.cs`
- `Assets/Scripts/Features/Combat/Enemy/EnemyRuntimeBase.cs`
- `Assets/Scripts/Features/Combat/Enemy/EnemySpawnerService.cs`
- `Assets/Scripts/Features/Combat/Enemy/EnemySelectionService.cs`
- `Assets/Scripts/Features/Combat/Enemy/EnemyDifficultyService.cs`
- `Assets/Scripts/Features/Battle/Models/BattleContext.cs`
- `Assets/Scripts/Features/Battle/Models/BattleResult.cs`
- `Assets/Scripts/Features/Battle/Models/RewardPayload.cs`
- `Assets/Scripts/Features/Battle/Services/BattleCoordinator.cs`
- `Assets/Scripts/Features/Battle/Services/RewardService.cs`
- `Assets/Scripts/Features/Battle/States/BattlePrepareState.cs`
- `Assets/Scripts/Features/Battle/States/BattleActiveState.cs`
- `Assets/Scripts/Features/Battle/States/BattleLevelCompleteState.cs`
- `Assets/Scripts/Features/Battle/States/BattleLevelLostState.cs`
- `Assets/Scripts/Features/Input/CombatInputRouter.cs`
- `Assets/Scripts/Features/Input/CombatButtonStateService.cs`
- `Assets/Scripts/Features/Presentation/Animation/CombatAnimationService.cs`
- `Assets/Scripts/Features/Presentation/VFX/CombatVfxService.cs`
- `Assets/Scripts/Features/Presentation/Camera/BattleCameraService.cs`
- `Assets/Scripts/Features/Battle/Environment/BackgroundRuntimeService.cs`
- `Assets/Scripts/UI/Home/HomeScreenView.cs`
- `Assets/Scripts/UI/Home/HomeScreenPresenter.cs`
- `Assets/Scripts/UI/Battle/BattleHudView.cs`
- `Assets/Scripts/UI/Battle/BattleHudPresenter.cs`
- `Assets/Scripts/UI/Battle/CombatActionButtonView.cs`
- `Assets/Scripts/UI/Result/BattleResultView.cs`
- `Assets/Scripts/UI/Result/BattleResultPresenter.cs`

## Testing strategy
- Unit: `DamageCalculatorService`, `CooldownService`, `DefenseResolverService`, `EnemyDifficultyService`, `RewardService`, `LocalSaveFacade`.
- Integration: loading -> main boot, home -> battle -> result -> home, enemy spawn tu catalog/addressable key, apply rewards -> save -> reload.
- Manual QA: attack tap/hold/release, block dung timing, dodge trai/phai, player/enemy death, revive, save/load, missing addressable fallback.

## Security / Performance
- Khong commit secrets khi mo phase CrazyGames/ads/IAP.
- Local save khong duoc tin tuyet doi; config/balance can tach rieng.
- Prefab battle/VFX/background can preload co chon loc; dung pooling cho FX nong.
- Khong poll UI bang `Update()` neu co the push qua signal/event.

## Risks and mitigations
- Scope creep tu ref: khoa M1 chi battle loop + progression co ban.
- Over-architecture: vertical slice playable truoc.
- UI ref khong map duoc sang MVP: chi port behavior.
- Difficulty tuning sai: gom balance vao config + tao golden-value tests.
- Addressables thieu key/prefab: audit asset truoc combat integration.

## TODO
- [ ] Deprecate plan cu va tro sang plan nay
- [ ] Audit asset/addressable keys cho M1
- [ ] Thiet ke typed local save models
- [ ] Implement meta load/save foundation
- [ ] Extract balance constants tu ref vao config
- [ ] Implement combat pure services + tests
- [ ] Implement player combat runtime
- [ ] Implement enemy runtime + difficulty
- [ ] Implement battle coordinator + states
- [ ] Implement battle HUD MVP
- [ ] Implement result screen + reward apply/save
- [ ] Run M1 playable loop QA

## Unresolved questions
- M1 co can giu `PVP`, `Boss`, `Challenge`, `Core-boosted` cung luc hay chi `Ordinary` truoc?
- Du lieu enemy/equipment nen di bang ScriptableObject, BlueprintFlow CSV, hay hybrid?
- Co can giu exact save-key compatibility voi ref de migrate save cu khong?
