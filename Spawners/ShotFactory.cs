using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MTGA.Core.Spawners
{
    public class ShotFactory
	{
		private static BallisticsCalculator ballisticsCalculator;

		private static MethodInfo methodShoot;

		private static Player player;

		private static Weapon weapon;

		private static MethodBase methodCreateShot;

		public static object MakeShot(object ammo, Vector3 shotPosition, Vector3 shotDirection, float speedFactor)
		{
			object obj = ShotFactory.methodCreateShot.Invoke(ShotFactory.ballisticsCalculator, new object[8]
			{
			ammo,
			shotPosition,
			shotDirection,
			0,
			ShotFactory.player,
			ShotFactory.weapon,
			speedFactor,
			0
			});
			ShotFactory.methodShoot.Invoke(ShotFactory.ballisticsCalculator, new object[1] { obj });
			return obj;
		}

		public static object GetBullet(string tid)
		{
			return ItemFactory.CreateItem(Guid.NewGuid().ToString("N").Substring(0, 24), tid);
		}

		public static void Init(Player player)
		{
			if (ShotFactory.ballisticsCalculator == null)
			{
				ShotFactory.ballisticsCalculator = Singleton<GameWorld>.Instance._sharedBallisticsCalculator;
			}
			if (null == ShotFactory.methodShoot)
			{
				ShotFactory.methodShoot = ((object)ShotFactory.ballisticsCalculator).GetType().GetMethod("Shoot");
			}
			if (null == ShotFactory.methodCreateShot)
			{
				ShotFactory.methodCreateShot = ((object)ShotFactory.ballisticsCalculator).GetType().GetMethod("CreateShot");
			}
			ShotFactory.player = player;
			if (ShotFactory.weapon == null)
			{
				ShotFactory.weapon = (Weapon)ItemFactory.CreateItem(Guid.NewGuid().ToString("N").Substring(0, 24), "5d52cc5ba4b9367408500062");
			}
		}
	}
}
