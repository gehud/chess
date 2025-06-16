using UnityEngine;

namespace Chess
{
    public class IndicatorSpawner : MonoBehaviour
    {
        [SerializeField]
        private GameObject indicator;
        [Space]
        [SerializeField]
        private Material move;
        [SerializeField]
        private Material check;

        public GameObject Spawn(Indication indication, Square square)
        {
            var instance = Instantiate(indicator);

            instance.GetComponent<MeshRenderer>().material = indication switch
            {
                Indication.Move => move,
                Indication.Check => check,
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
