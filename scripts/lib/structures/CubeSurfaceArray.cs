using System;
using Godot;


public struct CubeSurfaceArray<T>
{
    // save what face to map to when accessing a coordinate outside Face
    // each row represents an origin face, and the collumns represent the mappings, starting from the top, then right, bottom and left
    private static readonly int[] maps = {
        0, 3, 1, 2, // front,   0
        0, 2, 1, 3,  // back,    1
        5, 3, 4, 2, // top,     2
        4, 3, 5, 2, // bottom,  3
        0, 4, 1, 5, // left,    4
        0, 5, 1, 4 // right,   5
    };

    T[] data;
    int size;
    int sizeSq;

    public int Size => size;

    public CubeSurfaceArray(int size)
    {
        this.data = new T[6 * size * size];
        this.size = size;
        this.sizeSq = size * size;
    }

    public CubeSurfaceArray(T[] data)
    {
        this.data = data;
        this.size = data.Length;
        this.sizeSq = this.size * this.size;
    }

    public T Get(int f, int x, int y)
    {
        return data[f * sizeSq + x * size + y];
    }

    public void Set(T val, int f, int x, int y)
    {
        data[f * sizeSq + x * size + y] = val;
    }
}
