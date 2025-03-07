using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// Mooo
// ------------------------------------------------------------------
// Targetting: Naive
// Movement: Circle
// ------------------------------------------------------------------
public class Mooo : Bot
{   
    static void Main(string[] args)
    {
        new Mooo().Start();
    }

    Mooo() : base(BotInfo.FromFile("Mooo.json")) { }

    public override void Run()
    {
        BodyColor = Color.Black;
        TurretColor = Color.Black;
        RadarColor = Color.Black;
        BulletColor = Color.Black;
        ScanColor = Color.Black;

        SetTurnRadarRight(double.PositiveInfinity);
        AdjustGunForBodyTurn = true;

        while (IsRunning)
        {
            SetTurnLeft(10_000);
            Forward(10_000);
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        var bearingFromGun = GunBearingTo(e.X, e.Y);

        TurnGunLeft(bearingFromGun);
        Fire(1);
    }

    public override void OnHitBot(HitBotEvent e)
    {
        SetTurnLeft(180);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        SetTurnLeft(180);
    }

}
