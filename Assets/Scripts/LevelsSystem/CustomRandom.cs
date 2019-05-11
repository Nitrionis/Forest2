

public static class CustomRandom
{
    public static float Get(uint x)
	{
		x = (x << 13) ^ x;
		return (1.0f - ((x * (x * x * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0f) / 2.0f + 0.5f;
	}
}
