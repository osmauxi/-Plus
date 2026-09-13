using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectGame.HotFix.Gameplay.Combat.Health;
using ProjectGame.HotFix.UI.Gameplay.HUD;

namespace ProjectGame.HotFix.Tests.GameplayUI
{
    public sealed class GameplayHUDModelTests
    {
        [Test]
        public void Synchronize_SortsLocalFirst_UsesEntityIdAndLobbyName_AndNormalizesBars()
        {
            var health = new FakeHealth();
            health.Set(700, 1, 25, 10, 100, 50);
            health.Set(900, 1, 80, 50, 100, 50);
            var roster = new[]
            {
                new HUDPlayerBinding(8, 900, "RemoteName", false),
                new HUDPlayerBinding(3, 700, "LobbyName", true),
            };
            var model = new GameplayHUDModel();
            int changes = 0;
            model.Changed += () => changes++;

            Assert.That(model.Synchronize(roster, health, null), Is.True);
            Assert.That(changes, Is.EqualTo(1));
            Assert.That(model.Players[0].Player.PlayerName, Is.EqualTo("LobbyName"));
            Assert.That(model.Players[0].Player.EntityId, Is.EqualTo(700));
            Assert.That(model.Players[0].HealthRatio, Is.EqualTo(.25f));
            Assert.That(model.Players[0].ShieldRatio, Is.EqualTo(.2f));
            Assert.That(model.Players[1].Player.ClientId, Is.EqualTo(8));
            Assert.That(model.Ammo.HasLocalPlayer, Is.True);
            Assert.That(model.Ammo.HasWeapon, Is.False);
            Assert.That(model.Synchronize(roster, health, null), Is.False, "相同值不能重复驱动动画");
        }

        [Test]
        public void Synchronize_ReportsUnavailableHealth_AndRemovesDepartedClient()
        {
            var model = new GameplayHUDModel();
            var roster = new[] { new HUDPlayerBinding(1, 11, "One", true), new HUDPlayerBinding(2, 22, "Two", false) };
            model.Synchronize(roster, null, null);
            Assert.That(model.Players, Has.Count.EqualTo(2));
            Assert.That(model.Players[0].HasHealth, Is.False);

            model.Synchronize(new[] { roster[0] }, null, null);
            Assert.That(model.Players, Has.Count.EqualTo(1));
            Assert.That(model.Players[0].Player.ClientId, Is.EqualTo(1));
        }

        [Test]
        public void PlayerHealthWireState_RoundTripsAllHudAndAuthorityFields()
        {
            var state = new HealthSnapshot(new HealthEntity(44, 9), new HealthDefinition(2, 100, 50, 3, 4),
                63, 17, true, 30, true, 12);
            var copy = new PlayerHealthWireState(state).ToSnapshot(44);
            Assert.That(copy.Entity, Is.EqualTo(state.Entity));
            Assert.That(copy.CurrentHealth, Is.EqualTo(63));
            Assert.That(copy.CurrentShield, Is.EqualTo(17));
            Assert.That(copy.Definition.MaxHealth, Is.EqualTo(100));
            Assert.That(copy.Definition.MaxShield, Is.EqualTo(50));
            Assert.That(copy.IsInvulnerable, Is.True);
            Assert.That(copy.LastDamageTick, Is.EqualTo(30));
            Assert.That(copy.Revision, Is.EqualTo(12));
        }

        private sealed class FakeHealth : IHealthStateSource
        {
            private readonly Dictionary<ulong, HealthSnapshot> _states = new();
            public event Action<HealthChange> StateChanged { add { } remove { } }
            public void Set(ulong id, uint generation, float hp, float shield, float maxHp, float maxShield)
            {
                var entity = new HealthEntity(id, generation);
                _states[id] = new HealthSnapshot(entity, new HealthDefinition(0, maxHp, maxShield),
                    hp, shield, false, 0, false, generation);
            }
            public bool TryGetEntity(ulong entityId, out HealthEntity entity)
            { if (_states.TryGetValue(entityId, out var state)) { entity = state.Entity; return true; } entity = default; return false; }
            public bool TryGetHealth(HealthEntity entity, out HealthSnapshot state) =>
                _states.TryGetValue(entity.EntityId, out state) && state.Entity == entity;
        }
    }
}
