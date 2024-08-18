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

    T[,,] data;

    public int height => data.GetLength(2);
    public int width => data.GetLength(1);


    public CubeSurfaceArray(int _height, int _width)
    {
        this.data = new T[6, _width, _height];
    }

    public CubeSurfaceArray(T[,,] data)
    {
        this.data = data;
    }

    public enum Face { Front, Back, Top, Bottom, Left, Right }

    public T Get(Face face, int x, int y)
    {
        return data[(int)face, x, y];
    }

    public T Get(int f, int x, int y)
    {
        return data[f, x, y];
    }

    public T GetUnclamped(Face face, int x, int y)
    {
        return GetUnclamped((int)face, x, y);
    }

    public T GetUnclamped(int f, int x, int y)
    {

        if(x >= width)
        {
            return data[maps[f * 4 + 1], x - width, y];
        }
        else if(x < 0)
        {
            return data[maps[f * 4 + 3], x + width, y];
        }
        else if(y >= height)
        {
            return data[maps[f * 4 + 0], x, y - height];
        }
        else if(y < 0)
        {
            return data[maps[f * 4 + 2], x, y + height];
        }
        else
        {
            return data[f, x, y];
        }
    }
}
