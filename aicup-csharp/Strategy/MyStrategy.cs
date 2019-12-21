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

        Const.Reset(game.Properties);
        Debug = debug;
        var myGame = new MyGame(game, unit);
        var sim = m_lastGame = new SimGame(myGame, unit);
        m_lastTick = game.CurrentTick;
        DistService.CalcDists(sim);
        foreach (var b in sim.Bullets) b.CalcCollisionTime(sim);

        //TODO: Add collision with healing if life is below 70%?
        //TODO: Collision med life om fiende har bazooka? Eller det er miner i omegn?
        //TODO: Jump eget tree også? Blir kanskje litt far fetched..
        //TODO: Make independent tree for shooting? Blir ikke så påvirket av mine moves
        //TODO: Hva skal shooting skyte på? Nærmeste posisjoner fiende kan være på?
        //TODO: Første omgang er det nok å simme at fiende kan skyte 1 kule av Rocket. Slik at eg minimerer skade
        //TODO: Så ser vi hvor mye Spread faktisk har å si.
        //TODO: Ser da om fiende kan treffe meg med 1 kule.
        //TODO: Husk å redusere skade?
        //TODO: Verify jump pad sim.

        foreach(var u in sim.Units)
        {
            //TODO: All these needs to knpw what the enemy wants too?
            //u.AimTarget = AimService.GetAimTarget(myGame);
            //u.Shoot = ShootService.ShouldShoot(myGame, u.AimTarget);
            u.WalkTarget = WalkService.FindWalkTarget(myGame);
            u.AimTarget = AimService.GetAimTarget(myGame, u);
            u.Shoot = ShootService.ShouldShoot(myGame, u.AimTarget);
        }

        if(game.CurrentTick % 600 == 0)
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
        action.Reload = !shoot && pos.Dist(targetPos) > 5 && unit.HasWeapon && me.Weapon.Magazine < me.Weapon.Parameters.MagazineSize * 0.3;
        action.SwapWeapon = SwapService.ShouldSwap(game);
        action.PlantMine = myGame.Me.Center.Dist(myGame.Enemy.Center) < 3;
        return action;
    }
}
