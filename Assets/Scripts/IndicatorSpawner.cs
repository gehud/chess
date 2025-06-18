using UnityEngine;

namespace Chess
{
    public class IndicatorSpawner : MonoBehaviour
    {
        [SerializeField]
        private GameObject indicator;
        [Space]
        [SerializeField]
        private Material availableMove;
        [SerializeField]
        private Material lastMove;
        [SerializeField]
        private Material underCheck;

        public GameObject Spawn(Indication indication, Square square)
        {
            var instance = Instantiate(indicator);

            instance.GetComponent<MeshRenderer>().material = indication switch
            {
                Indication.AvailableMove => availableMove,
                Indication.LastMove => lastMove,
                Indication.UnderCheck => underCheck,
                _ => null,
            };

            instance.transform.position = new Vector3
            {
                x = square.File,
                y = indicator.transform.position.y,
                z = square.Rank
            };

            return instance;
        }
    }
}
