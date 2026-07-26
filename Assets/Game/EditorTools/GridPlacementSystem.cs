using UnityEngine;
using UnityEngine.Tilemaps;

namespace LastHope.EditorTools
{
    /// <summary>
    /// Utilities for snapping world positions to isometric grid and validating placement.
    /// Isometric grid: cell width = 1f, cell height = 0.5f (diamond shape).
    /// Per isometric-game-placement-rules.md §2: objects MUST snap to Grid.CellLayout.Isometric.
    /// </summary>
    public static class GridPlacementSystem
    {
        private const float GRID_CELL_X = 1f;
        private const float GRID_CELL_Y = 0.5f;

        /// <summary>
        /// Snap world position to nearest isometric grid cell center.
        /// </summary>
        public static Vector2 SnapToGrid(Vector2 worldPosition)
        {
            float snappedX = Mathf.Round(worldPosition.x / GRID_CELL_X) * GRID_CELL_X;
            float snappedY = Mathf.Round(worldPosition.y / GRID_CELL_Y) * GRID_CELL_Y;
            return new Vector2(snappedX, snappedY);
        }

        /// <summary>
        /// Check if placement at grid position is valid (no collision overlap with non-excluded objects).
        /// footprint = collider size (e.g., 1.5f x 0.5f for SearchPoint).
        /// Returns true if placement is clear; false if collision detected.
        /// </summary>
        public static bool ValidatePlacement(Vector2 gridPosition, Vector2 footprint, Collider2D[] excludeColliders = null)
        {
            Collider2D[] results = Physics2D.OverlapBoxAll(
                gridPosition,
                footprint,
                0f  // rotation for isometric
            );

            foreach (var collider in results)
            {
                if (collider.isTrigger) continue;  // ignore triggers
                if (excludeColliders != null && System.Array.Exists(excludeColliders, c => c == collider))
                    continue;  // ignore excluded colliders

                return false;  // collision found
            }

            return true;  // no collision
        }

        /// <summary>
        /// Check if there is clearance (empty cells) around placement position.
        /// radiusCells: number of grid cells to check in all directions.
        /// </summary>
        public static bool HasClearance(Vector2 gridPosition, float radiusCells)
        {
            float clearanceSize = radiusCells * GRID_CELL_X;
            return ValidatePlacement(gridPosition, new Vector2(clearanceSize, clearanceSize));
        }

        /// <summary>
        /// Check if a path from 'from' to 'to' is walkable (no solid collider blocking).
        /// Simple implementation: checks midpoint and endpoints. Future: proper pathfinding.
        /// </summary>
        public static bool IsPathFree(Vector2 from, Vector2 to, Collider2D[] excludeColliders = null)
        {
            Vector2 midpoint = (from + to) / 2f;

            // Check start, midpoint, and end
            if (!ValidatePlacement(from, Vector2.one * 0.5f, excludeColliders)) return false;
            if (!ValidatePlacement(midpoint, Vector2.one * 0.5f, excludeColliders)) return false;
            if (!ValidatePlacement(to, Vector2.one * 0.5f, excludeColliders)) return false;

            return true;
        }
    }
}
