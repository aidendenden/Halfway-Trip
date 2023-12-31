﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Tessera
{
    public struct CubeRotation
    {
        short value;

        private CubeRotation(short value)
        {
            this.value = value;
        }

        public static CubeRotation Identity => new CubeRotation(0x012);
        public static CubeRotation ReflectX => new CubeRotation(0x812);
        public static CubeRotation ReflectY => new CubeRotation(0x092);
        public static CubeRotation ReflectZ => new CubeRotation(0x01A);
        public static CubeRotation RotateXZ => new CubeRotation(0xA10);
        public static CubeRotation RotateXY => new CubeRotation(0x902);
        public static CubeRotation RotateYZ => new CubeRotation(0x0A1);

        // Ordered by all rotations, then all refelctions
        public static IEnumerable<CubeRotation> All
        {
            get
            {
                var evenPermutations = new short[]
                {
                    0x012,
                    0x120,
                    0x201,
                };
                var oddPermutations = new short[]
                {
                    0x021,
                    0x102,
                    0x210,
                };
                var evenReflections = new short[]
                {
                    0x000,
                    0x088,
                    0x808,
                    0x880,
                };
                var oddReflections = new short[]
                {
                    0x008,
                    0x080,
                    0x800,
                    0x888,
                };

                foreach (var r in evenReflections)
                {
                    foreach (var p in evenPermutations)
                    {
                        yield return new CubeRotation((short)(p + r));
                    }
                }
                foreach (var r in oddReflections)
                {
                    foreach (var p in oddPermutations)
                    {
                        yield return new CubeRotation((short)(p + r));
                    }
                }

                foreach (var r in evenReflections)
                {
                    foreach (var p in oddPermutations)
                    {
                        yield return new CubeRotation((short)(p + r));
                    }
                }
                foreach (var r in oddReflections)
                {
                    foreach (var p in evenPermutations)
                    {
                        yield return new CubeRotation((short)(p + r));
                    }
                }
            }
        }

        internal Matrix4x4 ToMatrix()
        {
            Vector4 GetCol(int i)
            {
                var sign = (i & 8) == 0 ? 1 : -1;
                var row = i & 3;
                return new Vector4(
                    sign * (row == 0 ? 1 : 0),
                    sign * (row == 1 ? 1 : 0),
                    sign * (row == 2 ? 1 : 0)
                    );
            }
            return new Matrix4x4(
                GetCol(value >> 8),
                GetCol(value >> 4),
                GetCol(value >> 0),
                new Vector4(0, 0, 0, 1)
            );
        }


        internal MatrixInt3x3 ToMatrixInt()
        {
            Vector3Int GetCol(int i)
            {
                var sign = (i & 8) == 0 ? 1 : -1;
                var row = i & 3;
                return new Vector3Int(
                    sign * (row == 0 ? 1 : 0),
                    sign * (row == 1 ? 1 : 0),
                    sign * (row == 2 ? 1 : 0)
                    );
            }
            return new MatrixInt3x3
            {
                col1 = GetCol(value >> 8),
                col2 = GetCol(value >> 4),
                col3 = GetCol(value >> 0),
            };
        }

        internal static CubeRotation FromMatrixInt(MatrixInt3x3 matrix)
        {
            int GetCol(Vector3Int c)
            {
                if(c.x != 0)
                {
                    return c.x > 0 ? 0 : 8;
                }
                else if(c.y != 0)
                {
                    return c.y > 0 ? 1 : 9;
                }
                else
                {
                    return c.z > 0 ? 2 : 10;
                }
            }
            return new CubeRotation((short)(
                (GetCol(matrix.col1) << 8) |
                (GetCol(matrix.col2) << 4) |
                (GetCol(matrix.col3) << 0)
                ));
        }

        public bool IsReflection
        {
            get
            {
                var isOddPermutation = ((value & 3) + ((value & (3 << 4)) >> 3)) % 3 != 1;
                return isOddPermutation ^ ((value & (1 << 3)) != 0) ^ ((value & (1 << 7)) != 0) ^ ((value & (1 << 11)) != 0);
            }
        }

        public CubeRotation Invert()
        {
            // TODO: Do fancy bitwise formula
            return FromMatrixInt(ToMatrixInt().T);
        }

        public override bool Equals(object obj)
        {
            return obj is CubeRotation rotation &&
                   value == rotation.value;
        }

        public override int GetHashCode()
        {
            return -1584136870 + value.GetHashCode();
        }

        public static bool operator ==(CubeRotation a, CubeRotation b)
        {
            return a.value == b.value;
        }

        public static bool operator !=(CubeRotation a, CubeRotation b)
        {
            return a.value != b.value;
        }

        public static CubeRotation operator*(CubeRotation a, CubeRotation b)
        {
            // bit fiddling version of FromMatrix(a.ToMatrix() * b.ToMatrix())

            // Which column in a to read, for a given axis
            var xOffset = (2 - ((b.value >> 8) & 3)) << 2;
            var yOffset = (2 - ((b.value >> 4) & 3)) << 2;
            var zOffset = (2 - ((b.value >> 0) & 3)) << 2;

            var col1 = ((a.value >> xOffset) & 15) ^ ((b.value >> 8) & (1 << 3));
            var col2 = ((a.value >> yOffset) & 15) ^ ((b.value >> 4) & (1 << 3));
            var col3 = ((a.value >> zOffset) & 15) ^ ((b.value >> 0) & (1 << 3));

            return new CubeRotation((short)((col1 << 8) + (col2 << 4) + (col3 << 0)));
        }

        public static Vector3 operator *(CubeRotation r, Vector3 v)
        {
            // bit fiddling version of r.ToMatrix().Multiply(v);

            var o = new Vector3();
            o[(r.value >> 8) & 3] = v.x * ((r.value & (1 << 11)) != 0 ? -1 : 1);
            o[(r.value >> 4) & 3] = v.y * ((r.value & (1 << 7)) != 0 ? -1 : 1);
            o[(r.value >> 0) & 3] = v.z * ((r.value & (1 << 3)) != 0 ? -1 : 1);
            return o;
        }

        public static Vector3Int operator *(CubeRotation r, Vector3Int v)
        {
            // bit fiddling version of r.ToMatrix().Multiply(v);

            var o = new Vector3Int();
            o[(r.value >> 8) & 3] = v.x * ((r.value & (1 << 11)) != 0 ? -1 : 1);
            o[(r.value >> 4) & 3] = v.y * ((r.value & (1 << 7)) != 0 ? -1 : 1);
            o[(r.value >> 0) & 3] = v.z * ((r.value & (1 << 3)) != 0 ? -1 : 1);
            return o;
        }

        public static BoundsInt operator *(CubeRotation r, BoundsInt bounds)
        {
            var a = r * bounds.min;
            var b = r * (bounds.max - Vector3Int.one);
            var min = Vector3Int.Min(a, b);
            var max = Vector3Int.Max(a, b);
            return new BoundsInt(min, max - min + Vector3Int.one);
        }

        private static CubeFaceDir ToFaceDir(Vector3Int v)
        {
            if (v.x == 1)
                return CubeFaceDir.Right;
            if (v.x == -1)
                return CubeFaceDir.Left;

            if (v.y == 1)
                return CubeFaceDir.Up;
            if (v.y == -1)
                return CubeFaceDir.Down;

            if (v.z == 1)
                return CubeFaceDir.Forward;
            if (v.z == -1)
                return CubeFaceDir.Back;

            throw new Exception();
        }

        public static CubeFaceDir operator *(CubeRotation r, CubeFaceDir dir)
        {
            return ToFaceDir(r * dir.Forward());
        }

        public static implicit operator CubeRotation(CellRotation r) => new CubeRotation((short) r);

        public static implicit operator CellRotation(CubeRotation r) => (CellRotation)r.value;
    }
}
