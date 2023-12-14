using UnityEngine;
using Zenject;

namespace Chess {
	public class PieceViewFactoryInstaller : MonoInstaller {
		[SerializeField] private PieceViewFactory instance;

		public override void InstallBindings() {
			Container
				.Bind<IPieceViewFactory>()
				.FromInstance(instance)
				.AsSingle()
				.NonLazy();
		}
	}
}