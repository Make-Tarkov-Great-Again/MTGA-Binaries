using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Comfort;
using Comfort.Common;


namespace SIT.A.Tarkov.Core.Spawners.Grenades
{
	public class GrenadeSpawner : MonoBehaviour
	{
		public static float rate = 0.5f;

		public static float range = 20f;

		public static float delay = 5f;

		private object bullet;

		private Player player;

		public virtual int Count { get; set; } = 1;

		public virtual string TemplateId { get; set; } = "5d70e500a4b9364de70d38ce";

		private void Start()
		{
			this.bullet = ShotFactory.GetBullet(TemplateId);
			this.player = Singleton<GameWorld>.Instance.RegisteredPlayers.Find((Player p) => p.IsYourPlayer);
			ShotFactory.Init(this.player);
			((MonoBehaviour)this).InvokeRepeating("Tick", ExplosiveGrenadeSpawner.delay, ExplosiveGrenadeSpawner.rate);
		}

		private void Tick()
		{
			Vector3 position = ((Component)this).transform.position;
			position.x += UnityEngine.Random.Range(0f - ExplosiveGrenadeSpawner.range, ExplosiveGrenadeSpawner.range);
			position.z += UnityEngine.Random.Range(0f - ExplosiveGrenadeSpawner.range, ExplosiveGrenadeSpawner.range);
			position.y += 300f;
			ShotFactory.MakeShot(this.bullet, position, Vector3.down, 1f);
			if (--this.Count <= 0)
			{
				UnityEngine.Object.Destroy(this);
			}
		}
	}
}
