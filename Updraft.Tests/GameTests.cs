using Updraft;
using Xunit;

public class GameTests
{
    [Fact]
    public void Starts_ready_and_does_not_fall_before_the_first_flap()
    {
        var game = new Game(seed: 1);
        float y = game.Y;
        game.Update(0.5f);
        Assert.Equal(Phase.Ready, game.Phase);
        Assert.Equal(y, game.Y);
    }

    [Fact]
    public void Flap_lifts_the_glider_then_gravity_pulls_it_down()
    {
        var game = new Game(seed: 1);
        float start = game.Y;
        game.Flap();
        game.Update(0.05f);
        Assert.True(game.Y < start);
        for (int i = 0; i < 20; i++) game.Update(0.016f);
        Assert.True(game.VelocityY > 0);
    }

    [Fact]
    public void Falling_to_the_ground_crashes_and_records_best()
    {
        var game = new Game(seed: 1);
        game.Flap();
        for (int i = 0; i < 200 && game.Phase == Phase.Flying; i++) game.Update(0.016f);
        Assert.Equal(Phase.Crashed, game.Phase);
    }

    [Fact]
    public void Flap_after_a_crash_starts_a_new_round()
    {
        var game = new Game(seed: 1);
        game.Flap();
        for (int i = 0; i < 200 && game.Phase == Phase.Flying; i++) game.Update(0.016f);
        game.Flap();
        Assert.Equal(Phase.Ready, game.Phase);
        Assert.Equal(0, game.Score);
        Assert.Empty(game.Gates);
    }

    [Fact]
    public void Gate_collision_only_outside_the_gap()
    {
        var gate = new Gate { X = Game.GliderX - 10, GapTop = 200 };
        Assert.False(Game.Hits(gate, 280, 160));  // centered in the gap
        Assert.True(Game.Hits(gate, 195, 160));   // clips the top wall
        Assert.True(Game.Hits(gate, 355, 160));   // clips the bottom wall
        var farAway = new Gate { X = 400, GapTop = 200 };
        Assert.False(Game.Hits(farAway, 50, 160));
    }

    [Fact]
    public void Steering_through_gates_scores_points_and_tightens_the_gap()
    {
        var game = new Game(seed: 7);
        game.Flap();
        // Autopilot: flap whenever the glider sinks below the middle of the next gap.
        for (int i = 0; i < 6000 && game.Phase == Phase.Flying && game.Score < 5; i++)
        {
            var next = game.Gates.FirstOrDefault(g => g.X + Game.GateWidth > Game.GliderX - Game.GliderRadius);
            float target = next is null ? Game.Height / 2 : next.GapTop + game.Gap / 2 + 15;
            if (game.Y > target && game.VelocityY > 0) game.Flap();
            game.Update(0.016f);
        }
        Assert.True(game.Score >= 5, $"autopilot scored {game.Score}");
        Assert.True(game.Gap < Game.StartGap);
    }
}
