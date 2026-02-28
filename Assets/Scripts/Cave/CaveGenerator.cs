using UnityEngine;
using System.Collections.Generic;

public class CaveGenerator : MonoBehaviour
{
    [Header("맵 기본 설정")]
    [SerializeField] private int baseWidth = 20;      // 1층 기준 가로 타일 수
    [SerializeField] private int baseHeight = 14;     // 1층 기준 세로 타일 수
    [SerializeField] private int maxWidth = 40;       // 맵 최대 가로 크기
    [SerializeField] private int maxHeight = 28;      // 맵 최대 세로 크기
    [SerializeField] private float wallChance = 0.45f; // 초기 벽 생성 확률 45%
    [SerializeField] private int automataSteps = 5;   // Cellular Automata 반복 횟수

    [Header("타일 프리팹")]
    [SerializeField] private GameObject wallTilePrefab;  // 벽 타일 프리팹
    [SerializeField] private GameObject floorTilePrefab; // 바닥 타일 프리팹
    [SerializeField] private GameObject stairTilePrefab; // 계단(출구) 타일 프리팹

    [Header("타일 크기")]
    [SerializeField] private float tileSize = 1f; // 한 타일의 유니티 단위 크기

    // 현재 맵 크기 (층수에 따라 변함)
    private int currentWidth;
    private int currentHeight;

    // 맵 데이터: true = 벽, false = 바닥
    private bool[,] map;

    // 생성된 타일 오브젝트 목록 (층 이동 시 전부 삭제용)
    private List<GameObject> spawnedTiles = new List<GameObject>();

    // 입구/출구 위치 (플레이어 배치, 계단 생성용)
    public Vector2Int entrancePos { get; private set; }
    public Vector2Int exitPos { get; private set; }

    // 외부(GameManager)에서 층수 전달받아 맵 생성 시작
    public void GenerateFloor(int floorNumber, int seed)
    {
        // 1. 이전 층 타일 전부 삭제
        ClearMap();

        // 2. 층수에 따라 맵 크기 계산
        CalculateMapSize(floorNumber);

        // 3. 시드 기반 랜덤 초기화 (같은 시드 = 같은 맵)
        Random.InitState(seed + floorNumber);

        // 4. 맵 배열 초기화 및 Cellular Automata 실행
        InitializeMap();
        for (int i = 0; i < automataSteps; i++)
            StepAutomata();

        // 5. 입구(상단)와 출구(하단) 경로 보장
        EnsurePath();

        // 6. 타일 오브젝트 생성
        SpawnTiles();
    }

    private void ClearMap()
    {
        // 이전에 생성된 모든 타일 오브젝트 삭제
        foreach (GameObject tile in spawnedTiles)
            Destroy(tile);
        spawnedTiles.Clear();
    }

    private void CalculateMapSize(int floorNumber)
    {
        // 홀수 층마다 가로/세로 1씩 확장, 최대 크기 초과 불가
        int expansion = (floorNumber / 2); // 홀수 층 기준 확장 횟수
        currentWidth = Mathf.Min(baseWidth + expansion, maxWidth);
        currentHeight = Mathf.Min(baseHeight + expansion, maxHeight);
    }

    private void InitializeMap()
    {
        // 맵 배열 생성
        map = new bool[currentWidth, currentHeight];

        for (int x = 0; x < currentWidth; x++)
        {
            for (int y = 0; y < currentHeight; y++)
            {
                // 맵 테두리는 무조건 벽
                if (x == 0 || x == currentWidth - 1 || y == 0 || y == currentHeight - 1)
                {
                    map[x, y] = true; // 벽
                }
                else
                {
                    // 내부는 wallChance 확률로 벽 배치
                    map[x, y] = Random.value < wallChance;
                }
            }
        }
    }

    private void StepAutomata()
    {
        // Cellular Automata 1회 실행
        // 주변 8칸 중 벽이 5개 이상이면 벽, 아니면 바닥으로 변환
        bool[,] newMap = new bool[currentWidth, currentHeight];

        for (int x = 0; x < currentWidth; x++)
        {
            for (int y = 0; y < currentHeight; y++)
            {
                // 테두리는 항상 벽 유지
                if (x == 0 || x == currentWidth - 1 || y == 0 || y == currentHeight - 1)
                {
                    newMap[x, y] = true;
                    continue;
                }

                int wallCount = CountAdjacentWalls(x, y);

                // 주변 벽이 5개 이상이면 벽, 아니면 바닥
                newMap[x, y] = wallCount >= 5;
            }
        }

        map = newMap;
    }

