#nullable enable

namespace HyperCasualGame.Scripts.Features.Combat.Services
{
    using System.Collections.Generic;
    using GameFoundationCore.Scripts.DI;
    using UnityEngine;

    public sealed class CooldownService : ITickable
    {
        private readonly Dictionary<string, float> cooldownDurations = new();
        private readonly Dictionary<string, float> cooldownTimers = new();
        private readonly List<string> cooldownIds = new();

        public void RegisterCooldown(string id, float duration)
        {
            this.cooldownDurations[id] = duration;
            this.cooldownTimers[id] = 0f;
        }

        public bool IsReady(string id)
        {
            return !this.cooldownTimers.TryGetValue(id, out var timer) || timer <= 0f;
        }

        public float GetProgress(string id)
        {
            if (!this.cooldownDurations.TryGetValue(id, out var duration) || duration <= 0f)
            {
                return 1f;
            }

            if (!this.cooldownTimers.TryGetValue(id, out var timer) || timer <= 0f)
            {
                return 1f;
            }

            return 1f - (timer / duration);
        }

        public void TriggerCooldown(string id)
        {
            if (this.cooldownDurations.TryGetValue(id, out var duration))
            {
                this.cooldownTimers[id] = duration;
            }
        }

        public void ResetAll()
        {
            this.cooldownIds.Clear();
            this.cooldownIds.AddRange(this.cooldownTimers.Keys);
            foreach (var id in this.cooldownIds)
            {
                this.cooldownTimers[id] = 0f;
            }
        }

        public void Tick()
        {
            var deltaTime = Time.deltaTime;
            this.cooldownIds.Clear();
            this.cooldownIds.AddRange(this.cooldownTimers.Keys);
            foreach (var id in this.cooldownIds)
            {
                this.cooldownTimers[id] = Mathf.Max(0f, this.cooldownTimers[id] - deltaTime);
            }
        }
    }
}
