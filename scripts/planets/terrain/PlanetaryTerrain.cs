using Godot;
using System;

[Tool]
public partial class PlanetaryTerrain : Node3D
{
    [ExportGroup("Parameters")]
    [Export] TerrainSettings terrainSettings;
    [Export] int seed = 0;

    [ExportGroup("Controls")]
    [Export] bool init = false;
    [Export] bool update = false;
    [Export] bool autoUpdate = false;
    [Export] bool printChunks = false;


    public enum ConstructMode { Dynamic, Uniform, Debug };
    [ExportGroup("Settings")]
    [Export] ConstructMode constructMode = ConstructMode.Debug;
    [Export] int chunkCount = 1;

    // UNIFORM
    [Export] int chunkResolution = 1;

    // DYNAMIC
    [Export] Node3D dynamicTarget;
    [Export] double cullingAngle = 100;
    [Export] int maxDepth = 7;
    [Export] double splitDistance = 20;

    CubeSurfaceArray<TerrainChunk> terrainChunks;

    //  This array stores the vertices of the base square, they are indexed according to this arrangement:
    //
    //             ^ y
    //             |
    //             7--------------6
    //            /|             /|
    //           / |            / |
    //          3--------------2  |
    //          |  |           |  |
    //          |  |           |  |
    //          |  |           |  |
    //          |  4-----------|--5--> x
    //          | /            | /
    //          |/             |/
    //          0--------------1
    //         /
    //        ↓ z
    //
    //  I used an ascii square i found at ’https://www.asciiart.eu/miscellaneous/boxes’ (28.07.2024), it came with the signature M.G., but since i altered the art
    //  i decided to remove it and mention the signature here instead. cheers to whoever M.G. might be ^^
    private static readonly Vector3[] baseVertices = {
        new(-1, -1, 1),     // 0
        new(1, -1, 1),      // 1
        new(1, 1, 1),       // 2
        new(-1, 1, 1),      // 3
        new(-1, -1, -1),    // 4
        new(1, -1, -1),     // 5
        new(1, 1, -1),      // 6
        new(-1, 1, -1)      // 7
    };

    // This array stores the indices for each face. These indices refer to the base vertex array above
    // The faces are indexed by a counter-clockwise order:
    //
    //   3------2
    //   |      |
    //   |      |
    //   0------1
    //
    // I have no idea if the order actually matters in the algorithms but in case it does i'm putting it here lmao
    // this is the way quads are ordered for the whole codebase
    private static readonly int[] baseQuads = {
        0, 1, 2, 3, // front face   0
        5, 4, 7, 6, // back face    1
        3, 2, 6, 7, // top face     2
        4, 5, 1, 0, // bottom face  3
        4, 0, 3, 7, // left face    4
        1, 5, 6, 2, // right face   5
    };

    private void Initialise()
    {
        if(chunkCount <= 0) return;

        Node3D terrainParent = GetChild<Node3D>(0);

        NodeHelper.RemoveChildren(terrainParent);

        TerrainChunk[,,] chunks = new TerrainChunk[6, chunkCount, chunkCount];

        Noise noise = new Noise(seed);

        double step = 1d / (double)chunkCount;

        for(int f = 0; f < 6; f++)
        {
            int k = f * 4;

            Vector3[] faceQuad = new Vector3[4];

            faceQuad[0] = baseVertices[baseQuads[k + 0]];
            faceQuad[1] = baseVertices[baseQuads[k + 1]];
            faceQuad[2] = baseVertices[baseQuads[k + 2]];
            faceQuad[3] = baseVertices[baseQuads[k + 3]];

            for(int i = 0; i < chunkCount; i++)
            {
                double tx = (double)i * step;

                for(int j = 0; j < chunkCount; j++)
                {
                    double ty = (double)j * step;

                    Vector3 dirX = faceQuad[1] - faceQuad[0];
                    Vector3 dirY = faceQuad[2] - faceQuad[1];

                    Vector3[] quad = new Vector3[4];

                    // I'll have to check if the order is right
                    quad[0] = faceQuad[0] + dirX * tx + dirY * ty;
                    quad[1] = quad[0] + dirX * step;
                    quad[2] = quad[1] + dirY * step;
                    quad[3] = quad[2] - dirX * step;


                    TerrainChunk newchunk = new TerrainChunk();

                    newchunk.Initialise(terrainSettings, (f, i, j), quad, noise);

                    terrainParent.AddChild(newchunk);
                    chunks[f, i, j] = newchunk;
                }
            }
        }

        terrainChunks = new CubeSurfaceArray<TerrainChunk>(chunks);
    }

    private void ConstructUniform()
    {
        for(int f = 0; f < 6; f++)
        {
            for(int i = 0; i < chunkCount; i++)
            {
                for(int j = 0; j < chunkCount; j++)
                {
                    terrainChunks.Get(f, i, j).ConstructUniform(chunkResolution);
                }
            }
        }
    }

    private void ConstructDynamic()
    {
        if(dynamicTarget == null)
        {
            GD.PrintErr($"Can't construct dynamically, as no target is set.");
        }

        Vector3 targetPosition = dynamicTarget.GlobalPosition - GlobalPosition;

        for(int f = 0; f < 6; f++)
        {
            for(int i = 0; i < chunkCount; i++)
            {
                for(int j = 0; j < chunkCount; j++)
                {
                    terrainChunks.Get(f, i, j).ConstructDynamic(targetPosition, maxDepth, splitDistance, Mathf.DegToRad(cullingAngle));
                }
            }
        }
    }

    private void ConstructDebug()
    {
        for(int f = 0; f < 6; f++)
        {
            for(int i = 0; i < chunkCount; i++)
            {
                for(int j = 0; j < chunkCount; j++)
                {
                    terrainChunks.Get(f, i, j).ConstructDebug();
                }
            }
        }
    }

    private void Construct()
    {
        if(chunkCount != terrainChunks.height)
        {
            Initialise();
        }

        switch(constructMode)
        {
            case ConstructMode.Uniform:
                ConstructUniform();
            return;

            case ConstructMode.Dynamic:
                ConstructDynamic();
            return;

            case ConstructMode.Debug:
                ConstructDebug();
            return;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if(init)
        {
            init = false;

            Initialise();
        }

        if(update || autoUpdate)
        {
            update = false;

            Construct();
        }

        if(printChunks)
        {
            for(int f = 0; f < 6; f++)
            {
                for(int i = 0; i < chunkCount; i++)
                {
                    for(int j = 0; j < chunkCount; j++)
                    {
                        GD.Print(terrainChunks.Get(f, i, j));
                    }
                }
            }
        }
    }
}
