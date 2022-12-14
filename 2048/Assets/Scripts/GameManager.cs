using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Random = UnityEngine.Random;
using Unity.VisualScripting;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;

public class GameManager : MonoBehaviour {
    [SerializeField] private int width = 4;
    [SerializeField] private int height = 4;
    [SerializeField] private Node nodeprefab;
    [SerializeField] private SpriteRenderer boardPrefab;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private List<BlockType> types;
    [SerializeField] private int WinCodition = 2048;
    [SerializeField] private GameObject wingame, losegame;

    private float traveltime = 0.2f;

    private List<Node> nodes;
    private List<Block> blocks;

    private GameState state;
    private int round;
    private BlockType GetBlockTypeByValue(int value) => types.First(t => t.value == value);
    // Start is called before the first frame update
    void Start() {
        ChangeState(GameState.GenerateLevel);
    }
    private void ChangeState(GameState newState) {
        state = newState;

        switch (state) {
            case GameState.GenerateLevel:
                GenerateGrid();
                break;
            case GameState.SpawningBlocks:
                SpawnBlocks(round++ == 0 ? 2 : 1);
                break;
            case GameState.WaitingInput:
                break;
            case GameState.Moving:
                break;
            case GameState.Win:
                    wingame.SetActive(true);
                break;
            case GameState.Lose:
                    losegame.SetActive(true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), newState, null);
        }
    }

    void GenerateGrid() {
        round = 0;
        nodes = new List<Node>();
        blocks = new List<Block>();
        wingame.SetActive(false);
        losegame.SetActive(false);
        for (int row = 0; row < height; row++) {
            for (int col = 0; col < width; col++) {
                var node = Instantiate(nodeprefab, new Vector2(row, col), Quaternion.identity);
                nodes.Add(node);
            }
        }

        var center = new Vector2((float)width / 2 - 0.5f, (float)height / 2 - 0.5f);

        var board = Instantiate(boardPrefab, center, Quaternion.identity);

        board.size = new Vector2(width, height);

        Camera.main.transform.position = new Vector3(center.x, center.y, -5);

        ChangeState(GameState.SpawningBlocks);
    }
    // Update is called once per frame
    void Update() {
        if (state != GameState.WaitingInput) return;


        if (Input.GetKeyDown(KeyCode.LeftArrow))
            Shift(Vector2.left);
        if (Input.GetKeyDown(KeyCode.RightArrow))
            Shift(Vector2.right);
        if (Input.GetKeyDown(KeyCode.UpArrow))
            Shift(Vector2.up);
        if (Input.GetKeyDown(KeyCode.DownArrow))
            Shift(Vector2.down);
    }

    void SpawnBlocks(int amount) {
        var freeNodes = nodes.Where(n => n.OccupiedBlock == null).OrderBy(b => Random.value).ToList();

        foreach (var node in freeNodes.Take(amount)) {
            SpawnBlock(node, Random.value > 0.7f ? 4 : 2);
        }
        if (blocks.Any(b => b.value == WinCodition)) {
            ChangeState(GameState.Win);
        } else if (Islose()) {
            ChangeState(GameState.Lose);
        } else {
            ChangeState(GameState.WaitingInput);
        }

    }

    void SpawnBlock(Node node,int value) {
            var block = Instantiate(blockPrefab, node.Pos, Quaternion.identity);
            block.Init(GetBlockTypeByValue(value));
            block.SetBlock(node);
            blocks.Add(block);
    }
    void Shift(Vector2 dir) {
        ChangeState(GameState.Moving);

        List<Block> orderedBlocks = blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList();
        if (dir == Vector2.up || dir == Vector2.right) orderedBlocks.Reverse();

        bool HasNodeCanShift = false;
        foreach (Block block in orderedBlocks) {
            Node next = block.node;
            do {
                block.SetBlock(next);

                Node possibleNode = GetNodeAtPosition(next.Pos + dir);
                if (possibleNode != null) {

                    if (possibleNode.OccupiedBlock != null && possibleNode.OccupiedBlock.CanMerge(block.value)) {
                        block.MergeBlock(possibleNode.OccupiedBlock);
                        HasNodeCanShift= true;
                    } else if (possibleNode.OccupiedBlock == null) {
                        next = possibleNode;
                        HasNodeCanShift = true;
                    }
                }
            } while (next != block.node);

            //block.transform.position = block.node.Pos;
            //need to download Dotween package from asset store, use above if dont want to download;
            //block.transform.DOMove(block.node.Pos, traveltime);
        }


        var sequence = DOTween.Sequence();

        foreach(Block block in orderedBlocks) {
            var movePoint = block.MergingBlock!= null ? block.MergingBlock.node.Pos : block.node.Pos;

            sequence.Insert(0, block.transform.DOMove(block.node.Pos, traveltime));
        }

        sequence.OnComplete(() => {
            var mergeBlocks = orderedBlocks.Where(b => b.MergingBlock != null).ToList();
            foreach (var block in mergeBlocks) {
                MergeBlocks(block.MergingBlock, block);
            }
                
                ChangeState(HasNodeCanShift ? GameState.SpawningBlocks : GameState.WaitingInput);
        });
    }
    bool Islose() {
        if (blocks.Count < nodes.Count) return false;
        foreach(Node node in nodes) {
            Node right = GetNodeAtPosition(node.Pos + Vector2.right);
            Node down = GetNodeAtPosition(node.Pos + Vector2.down);
            if(right!=null && node.OccupiedBlock.value == right.OccupiedBlock.value) {
                return false;
            }
            if(down!=null && node.OccupiedBlock.value == down.OccupiedBlock.value) {
                return false;
            }
        }
        return true;
    }
    Node GetNodeAtPosition(Vector2 pos) {
        return nodes.FirstOrDefault(n => n.Pos == pos);
    }

    void MergeBlocks(Block baseBlock,Block mergingBlock) {
        SpawnBlock(baseBlock.node, baseBlock.value * 2);

        RemoveBlock(mergingBlock);
        RemoveBlock(baseBlock);

    }
    void RemoveBlock(Block block) {
        blocks.Remove(block);
        Destroy(block.gameObject);
    }
}



public enum GameState {
    GenerateLevel,
    newState,
    SpawningBlocks,
    WaitingInput,
    Moving,

    Win,
    Lose
}

[Serializable]

public struct BlockType {
    public int value;
    public Color Color;
}