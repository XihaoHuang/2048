using TMPro;
using UnityEngine;

public class Block : MonoBehaviour {
    public int value;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private TextMeshPro Text;
    public void Init(BlockType type) {
        value = type.value;
        _renderer.color = type.color;
        Text.text = type.value.ToString();
    }
}
