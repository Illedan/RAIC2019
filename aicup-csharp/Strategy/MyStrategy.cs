using static AiCup2019.Model.CustomData;
using aicup2019.Strategy;
using System.Linq;
using AiCup2019.Model;
using AiCup2019;
using System;
using aicup2019.Strategy.Services;
using static AiCup2019.Model.Item;
using aicup2019.Strategy.Sim;
using System.Diagnostics;

public class MyStrategy
{
    public static AiCup2019.Debug Debug;

    public UnitAction GetAction(Unit unit, Game game, AiCup2019.Debug debug)
    {
        Const.Reset(game.Properties);

        Debug = debug;
        var myGame = new MyGame(game, unit);
        var me = myGame.Me;
        var aim = AimService.GetAimTarget(myGame);
        var shoot = ShootService.ShouldShoot(myGame, aim);
        var walkTarget = WalkService.FindWalkTarget(myGame);
        var jump = JumpService.GetDir(myGame, walkTarget);

        var stopwatch = Stopwatch.StartNew();
        var sim = new SimGame(myGame);
        var mySimUnit = sim.Units.First(u => u.unit.Id == me.Unit.Id);
        var selectedAction = MyAction.DoNothing;
        var bestScore = -10000.0;
        Const.Depth = sim.Bullets.Any(b => b.bullet.UnitId != mySimUnit.unit.Id) || me.Center.Dist(walkTarget) > 3 ? 30 : 6;

        foreach (var act in MyAction.Actions)
        {
            var score = SimService.ScoreDir(sim, act, walkTarget, mySimUnit, true);
            sim.Reset();
            if (score > bestScore)
            {
                selectedAction = act;
                bestScore = score;
            }
        }

        foreach (var act in MyAction.Actions)
        {
            var score = SimService.ScoreDir(sim, act, walkTarget, mySimUnit, false);
            sim.Reset();
            if(score > bestScore)
            {
                selectedAction = act;
                bestScore = score;
            }
        }
        //Console.Error.WriteLine("Time: " + stopwatch.ElapsedMilliseconds + " BestScore: " + bestScore);

       // debug.Draw(new Line(aim.CreateFloatVec, me.Center.CreateFloatVec, 0.05f, new ColorFloat(1, 0, 0, 1)));
        //debug.Draw(new Line(walkTarget.CreateFloatVec, me.Center.CreateFloatVec, 0.1f, new ColorFloat(0, 0.5f, 0, 1)));
        //  Debug.Draw(new Log("Spread: " + (unit.Weapon.HasValue?unit.Weapon.Value.Spread:0) + " MAG: " + (unit.Weapon.HasValue ? unit.Weapon.Value.Magazine : 0) + " Reload: " + reload));
        UnitAction action = new UnitAction();
        //action.Velocity =me.Center.X < walkTarget.X ? game.Properties.UnitMaxHorizontalSpeed : -game.Properties.UnitMaxHorizontalSpeed;
        //action.Jump = jump > 0;
        //action.JumpDown = jump < 0;
        action.Velocity = selectedAction.Dx * game.Properties.UnitMaxHorizontalSpeed;
        action.Jump = selectedAction.JumpUp;
        action.JumpDown = selectedAction.JumpDown;
        action.Aim = new Vec2Double(aim.X - me.Center.X, aim.Y - me.Center.Y);
        action.Shoot = shoot;
        action.Reload = me.Center.Dist(myGame.Enemy.Center) > 5 && me.HasWeapon && me.Weapon.Magazine < me.Weapon.Parameters.MagazineSize*0.3;
        action.SwapWeapon = SwapService.ShouldSwap(myGame);
        action.PlantMine = myGame.Me.Center.Dist(myGame.Enemy.Center) < 3;

        var spread = AimService.GetSpread(myGame, aim);
        foreach(var point in spread)
        {
           debug.Draw(new Line(point.CreateFloatVec, me.Center.CreateFloatVec, 0.05f, new ColorFloat(0.1f, 0.1f, 0.4f, 1)));
        }

        foreach (var bullet in myGame.Bullets)
        {
            var start = bullet.Bullet.Position;
            var end = myGame.CalcBulletEnd(bullet, out bool didHit);
            var sFloat = new Vec2Float((float)start.X, (float)start.Y);
            var eFloat = new Vec2Float((float)end.X, (float)end.Y);
            debug.Draw(new Line(sFloat, eFloat, 0.1f, new ColorFloat(0, 0, 0, 1)));
        }

        return action;
    }
}
