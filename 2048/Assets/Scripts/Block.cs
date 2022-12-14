using TMPro;
using UnityEngine;

public class Block : MonoBehaviour {
    public int value;
    public Node node;
    public Block MergingBlock;
    public bool Merging;
    public Vector2 Pos => transform.position;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private TextMeshPro Text;
    public void Init(BlockType type) {
        value = type.value;
        Color c = type.Color;
        _renderer.color = new Color(c.r,c.g,c.b);
        Text.text = type.value.ToString();
    }

    public void SetBlock(Node newNode) {
        if (node != null)
            node.OccupiedBlock = null;
        node = newNode;
        node.OccupiedBlock = this;

    }

    public void MergeBlock(Block blockToMergeWith) {
        MergingBlock = blockToMergeWith;

        node.OccupiedBlock=null;

        blockToMergeWith.Merging = true;
    }
    public bool CanMerge(int value) => this.value== value && !Merging && MergingBlock == null;
}
