﻿using System.Collections.Generic;

namespace MTGA.Utilities.Player.Health
{
    public class PlayerHealth
    {
        private readonly Dictionary<EBodyPart, BodyPartHealth> _health = new Dictionary<EBodyPart, BodyPartHealth>() {
            //{ EBodyPart.Common, new BodyPartHealth() },
            { EBodyPart.Head, new BodyPartHealth() },
            { EBodyPart.Chest, new BodyPartHealth() },
            { EBodyPart.Stomach, new BodyPartHealth() },
            { EBodyPart.LeftArm, new BodyPartHealth() },
            { EBodyPart.RightArm, new BodyPartHealth() },
            { EBodyPart.LeftLeg, new BodyPartHealth() },
            { EBodyPart.RightLeg, new BodyPartHealth() }
        };

        public bool IsAlive { get; set; } = true;

        public IReadOnlyDictionary<EBodyPart, BodyPartHealth> Health => _health;

        public float Hydration { get; set; }

        public float Energy { get; set; }
    }
}
