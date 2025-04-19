public static class TerrainInfo{
    public static readonly float SCALE_DENOMINATOR = 2.1814659085787088E+06f;
    public static readonly float TILE_WIDTH = 256f;
    public static readonly float WMS_PIXEL_SIZE = 0.28e-3f;

    private static float LATERAL_SCALE_FACTOR = 0.01f;
    private static float HEIGHT_SCALE_FACTOR = 0.00025f;

    public static readonly float MIN_ELEVATION = -8000f;
    public static readonly float MAX_ELEVATION = 21000f;

    public static readonly float ELEVATION_RANGE;
    public static readonly float TERRAIN_WIDTH;
    public static readonly float TERRAIN_LENGTH;

    public static readonly int HEIGHTMAP_RESOLUTION = 257;
    public static readonly int TILE_MATRIX_SET = 7;

    static TerrainInfo(){
        TERRAIN_WIDTH = GetTileSpan();
        TERRAIN_LENGTH = TERRAIN_WIDTH;
        ELEVATION_RANGE = GetElevationRange();
    }

    private static float GetPixelSpan(){
        return SCALE_DENOMINATOR * WMS_PIXEL_SIZE;
    }

    private static float GetTileSpan(){
        return TILE_WIDTH * GetPixelSpan() * LATERAL_SCALE_FACTOR;
    }

    private static float GetElevationRange(){
        return (MAX_ELEVATION - MIN_ELEVATION) * HEIGHT_SCALE_FACTOR;
    }
}