using UnityEngine;

namespace Chess
{
    public class PieceSpawner : MonoBehaviour
    {
        [Header("Prefabs")]

        [SerializeField]
        private GameObject pawn;
        [SerializeField]
        private GameObject knight;
        [SerializeField]
        private GameObject bishop;
        [SerializeField]
        private GameObject rook;
        [SerializeField]
        private GameObject queen;
        [SerializeField]
        private GameObject king;

        [Header("Materials")]

        [SerializeField]
        private Material dark;
        [SerializeField]
        private new Material light;

        public GameObject Spawn(Piece piece)
        {
            var prefab = piece.Figure switch
            {
                Figure.Pawn => pawn,
                Figure.Knight => knight,
                Figure.Bishop => bishop,
                Figure.Rook => rook,
                Figure.Queen => queen,
                Figure.King => king,
                _ => null
            };

            var material = piece.Color switch
            {
                Color.Black => dark,
                Color.White => light,
                _ => null
            };

            var view = Instantiate(prefab);
            view.GetComponent<MeshRenderer>().material = material;

            if (piece.Color == Color.Black)
            {
                view.transform.Rotate(Vector3.up, 180f);
            }

            return view;
        }
    }
}
