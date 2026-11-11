using System.Drawing;
using System.Reflection;
using Updraft;
using Xunit;

public class RenderTests
{
    [Fact]
    public void Main_form_draws_a_frame_mid_flight()
    {
        using var form = new MainForm();
        var game = (Game)typeof(MainForm).GetField("_game", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(form)!;
        game.Flap();
        for (int i = 0; i < 90; i++)
        {
            if (game.VelocityY > 120) game.Flap();
            game.Update(0.016f);
        }

        using var bmp = new Bitmap(form.ClientSize.Width, form.ClientSize.Height);
        form.DrawToBitmap(bmp, new Rectangle(Point.Empty, bmp.Size));
        bmp.Save(Path.Combine(Path.GetTempPath(), "updraft-frame.png"));

        // Sky, walls and ground give clearly different colors.
        var colors = new HashSet<int>();
        for (int x = 0; x < bmp.Width; x += 8)
            for (int y = 0; y < bmp.Height; y += 8)
                colors.Add(bmp.GetPixel(x, y).ToArgb());
        Assert.True(colors.Count > 20);
    }
}
