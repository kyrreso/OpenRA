#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;

namespace OpenRA.Traits
{
	public class DeveloperModeInfo : ITraitInfo
	{
		public int Cash = 20000;
		public bool FastBuild = false;
		public bool FastCharge = false;
		public bool DisableShroud = false;
		public bool PathDebug = false;
		public bool UnlimitedPower;
        public bool BuildAnywhere;

		public object Create (ActorInitializer init) { return new DeveloperMode(this); }
	}

	public class DeveloperMode : IResolveOrder, ISync
	{
		DeveloperModeInfo Info;
		[Sync] public bool FastCharge;
		[Sync] public bool AllTech;
		[Sync] public bool FastBuild;
		[Sync] public bool DisableShroud;
		[Sync] public bool PathDebug;
		[Sync] public bool UnlimitedPower;
        [Sync] public bool BuildAnywhere;

		public DeveloperMode(DeveloperModeInfo info)
		{
			Info = info;
			FastBuild = info.FastBuild;
			FastCharge = info.FastCharge;
			DisableShroud = info.DisableShroud;
			PathDebug = info.PathDebug;
			UnlimitedPower = info.UnlimitedPower;
            BuildAnywhere = info.BuildAnywhere;
		}

		public void ResolveOrder (Actor self, Order order)
		{
			if (!self.World.LobbyInfo.GlobalSettings.AllowCheats) return;

			switch(order.OrderString)
			{
				case "DevEnableTech":
					{
						AllTech ^= true;
						break;
					}
				case "DevFastCharge":
					{
						FastCharge ^= true;
						break;
					}
				case "DevFastBuild":
					{
						FastBuild ^= true;
						break;
					}
				case "DevGiveCash":
					{
						self.Trait<PlayerResources>().GiveCash(Info.Cash);
						break;
					}
				case "DevShroud":
					{
						DisableShroud ^= true;
						if (self.World.LocalPlayer == self.Owner)
							self.World.LocalShroud.Disabled = DisableShroud;
						break;
					}
				case "DevPathDebug":
					{
						PathDebug ^= true;
						break;
					}
				case "DevGiveExploration":
					{
						if (self.World.LocalPlayer == self.Owner)
							self.World.WorldActor.Trait<Shroud>().ExploreAll(self.World);
						break;
					}
				case "DevUnlimitedPower":
					{
						UnlimitedPower ^= true;
						break;
					}
                case "DevBuildAnywhere":
                    {
                        BuildAnywhere ^= true;
                        break;
                    }
				default:
					return;
			}

			Game.Debug("Cheat used: {0} by {1}"
				.F(order.OrderString, self.Owner.PlayerName));
		}
	}
}

