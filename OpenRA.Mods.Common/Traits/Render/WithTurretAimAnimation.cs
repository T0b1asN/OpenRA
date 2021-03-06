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
	public class WithTurretAimAnimationInfo : ITraitInfo, Requires<WithSpriteTurretInfo>, Requires<ArmamentInfo>, Requires<AttackBaseInfo>
	{
		[Desc("Armament name")]
		public readonly string Armament = "primary";

		[Desc("Turret name")]
		public readonly string Turret = "primary";

		[Desc("Displayed while targeting.")]
		[SequenceReference] public readonly string Sequence = null;

		[Desc("Shown while reloading.")]
		[SequenceReference(null, true)] public readonly string ReloadPrefix = null;

		public object Create(ActorInitializer init) { return new WithTurretAimAnimation(init, this); }
	}

	public class WithTurretAimAnimation : ITick
	{
		readonly WithTurretAimAnimationInfo info;
		readonly AttackBase attack;
		readonly Armament armament;
		readonly WithSpriteTurret wst;

		public WithTurretAimAnimation(ActorInitializer init, WithTurretAimAnimationInfo info)
		{
			this.info = info;
			attack = init.Self.Trait<AttackBase>();
			armament = init.Self.TraitsImplementing<Armament>()
				.Single(a => a.Info.Name == info.Armament);
			wst = init.Self.TraitsImplementing<WithSpriteTurret>()
				.Single(st => st.Info.Turret == info.Turret);
		}

		void ITick.Tick(Actor self)
		{
			if (string.IsNullOrEmpty(info.Sequence) && string.IsNullOrEmpty(info.ReloadPrefix))
				return;

			var sequence = wst.Info.Sequence;
			if (!string.IsNullOrEmpty(info.Sequence) && attack.IsAiming)
				sequence = info.Sequence;

			var prefix = (armament.IsReloading && !string.IsNullOrEmpty(info.ReloadPrefix)) ? info.ReloadPrefix : "";

			if (!string.IsNullOrEmpty(prefix) && sequence != (prefix + sequence))
				sequence = prefix + sequence;

			wst.DefaultAnimation.ReplaceAnim(sequence);
		}
	}
}
