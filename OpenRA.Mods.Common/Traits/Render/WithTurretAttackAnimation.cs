#region Copyright & License Information
/*
 * Copyright 2007-2018 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits.Render
{
	public class WithTurretAttackAnimationInfo : ITraitInfo, Requires<WithSpriteTurretInfo>, Requires<ArmamentInfo>
	{
		[Desc("Armament name")]
		public readonly string Armament = "primary";

		[Desc("Turret name")]
		public readonly string Turret = "primary";

		[Desc("Displayed while attacking.")]
		[SequenceReference] public readonly string Sequence = null;

		[Desc("Delay in ticks before animation starts, either relative to attack preparation or attack.")]
		public readonly int Delay = 0;

		[Desc("Should the animation be delayed relative to preparation or actual attack?")]
		public readonly AttackDelayType DelayRelativeTo = AttackDelayType.Preparation;

		public object Create(ActorInitializer init) { return new WithTurretAttackAnimation(init, this); }
	}

	public class WithTurretAttackAnimation : ITick, INotifyAttack
	{
		readonly WithTurretAttackAnimationInfo info;
		readonly WithSpriteTurret wst;

		int tick;

		public WithTurretAttackAnimation(ActorInitializer init, WithTurretAttackAnimationInfo info)
		{
			this.info = info;
			wst = init.Self.TraitsImplementing<WithSpriteTurret>()
				.Single(st => st.Info.Turret == info.Turret);
		}

		void PlayAttackAnimation(Actor self)
		{
			if (!string.IsNullOrEmpty(info.Sequence))
				wst.PlayCustomAnimation(self, info.Sequence);
		}

		void INotifyAttack.Attacking(Actor self, Target target, Armament a, Barrel barrel)
		{
			if (a.Info.Name != info.Armament)
				return;

			if (info.DelayRelativeTo == AttackDelayType.Attack)
			{
				if (info.Delay > 0)
					tick = info.Delay;
				else
					PlayAttackAnimation(self);
			}
		}

		void INotifyAttack.PreparingAttack(Actor self, Target target, Armament a, Barrel barrel)
		{
			if (a.Info.Name != info.Armament)
				return;

			if (info.DelayRelativeTo == AttackDelayType.Preparation)
			{
				if (info.Delay > 0)
					tick = info.Delay;
				else
					PlayAttackAnimation(self);
			}
		}

		void ITick.Tick(Actor self)
		{
			if (info.Delay > 0 && --tick == 0)
				PlayAttackAnimation(self);
		}
	}
}
