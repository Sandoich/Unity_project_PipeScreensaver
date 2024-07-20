using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeManager : MonoBehaviour
{
    // 直管预制体，用于生成新的管道段
    public GameObject straightPipePrefab;

    // 每段管道的长度
    public float pipeLength = 2f;

    // 3D范围的长、宽、高
    public float rangeLength = 20f; // x方向长度
    public float rangeWidth = 20f; // y方向宽度
    public float rangeHeight = 20f; // z方向高度

    // 当前管道段的位置
    private Vector3 currentPos;

    // 当前管道段的方向
    private Vector3 currentDirection;

    // 存储管道位置的集合，用于检测管道是否重叠
    private HashSet<Vector3> pipePositions = new HashSet<Vector3>();

    // 当前颜色
    private Color currentColor;

    void Start()
    {
        // 起点为 (0, 0, 0)
        currentPos = Vector3.zero;

        // 随机选择一个初始方向
        currentDirection = RandomDirection();

        // 将起点位置添加到集合中
        pipePositions.Add(currentPos);

        // 随机选择一个初始颜色
        currentColor = RandomColor();

        // 创建第一个直管道
        CreateNewPipe(currentPos, currentDirection);
    }

    void Update()
    {
        // 每帧调用 GrowPipe 方法以生成新的管道段
        GrowPipe();
    }

    void GrowPipe()
    {
        // 获取下一个有效的方向和位置
        Vector3 nextPosition;
        Vector3 nextDirection = GetValidDirection(out nextPosition);

        // 如果找到有效方向，则生成新管道
        if (nextDirection != Vector3.zero)
        {
            CreateNewPipe(currentPos, nextDirection);
        }
        else
        {
            // 如果没有可用的方向，打印警告信息
            Debug.LogWarning("没有可用的方向来生成新管道");

            // 生成新管道并继续生长
            currentPos = FindNewStartPosition();
            currentDirection = RandomDirection();

            // 为新起点选择一个随机颜色
            currentColor = RandomColor();
            
            CreateNewPipe(currentPos, currentDirection);
        }
    }

    Vector3 GetValidDirection(out Vector3 nextPosition)
    {
        // 定义所有可能的方向
        List<Vector3> directions = new List<Vector3>
        {
            Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back
        };

        // 随机打乱方向列表
        Shuffle(directions);

        // 遍历所有方向，检查是否可以生成新管道
        foreach (Vector3 direction in directions)
        {
            // 计算新管道的终点位置
            nextPosition = currentPos + direction * pipeLength;

            // 检查新位置是否在边界内且没有管道
            if (IsWithinBounds(nextPosition) && !pipePositions.Contains(nextPosition))
            {
                return direction;
            }
        }

        // 如果没有可用的方向，返回 Vector3.zero
        nextPosition = Vector3.zero;
        return Vector3.zero;
    }

    void CreateNewPipe(Vector3 position, Vector3 direction)
    {
        // 计算新的管道末端位置
        Vector3 adjustedPosition = position + direction * pipeLength;

        // 计算新的管道的旋转，使其方向与当前方向一致
        Quaternion rotation = Quaternion.LookRotation(direction);

        // 创建新的管道
        GameObject newPipe = Instantiate(
            straightPipePrefab,
            position, // 使用当前位置作为起点
            rotation
        );

        // 获取所有渲染器组件
        Renderer[] renderers = newPipe.GetComponentsInChildren<Renderer>();

        // 为所有渲染器设置当前颜色
        foreach (Renderer renderer in renderers)
        {
            renderer.material.color = currentColor;
        }

        // 更新 currentPos，使其指向新管道的末端
        currentPos = adjustedPosition;

        // 将新的管道位置添加到集合中
        pipePositions.Add(currentPos);

        // 输出调试信息
        Debug.Log($"Created pipe at {currentPos} with direction {direction}");
        Debug.Log($"Current pipe end position: {adjustedPosition}");
    }

    Vector3 RandomDirection()
    {
        // 返回一个随机方向（上、下、左、右、前、后）
        int rand = Random.Range(0, 6);
        switch (rand)
        {
            case 0: return Vector3.up;
            case 1: return Vector3.down;
            case 2: return Vector3.left;
            case 3: return Vector3.right;
            case 4: return Vector3.forward;
            case 5: return Vector3.back;
        }
        return Vector3.zero;
    }

    // 随机生成一个颜色
    Color RandomColor()
    {
        return new Color(Random.value, Random.value, Random.value);
    }

    // 随机打乱列表中的元素顺序
    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            // 暂存当前元素
            T temp = list[i];

            // 随机选择一个索引
            int randomIndex = Random.Range(i, list.Count);

            // 交换当前元素和随机选择的元素
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // 检查位置是否在边界范围内
    bool IsWithinBounds(Vector3 position)
    {
        return position.x >= -rangeLength / 2 && position.x <= rangeLength / 2 &&
               position.y >= -rangeWidth / 2 && position.y <= rangeWidth / 2 &&
               position.z >= -rangeHeight / 2 && position.z <= rangeHeight / 2;
    }

    // 寻找新的起始位置
    Vector3 FindNewStartPosition()
    {
        Vector3 newPosition;
        int attempts = 0;

        // 尝试找到一个空位置
        do
        {
            newPosition = new Vector3(
                Random.Range(-rangeLength / 2, rangeLength / 2),
                Random.Range(-rangeWidth / 2, rangeWidth / 2),
                Random.Range(-rangeHeight / 2, rangeHeight / 2)
            );
            attempts++;
        } while (pipePositions.Contains(newPosition) && attempts < 100);

        if (attempts >= 100)
        {
            Debug.LogError("无法找到新的起始位置");
            return Vector3.zero;
        }

        return newPosition;
    }
}
