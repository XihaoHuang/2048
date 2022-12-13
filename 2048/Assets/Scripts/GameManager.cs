using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Random = UnityEngine.Random;
public class GameManager : MonoBehaviour {
    [SerializeField] private int width = 4;
    [SerializeField] private int height = 4;
    [SerializeField] private Node nodeprefab;
    [SerializeField] private SpriteRenderer boardPrefab;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private List<BlockType> types;

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

        switch(state) {
            case GameState.GenerateLevel:
                GenerateGrid();
                break;
            case GameState.SpawningBlocks:
                SpawnBlocks(round++ ==0 ? 2 : 1);
                break;
            case GameState.WaitingInput:
                break;
            case GameState.Moving:
                break;
            case GameState.Win:
                break;
            case GameState.Lose:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state),newState,null);
        }
    }

    void GenerateGrid() {
        nodes= new List<Node>();
        blocks= new List<Block>();
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

    }

    void SpawnBlocks(int amount) {
        var freeNodes = nodes.Where(n => n.OccupiedBlock == null).OrderBy(b => Random.value).ToList();

        foreach (var node in freeNodes.Take(amount)) {
            var block = Instantiate(blockPrefab, node.Pos, Quaternion.identity);
            block.Init(GetBlockTypeByValue(Random.value > 0.6f ? 4 :2));
        }
    }


}


[Serializable]

public struct BlockType {
    public int value;
    public Color color;
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