﻿using UnityEngine;

namespace Gyvr.Mythril2D
{
    public static class DamageDispatcher
    {
        public static bool Send(GameObject target, DamageOutputDescriptor damageOutput)
        {
            CharacterBase targetCharacter = target.GetComponent<CharacterBase>();

            if (targetCharacter != null)
            {
                GameManager.Player.CancelLooting();
                return targetCharacter.Damage(damageOutput);
            }

            return false;
        }
    }
}
