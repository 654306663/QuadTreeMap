using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WCC.QuadTree
{
    public static class Expand
    {
        public static bool CheckBoundIsInCamera(this Bounds bound, Camera camera, float viewRatio = 1)
        {
            System.Func<Vector4, int> ComputeOutCode = (projectionPos) =>
            {
                int _code = 0;
                if (projectionPos.x < -projectionPos.w) _code |= 1;
                if (projectionPos.x > projectionPos.w) _code |= 2;
                if (projectionPos.y < -projectionPos.w) _code |= 4;
                if (projectionPos.y > projectionPos.w) _code |= 8;
                if (projectionPos.z < -projectionPos.w) _code |= 16;
                if (projectionPos.z > projectionPos.w) _code |= 32;
                return _code;
            };

            Vector4 worldPos = Vector4.one;
            int code = 63;
            for (int i = -1; i <= 1; i += 2)
            {
                for (int j = -1; j <= 1; j += 2)
                {
                    for (int k = -1; k <= 1; k += 2)
                    {
                        worldPos.x = bound.center.x + i * bound.extents.x;
                        worldPos.y = bound.center.y + j * bound.extents.y;
                        worldPos.z = bound.center.z + k * bound.extents.z;

                        code &= ComputeOutCode(camera.projectionMatrix * camera.worldToCameraMatrix * worldPos * viewRatio);
                    }
                }
            }
            return code == 0 ? true : false;
        }
    }
}