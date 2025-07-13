using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

namespace Bundos.MovingPlatforms
{
    public enum WaypointPathType
    {
        Closed,
        Open
    }

    public enum WaypointBehaviorType
    {
        Loop,
        PingPong
    }

    public class PlatformController : MonoBehaviour
    {
        [HideInInspector]
        public List<Vector3> waypoints = new List<Vector3>();
    
        [Header("Editor Settings")]
        public float handleRadius = .5f;
        public Vector2 snappingSettings = new Vector2(.1f, .1f);
        public Color gizmoDeselectedColor = Color.blue;

        [Header("Platform Waypoint Settings")]
        [SerializeField] private Rigidbody2D rb;
        public bool editing = false;

        public WaypointPathType pathType = WaypointPathType.Closed;
        public WaypointBehaviorType behaviorType = WaypointBehaviorType.Loop;

        public float moveSpeed = 5f; // Speed of movement
        public float stopDistance = 0.1f; // Distance to consider reaching a waypoint

        private int lastWaypointIndex = -1;
        private int currentWaypointIndex = 0;
        private int direction = 1; // 1 for forward, -1 for reverse
        public int RemainingCount = 0;
        public void SetRemainingWaypointsCount(int count)//设置剩余前进路径点数量
        {
            RemainingCount = count;
        }
        public void SetStatus(PlatformState state)
        {
            waypoints = state.waypoints;
            handleRadius = state.handleRadius;
            snappingSettings = state.snappingSettings;
            gizmoDeselectedColor = state.gizmoDeselectedColor;
            editing = state.editing;
            pathType = state.pathType;
            behaviorType = state.behaviorType;
            moveSpeed = state.moveSpeed;
            stopDistance = state.stopDistance;
            lastWaypointIndex = state.lastWaypointIndex;
            currentWaypointIndex = state.currentWaypointIndex;
            direction = state.direction;
            RemainingCount = state.RemainingCount;
            transform.position = state.position; // Restore the platform's initial position

            if (rb == null)
                rb = GetComponent<Rigidbody2D>();
        }
        public PlatformState GetStatus()
        {
            PlatformState state = new PlatformState();
            state.waypoints = waypoints;
            state.handleRadius = handleRadius;
            state.snappingSettings = snappingSettings;
            state.gizmoDeselectedColor = gizmoDeselectedColor;
            state.editing = editing;
            state.pathType = pathType;
            state.behaviorType = behaviorType;
            state.moveSpeed = moveSpeed;
            state.stopDistance = stopDistance;
            state.lastWaypointIndex = lastWaypointIndex;
            state.currentWaypointIndex = currentWaypointIndex;
            state.direction = direction;
            state.RemainingCount = RemainingCount;
            state.position = transform.position; // Save the platform's initial position

            return state;
        }
        private void Update()
        {
            if (RemainingCount == 0)
            {
                if (Vector2.Distance(transform.position, waypoints[currentWaypointIndex]) <= stopDistance)
                {
                    AudioSource audioSource = GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        // 确保音频源没有在播放
                        audioSource.Stop();
                    }
                }
                return;
            }
            if (waypoints.Count == 0)
                return;

            if (Vector2.Distance(transform.position, waypoints[currentWaypointIndex]) <= stopDistance)
            {
                RemainingCount--;
                AudioSource audioSource = GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.Play();
                }
                if (pathType == WaypointPathType.Closed)
                {
                    switch (behaviorType)
                    {
                        case WaypointBehaviorType.Loop:
                            lastWaypointIndex = currentWaypointIndex;
                            currentWaypointIndex = mod((currentWaypointIndex + direction), waypoints.Count);
                            break;
                        case WaypointBehaviorType.PingPong:
                            if ((lastWaypointIndex == 1 && currentWaypointIndex == 0 && direction < 0) || (lastWaypointIndex == waypoints.Count - 1 && currentWaypointIndex == 0 && direction > 0))
                            {
                                direction *= -1;
                            }

                            lastWaypointIndex = currentWaypointIndex;
                            currentWaypointIndex = mod((currentWaypointIndex + direction), waypoints.Count);
                            break;
                    }
                }
                else if (pathType == WaypointPathType.Open)
                {
                    switch (behaviorType)
                    {
                        case WaypointBehaviorType.Loop:
                            int nextWaypointIndex = mod((currentWaypointIndex + direction), waypoints.Count);

                            if ((lastWaypointIndex == 1 && currentWaypointIndex == 0 && direction < 0) || (lastWaypointIndex == waypoints.Count - 2 && currentWaypointIndex == waypoints.Count - 1 && direction > 0))
                            {
                                transform.position = waypoints[nextWaypointIndex];
                            }

                            lastWaypointIndex = currentWaypointIndex;
                            currentWaypointIndex = mod((currentWaypointIndex + direction), waypoints.Count);
                            break;
                        case WaypointBehaviorType.PingPong:
                            if ((lastWaypointIndex == 1 && currentWaypointIndex == 0 && direction < 0) || (lastWaypointIndex == waypoints.Count - 2 && currentWaypointIndex == waypoints.Count - 1 && direction > 0))
                            {
                                direction *= -1;
                            }

                            lastWaypointIndex = currentWaypointIndex;
                            currentWaypointIndex = mod((currentWaypointIndex + direction), waypoints.Count);
                            break;
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            MoveToWaypoint(waypoints[currentWaypointIndex]);
        }
        private void MoveToWaypoint(Vector3 waypoint)
        {
            Vector2 direction = (waypoint - transform.position).normalized;
            rb.velocity = direction * moveSpeed;
        }
        private void OnDrawGizmos()
        {
            if (IsSelected() && editing)
                return;

            if (pathType == WaypointPathType.Closed)
            {
                for (int i = 0; i < waypoints.Count; i++)
                {
                    Gizmos.color = gizmoDeselectedColor;

                    Vector3 nextPoint = waypoints[(i + 1) % waypoints.Count];
                    Gizmos.DrawLine(waypoints[i], nextPoint);

                    Gizmos.DrawSphere(waypoints[i], handleRadius / 2);
                }
            }
            else
            {
                for (int i = 0; i < waypoints.Count; i++)
                {
                    Gizmos.color = gizmoDeselectedColor;

                    Vector3 nextPoint = waypoints[(i + 1) % waypoints.Count];
                    if (i != waypoints.Count - 1)
                        Gizmos.DrawLine(waypoints[i], nextPoint);

                    Gizmos.DrawSphere(waypoints[i], handleRadius / 2);
                }
            }
        }

        private bool IsSelected()
        {
#if UNITY_EDITOR
            return UnityEditor.Selection.activeGameObject == transform.gameObject;
#else
            return false;
#endif
        }

        int mod(int x, int m)
        {
            return (x % m + m) % m;
        }
    }
}
