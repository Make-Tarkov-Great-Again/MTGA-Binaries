using Comfort.Common;
using EFT;
using UnityEngine;


namespace MTGA.Spawners.Grenades
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
			this.player = Singleton<GameWorld>.Instance.MainPlayer;
			ShotFactory.Init(this.player);
			((MonoBehaviour)this).InvokeRepeating("Tick", ExplosiveGrenadeSpawner.delay, ExplosiveGrenadeSpawner.rate);
		}

		private void Tick()
		{
			Vector3 Position = ((Component)this).transform.position;
			Position.x += UnityEngine.Random.Range(0f - ExplosiveGrenadeSpawner.range, ExplosiveGrenadeSpawner.range);
			Position.z += UnityEngine.Random.Range(0f - ExplosiveGrenadeSpawner.range, ExplosiveGrenadeSpawner.range);
			Position.y += 300f;
			ShotFactory.MakeShot(this.bullet, Position, Vector3.down, 1f);
			if (--this.Count <= 0)
			{
				UnityEngine.Object.Destroy(this);
			}
		}
	}
}
