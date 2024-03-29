﻿using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class RowStamper : MonoBehaviour
{
    [Tooltip("The time before the first row is stamped at the top of the wheel")]
    [SerializeField] float delay = 0f;

    [Tooltip("The time (in seconds) between each row.")]
    [SerializeField] float spacing = 2f;
    private Wheel wheel;
    int generationCount;
    private Level level;
    private int rowIndex;

    void Start()
    {
        if (!Levels.gameJustStarted) Init();
    }

    public void Init()
    {
        wheel = FindObjectOfType<Wheel>();
        float startingSpeed = wheel.speed;
        level = Levels.GetLevel();

        IEnumerable<ObstacleRow> scheduledRows =
            level.Rows.Where((row, i) => i == 0 || !level.Rows[i - 1].IsParent);

        for (int i = 0; i < scheduledRows.Count(); i++)
        {
            float speedModifier = 1 / (wheel.speed / startingSpeed);
            float invokeAfterSeconds = i * spacing * speedModifier + delay;
            Invoke("StampHierarchy", invokeAfterSeconds);
        }
    }

    private void StampHierarchy()
    {
        bool offset = rowIndex > 0 && level.Rows[rowIndex - 1].OffsetChild;

        Stamp(level.Rows[rowIndex], offset);

        var thisRow = level.Rows[rowIndex++];

        if (thisRow.IsParent)
        {
            if (offset)
            {
                generationCount++;
            }
            StampHierarchy();
        }
        else
        {
            generationCount = 0;
        }
    }

    private void Stamp(ObstacleRow row, bool offset)
    {
        if (row.Obstacles.Length != Constants.OBSTACLES_PER_ROW)
            throw new Exception($"Must be {Constants.OBSTACLES_PER_ROW} obstacles per row.");

        for (int i = 0; i < row.Obstacles.Length; i++)
        {
            if (row.Obstacles[i] == ObstacleCode.Nothing) continue;

            GameObject prefab =
                Resources.Load($"Obstacles/{row.Obstacles[i].ToString()}") as GameObject;

            GameObject obstacle = Instantiate(prefab, transform.position, transform.rotation);

            obstacle.transform.SetParent(wheel.gameObject.transform, true);

            Vector3 pos = obstacle.transform.position;
            pos.x = GetObstacleXPosition(i);

            //bool useCurrentGeneration = offset || rowIndex == 0 || !level.Rows[rowIndex - 1].IsParent;
            //int generation = useCurrentGeneration ? generationCount : generationCount - 1;

            float rotationDegrees = -generationCount / Constants.WHEEL_RADIUS * 90f;
            float t = -rotationDegrees * (float)Math.PI / 180f;
            pos.y = Constants.WHEEL_RADIUS * Mathf.Cos(t);
            pos.z = -Constants.WHEEL_RADIUS * Mathf.Sin(t);
            obstacle.transform.Rotate(rotationDegrees, 0, 0);


            obstacle.transform.position = pos;
        }
    }

    private float GetObstacleXPosition(int index)
    {
        float start = -Constants.WHEEL_WIDTH * .5f;
        float obstacleLaneWidth = Constants.WHEEL_WIDTH / Constants.OBSTACLES_PER_ROW;
        float laneOffset = .5f * obstacleLaneWidth;
        return start + index * obstacleLaneWidth + laneOffset;
    }
}
