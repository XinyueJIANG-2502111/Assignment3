using UnityEngine;

public class ScreenBoundaryAdapter : MonoBehaviour
{
    // 在 Inspector 中拖入对应的墙壁物体
    // Drag and drop the corresponding wall objects in the Inspector
    public Transform leftWall;
    public Transform rightWall;
    public Transform floor;

    // 墙壁的厚度 / Thickness of the walls
    public float wallThickness = 1.0f;

    void Start()
    {
        AdaptWallsToScreen();
    }

    void AdaptWallsToScreen()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        // 1. 获取屏幕左下角和右上角在世界坐标系中的位置 (Z轴设为0)
        // Get the bottom-left and top-right corners of the screen in world space
        Vector3 bottomLeft = mainCam.ScreenToWorldPoint(new Vector3(0, 0, mainCam.nearClipPlane));
        Vector3 topRight = mainCam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, mainCam.nearClipPlane));

        // 计算屏幕的中心点、宽度和高度
        // Calculate screen center, width, and height in world units
        float screenWidth = topRight.x - bottomLeft.x;
        float screenHeight = topRight.y - bottomLeft.y;
        float screenCenterY = (topRight.y + bottomLeft.y) / 2.0f;

        // 2. 配置【左墙】 / Configure [Left Wall]
        if (leftWall != null)
        {
            // 位置：正好在屏幕左边缘再往左移半个墙厚度 / Position: Just outside the left screen edge
            leftWall.position = new Vector3(bottomLeft.x - (wallThickness / 2f), screenCenterY, 0);
            // 缩放：高度和小于等于屏幕高度，宽度为墙厚度 / Scale: Match screen height, width is thickness
            leftWall.localScale = new Vector3(wallThickness, screenHeight * 1.5f, 1);
        }

        // 3. 配置【右墙】 / Configure [Right Wall]
        if (rightWall != null)
        {
            // 位置：正好在屏幕右边缘再往右移半个墙厚度 / Position: Just outside the right screen edge
            rightWall.position = new Vector3(topRight.x + (wallThickness / 2f), screenCenterY, 0);
            // 缩放：同上 / Scale: Same as left wall
            rightWall.localScale = new Vector3(wallThickness, screenHeight * 1.5f, 1);
        }

        // 4. 配置【地板】 / Configure [Floor]
        if (floor != null)
        {
            // 位置：正好在屏幕下边缘再往下移半个墙厚度 / Position: Just below the bottom screen edge
            floor.position = new Vector3(0, bottomLeft.y - (wallThickness / 2f), 0);
            // 缩放：宽度填满屏幕（多给点缓冲），高度为墙厚度 / Scale: Fill screen width, height is thickness
            floor.localScale = new Vector3(screenWidth * 1.5f, wallThickness, 1);
        }
    }
}