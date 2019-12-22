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
    public static int m_lastTick = -10;
    private static SimGame m_lastGame;
    public UnitAction GetAction(Unit unit, Game game, AiCup2019.Debug debug)
    {
        if(m_lastTick >= game.CurrentTick-1) // Verify this number
        {
            //TODO: Check number of bullets
            LogService.WriteLine("Cached choice");
            return CreateAction(m_lastGame.Units.First(u => u.unit.Id == unit.Id), m_lastGame);
        }

        Const.Reset(game.Properties, game);
        Debug = debug;
        var myGame = new MyGame(game, unit);
        var sim = m_lastGame = new SimGame(myGame, unit);
        m_lastTick = game.CurrentTick;
        DistService.CalcDists(sim);
        ShootService.Initialize(sim);
        foreach (var b in sim.Bullets) b.CalcCollisionTime(sim);

        foreach(var u in sim.Units)
        {
            u.WalkTarget = WalkService.FindWalkTarget(sim, u);
            u.AimTarget = AimService.GetAimTarget(myGame, u);
            u.Shoot = ShootService.ShouldShoot(sim, u);
        }

        MCTSService.Search(sim);
        MCTSService.DoOneRound(sim, true);

        if (game.CurrentTick % 600 == 0)
            Console.Error.WriteLine("Time: " + Const.GetTime + " Evals: " + Const.Evals + " Sims: " + Const.Sims);

        return CreateAction(m_lastGame.Units.First(u => u.unit.Id == unit.Id), m_lastGame);
    }

    private static UnitAction CreateAction(SimUnit unit, SimGame game)
    {
        var selectedAction = unit.GetBestNode();
        var aim = unit.AimTarget;
        var pos = unit.Position;
        var shoot = unit.Shoot;
        var enemies = unit.Enemies.OrderBy(e => e.Position.Dist(pos) + e.Health * 0.01).ToList();
        var targetPos = enemies.First().Position;
        UnitAction action = new UnitAction();
        action.Velocity = selectedAction.Dx * Const.Properties.UnitMaxHorizontalSpeed;
        action.Jump = selectedAction.JumpUp;
        action.JumpDown = selectedAction.JumpDown;
        action.Aim = new Vec2Double(aim.X -pos.X, aim.Y - pos.Y);
        action.Shoot = shoot;
        action.Reload = !shoot && unit.HasWeapon && pos.Dist(targetPos) > 5 && unit.HasWeapon && unit.Weapon.Magazine < unit.Weapon.Parameters.MagazineSize * 0.3;
        action.SwapWeapon = SwapService.ShouldSwap(game, unit);
        action.PlantMine = unit.Position.Dist(unit.TargetEnemy.Position) < 3;
        return action;
    }
}