    private int CountAdjacentWalls(int x, int y)
    {
        // 주변 8방향 + 자기 자신 포함 벽 개수 카운트
        int count = 0;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;

                // 맵 범위 밖은 벽으로 취급
                if (nx < 0 || nx >= currentWidth || ny < 0 || ny >= currentHeight)
                    count++;
                else if (map[nx, ny])
                    count++;
            }
        }

        return count;
    }

    private void EnsurePath()
{
    // 입구: 상단에서 아래로 내려오면서 첫 번째 바닥 타일 찾기
    entrancePos = FindFloorFromTop();

    // 출구: 하단에서 위로 올라오면서 첫 번째 바닥 타일 찾기
    exitPos = FindFloorFromBottom();

    // 입구~출구 사이 강제 통로 뚫기
    CarvePath(entrancePos, exitPos);

    // 입구/출구 주변 2x2 공간 확보 (플레이어가 끼지 않도록)
    ClearArea(entrancePos);
    ClearArea(exitPos);

    Debug.Log($"[Cave] 입구: {entrancePos}, 출구: {exitPos}");
}

private Vector2Int FindFloorFromTop()
{
    // 상단 행부터 아래로 내려오면서 바닥 타일 탐색
    for (int y = currentHeight - 2; y >= currentHeight / 2; y--)
    {
        for (int x = 1; x < currentWidth - 1; x++)
        {
            if (!map[x, y])
                return new Vector2Int(x, y);
        }
    }

    // 못 찾으면 중앙 상단 강제 생성
    int cx = currentWidth / 2;
    int cy = currentHeight - 2;
    map[cx, cy] = false;
    return new Vector2Int(cx, cy);
}

private Vector2Int FindFloorFromBottom()
{
    // 하단 행부터 위로 올라오면서 바닥 타일 탐색
    for (int y = 1; y <= currentHeight / 2; y++)
    {
        for (int x = 1; x < currentWidth - 1; x++)
        {
            if (!map[x, y])
                return new Vector2Int(x, y);
        }
    }

    // 못 찾으면 중앙 하단 강제 생성
    int cx = currentWidth / 2;
    int cy = 1;
    map[cx, cy] = false;
    return new Vector2Int(cx, cy);
}

private void ClearArea(Vector2Int center)
{
    // 중심 주변 2x2 영역을 바닥으로 만들어서 공간 확보
    for (int dx = -1; dx <= 1; dx++)
    {
        for (int dy = -1; dy <= 1; dy++)
        {
            int x = Mathf.Clamp(center.x + dx, 1, currentWidth - 2);
            int y = Mathf.Clamp(center.y + dy, 1, currentHeight - 2);
            map[x, y] = false;
        }
    }
}


    private void CarvePath(Vector2Int from, Vector2Int to)
    {
        // 입구에서 출구까지 L자 형태로 통로를 강제로 뚫음
        // 먼저 가로 방향으로 이동 후 세로 방향으로 이동
        Vector2Int current = from;

        // 가로 이동 (x축 맞추기)
        while (current.x != to.x)
        {
            current.x += (to.x > current.x) ? 1 : -1;
            map[current.x, current.y] = false;       // 통로
            // 최소 2타일 너비 보장
            if (current.y + 1 < currentHeight - 1)
                map[current.x, current.y + 1] = false;
        }

        // 세로 이동 (y축 맞추기)
        while (current.y != to.y)
        {
            current.y += (to.y > current.y) ? 1 : -1;
            map[current.x, current.y] = false;       // 통로
            // 최소 2타일 너비 보장
            if (current.x + 1 < currentWidth - 1)
                map[current.x + 1, current.y] = false;
        }
    }

    private void SpawnTiles()
    {
        for (int x = 0; x < currentWidth; x++)
        {
            for (int y = 0; y < currentHeight; y++)
            {
                // 타일의 월드 좌표 계산
                Vector3 worldPos = new Vector3(x * tileSize, y * tileSize, 0);

                GameObject prefabToUse;

                if (map[x, y])
                {
                    // 벽 타일
                    prefabToUse = wallTilePrefab;
                }
                else if (new Vector2Int(x, y) == exitPos)
                {
                    // 출구(계단) 타일
                    prefabToUse = stairTilePrefab;
                }
                else
                {
                    // 바닥 타일
                    prefabToUse = floorTilePrefab;
                }

                // 프리팹이 없으면 스킵
                if (prefabToUse == null) continue;

                // 타일 생성 후 목록에 추가
                GameObject tile = Instantiate(prefabToUse, worldPos, Quaternion.identity, transform);
                spawnedTiles.Add(tile);
            }
        }
    }

    // 외부에서 특정 좌표가 벽인지 바닥인지 확인할 때 사용 (충돌 처리용)
    public bool IsWall(int x, int y)
    {
        if (x < 0 || x >= currentWidth || y < 0 || y >= currentHeight)
            return true; // 범위 밖은 벽으로 취급
        return map[x, y];
    }

    // 타일 좌표 → 월드 좌표 변환 (플레이어 배치용)
    public Vector3 TileToWorld(Vector2Int tilePos)
    {
        return new Vector3(tilePos.x * tileSize, tilePos.y * tileSize, 0);
    }
}