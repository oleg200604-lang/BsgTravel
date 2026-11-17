using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem ;

public class PlayerMoveScr : MonoBehaviour
{
    [Header("Посилання")]
    [SerializeField] private Chapter chapter;
    [SerializeField] private Tilemap tilemap;        // Tilemap з гекс-сіткою
    [SerializeField] private Camera cam;

    [Header("Налаштування")]
    [SerializeField] private float moveSpeed = 2f;   // юніти на секунду
    [SerializeField] private bool pointyTop = true;  // Hex Layout: Pointy Top чи Flat Top
    [SerializeField] private bool freeStraightLine = true; // true: рух строго по прямій, false: по центрах гексів
    private bool isMoving;
    private Coroutine moveRoutine;

    // Сусіди для "odd-r" (pointy-top, непарні рядки зсунуті вправо)
    private static readonly Vector3Int[] evenRowDirs = { new Vector3Int( 1,  0, 0), new Vector3Int(-1,  0, 0), new Vector3Int( 0,  1, 0), new Vector3Int(-1,  1, 0), new Vector3Int( 0, -1, 0), new Vector3Int(-1, -1, 0),};
    private static readonly Vector3Int[] oddRowDirs = { new Vector3Int( 1,  0, 0), new Vector3Int(-1,  0, 0), new Vector3Int( 1,  1, 0), new Vector3Int( 0,  1, 0), new Vector3Int( 1, -1, 0), new Vector3Int( 0, -1, 0),};

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 world = cam.ScreenToWorldPoint(screenPos);
            world.z = 0f;

            Vector3Int target = tilemap.WorldToCell(world);
            Vector3Int start = tilemap.WorldToCell(transform.position);

            if (!tilemap.HasTile(target)) return;   // клік повз тайли
            if (target == start) return;

            List<Vector3Int> path = FindStraightPath(start, target) ?? FindPath(start, target);
            if (path == null) return;               // шляху нема

            if (moveRoutine != null) StopCoroutine(moveRoutine);
            moveRoutine = StartCoroutine(FollowPath(path));
        }
    }

    private IEnumerator FollowPath(List<Vector3Int> path)
    {
        isMoving = true;

        foreach (Vector3Int cell in path)
        {
            Vector3 targetPos = tilemap.GetCellCenterWorld(cell);
            targetPos.z = transform.position.z;

            while ((transform.position - targetPos).sqrMagnitude > 0.0001f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = targetPos;
        }

        isMoving = false;
    }

    // ---------- Найпряміший ланцюжок гексів від A до B ----------
    private List<Vector3Int> FindStraightPath(Vector3Int start, Vector3Int goal)
    {
        int n = HexDistance(start, goal);
        if (n == 0) return null;

        Vector3 cs = OffsetToCube(start);
        Vector3 cg = OffsetToCube(goal);

        // крихітний зсув, щоб на межі двох гексів вибір був стабільним
        Vector3 eps = new Vector3(1e-4f, 2e-4f, -3e-4f);
        cs += eps;
        cg += eps;

        var line = new List<Vector3Int>(n);
        for (int i = 1; i <= n; i++)
        {
            Vector3Int cell = CubeToOffset(CubeRound(Vector3.Lerp(cs, cg, (float)i / n)));
            if (!IsWalkable(cell)) return null;   // перешкода: запасний варіант BFS
            line.Add(cell);
        }
        return line;
    }

    private int HexDistance(Vector3Int a, Vector3Int b)
    {
        Vector3 ca = OffsetToCube(a), cb = OffsetToCube(b);
        return Mathf.RoundToInt(Mathf.Max(
            Mathf.Abs(ca.x - cb.x), Mathf.Abs(ca.y - cb.y), Mathf.Abs(ca.z - cb.z)));
    }

    // odd-r offset -> cube (x = q, y = -q-r, z = r)
    private Vector3 OffsetToCube(Vector3Int c)
    {
        int q = c.x - (c.y - (c.y & 1)) / 2;
        int r = c.y;
        return new Vector3(q, -q - r, r);
    }

    private Vector3Int CubeToOffset(Vector3Int cube)
    {
        int r = cube.z;
        int col = cube.x + (r - (r & 1)) / 2;
        return new Vector3Int(col, r, 0);
    }

    private Vector3Int CubeRound(Vector3 c)
    {
        int rx = Mathf.RoundToInt(c.x);
        int ry = Mathf.RoundToInt(c.y);
        int rz = Mathf.RoundToInt(c.z);

        float dx = Mathf.Abs(rx - c.x);
        float dy = Mathf.Abs(ry - c.y);
        float dz = Mathf.Abs(rz - c.z);

        if (dx > dy && dx > dz) rx = -ry - rz;
        else if (dy > dz) ry = -rx - rz;
        else rz = -rx - ry;

        return new Vector3Int(rx, ry, rz);
    }
    // ---------- Пошук шляху (BFS) ----------
    private List<Vector3Int> FindPath(Vector3Int start, Vector3Int goal)
    {
        var queue = new Queue<Vector3Int>();
        var cameFrom = new Dictionary<Vector3Int, Vector3Int>();

        queue.Enqueue(start);
        cameFrom[start] = start;

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();
            if (current == goal) break;

            foreach (Vector3Int next in GetNeighbors(current))
            {
                if (cameFrom.ContainsKey(next)) continue;
                if (!IsWalkable(next)) continue;

                cameFrom[next] = current;
                queue.Enqueue(next);
            }
        }

        if (!cameFrom.ContainsKey(goal)) return null;

        // Відновлюємо шлях з кінця до початку
        var path = new List<Vector3Int>();
        Vector3Int step = goal;
        while (step != start)
        {
            path.Add(step);
            step = cameFrom[step];
        }
        path.Reverse();
        return path;
    }

    private IEnumerable<Vector3Int> GetNeighbors(Vector3Int cell)
    {
        Vector3Int[] dirs = (cell.y & 1) == 0 ? evenRowDirs : oddRowDirs;
        foreach (var d in dirs)
            yield return cell + d;
    }

    private bool IsWalkable(Vector3Int cell)
    {
        // Тут можна додати перевірку перешкод (інший Tilemap, список заблокованих тощо)
        return tilemap.HasTile(cell);
    }
}