using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace Updraft;

public sealed class MainForm : Form
{
    private readonly Game _game = new();
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 15 };
    private double _last;
    private float _cloudOffset;

    private static readonly Color SkyTop = Color.FromArgb(255, 244, 228);
    private static readonly Color SkyBottom = Color.FromArgb(255, 205, 178);
    private static readonly Color Wall = Color.FromArgb(62, 74, 107);
    private static readonly Color WallEdge = Color.FromArgb(44, 53, 79);
    private static readonly Color Paper = Color.FromArgb(252, 252, 250);
    private static readonly Color Ink = Color.FromArgb(38, 38, 48);

    public MainForm()
    {
        Text = "Updraft";
        ClientSize = new Size((int)Game.Width, (int)Game.Height);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        DoubleBuffered = true;
        KeyPreview = true;

        KeyDown += (_, e) =>
        {
            if (e.KeyCode is Keys.Space or Keys.Up or Keys.W) _game.Flap();
            if (e.KeyCode == Keys.Escape) Close();
        };
        MouseDown += (_, _) => _game.Flap();

        _timer.Tick += (_, _) => Tick();
        _timer.Start();
    }

    private void Tick()
    {
        double now = _clock.Elapsed.TotalSeconds;
        // Clamp the step so a stalled window cannot push the glider through a wall.
        float dt = (float)Math.Min(now - _last, 1.0 / 30);
        _last = now;
        _game.Update(dt);
        if (_game.Phase != Phase.Crashed) _cloudOffset = (_cloudOffset + dt * 20) % Game.Width;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using (var sky = new LinearGradientBrush(new Point(0, 0), new Point(0, (int)Game.Ground), SkyTop, SkyBottom))
            g.FillRectangle(sky, 0, 0, Game.Width, Game.Ground);

        using (var cloud = new SolidBrush(Color.FromArgb(120, 255, 255, 255)))
        {
            for (int i = 0; i < 4; i++)
            {
                float x = (i * 150 - _cloudOffset + Game.Width) % (Game.Width + 120) - 60;
                g.FillEllipse(cloud, x, 80 + i * 97 % 220, 110, 34);
            }
        }

        using (var wall = new SolidBrush(Wall))
        using (var edge = new Pen(WallEdge, 3))
        {
            foreach (var gate in _game.Gates)
            {
                float bottomY = gate.GapTop + _game.Gap;
                g.FillRectangle(wall, gate.X, -4, Game.GateWidth, gate.GapTop + 4);
                g.FillRectangle(wall, gate.X, bottomY, Game.GateWidth, Game.Ground - bottomY);
                g.DrawLine(edge, gate.X, gate.GapTop, gate.X + Game.GateWidth, gate.GapTop);
                g.DrawLine(edge, gate.X, bottomY, gate.X + Game.GateWidth, bottomY);
            }
        }

        using (var ground = new SolidBrush(Color.FromArgb(214, 160, 128)))
            g.FillRectangle(ground, 0, Game.Ground, Game.Width, Game.Height - Game.Ground);

        DrawGlider(g);
        DrawText(g);
    }

    private void DrawGlider(Graphics g)
    {
        // A paper plane, tilted by its vertical speed.
        float angle = Math.Clamp(_game.VelocityY / 12f, -25, 70);
        var state = g.Save();
        g.TranslateTransform(Game.GliderX, _game.Y);
        g.RotateTransform(angle);
        PointF[] wing = { new(20, 0), new(-16, -12), new(-8, 0), new(-16, 10) };
        using var fill = new SolidBrush(Paper);
        using var outline = new Pen(Ink, 2) { LineJoin = LineJoin.Round };
        g.FillPolygon(fill, wing);
        g.DrawPolygon(outline, wing);
        g.DrawLine(outline, -8, 0, 20, 0);
        g.Restore(state);
    }

    private void DrawText(Graphics g)
    {
        using var big = new Font("Segoe UI Semibold", 34);
        using var small = new Font("Segoe UI", 12);
        using var ink = new SolidBrush(Ink);
        var center = new StringFormat { Alignment = StringAlignment.Center };

        g.DrawString(_game.Score.ToString(), big, ink, Game.Width / 2, 28, center);
        if (_game.Phase == Phase.Ready)
            g.DrawString("Space, click or Up to catch an updraft", small, ink, Game.Width / 2, Game.Height / 2 + 60, center);
        if (_game.Phase == Phase.Crashed)
        {
            g.DrawString("Crashed", big, ink, Game.Width / 2, Game.Height / 2 - 90, center);
            g.DrawString($"Score {_game.Score}   Best {_game.Best}\nSpace to fly again", small, ink, Game.Width / 2, Game.Height / 2 - 30, center);
        }
    }
}
