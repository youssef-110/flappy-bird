namespace Updraft;

/// <summary>A wind gate: a pair of walls with a gap the glider must pass through.</summary>
public sealed class Gate
{
    public float X;
    public float GapTop;
    public bool Scored;
}

public enum Phase { Ready, Flying, Crashed }

/// <summary>
/// All game rules, with no drawing or input code, so they can be tested.
/// Units are pixels and seconds. The glider stays at a fixed X; gates move left.
/// </summary>
public sealed class Game
{
    public const float Width = 480, Height = 640, Ground = 600;
    public const float GliderX = 140, GliderRadius = 14;
    public const float Gravity = 1400, LiftSpeed = -430, MaxFallSpeed = 650;
    public const float GateWidth = 70, GateSpacing = 240;
    public const float StartGap = 190, MinGap = 130, StartSpeed = 170, MaxSpeed = 290;

    private readonly Random _random;

    public Game(int seed = 0)
    {
        _random = seed == 0 ? new Random() : new Random(seed);
        Reset();
    }

    public Phase Phase { get; private set; }
    public float Y { get; private set; }
    public float VelocityY { get; private set; }
    public int Score { get; private set; }
    public int Best { get; private set; }
    public List<Gate> Gates { get; } = new();

    /// <summary>The gap narrows and the scroll speeds up as the score grows.</summary>
    public float Gap => Math.Max(MinGap, StartGap - Score * 3);
    public float Speed => Math.Min(MaxSpeed, StartSpeed + Score * 4);

    public void Reset()
    {
        Phase = Phase.Ready;
        Y = Height / 2 - 40;
        VelocityY = 0;
        Score = 0;
        Gates.Clear();
    }

    /// <summary>The one control: Space, click or Up gives the glider an updraft.</summary>
    public void Flap()
    {
        if (Phase == Phase.Crashed) { Reset(); return; }
        Phase = Phase.Flying;
        VelocityY = LiftSpeed;
    }

    public void Update(float dt)
    {
        if (Phase != Phase.Flying) return;

        VelocityY = Math.Min(VelocityY + Gravity * dt, MaxFallSpeed);
        Y += VelocityY * dt;

        foreach (var gate in Gates) gate.X -= Speed * dt;
        Gates.RemoveAll(g => g.X + GateWidth < 0);
        if (Gates.Count == 0 || Gates[^1].X < Width - GateSpacing) SpawnGate();

        foreach (var gate in Gates)
        {
            if (!gate.Scored && gate.X + GateWidth < GliderX - GliderRadius)
            {
                gate.Scored = true;
                Score++;
            }
        }

        if (Y + GliderRadius >= Ground || Y - GliderRadius <= 0 || Gates.Any(g => Hits(g, Y, Gap))) Crash();
    }

    private void SpawnGate()
    {
        const float margin = 60;
        float top = margin + (float)_random.NextDouble() * (Ground - Gap - 2 * margin);
        Gates.Add(new Gate { X = Width + 20, GapTop = top });
    }

    /// <summary>Circle-versus-rectangle test against the two walls of a gate.</summary>
    public static bool Hits(Gate gate, float y, float gap)
    {
        bool overlapsX = GliderX + GliderRadius > gate.X && GliderX - GliderRadius < gate.X + GateWidth;
        if (!overlapsX) return false;
        return y - GliderRadius < gate.GapTop || y + GliderRadius > gate.GapTop + gap;
    }

    private void Crash()
    {
        Phase = Phase.Crashed;
        Best = Math.Max(Best, Score);
    }

    /// <summary>Test hook: place the glider at a height.</summary>
    public void SetY(float y) => Y = y;
}
