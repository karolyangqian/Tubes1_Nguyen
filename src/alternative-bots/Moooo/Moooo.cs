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
public class Moooo : Bot
{   
    static void Main(string[] args)
    {
        new Moooo().Start();
    }

    Moooo() : base(BotInfo.FromFile("Moooo.json")) { }

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
        double firePower = 1;
        double bulletSpeed = CalcBulletSpeed(firePower);
        
        double dx = e.X - X;
        double dy = e.Y - Y;
        double absBearing = Math.Atan2(dy, dx);
        
        double enemyDir = e.Direction * Math.PI / 180.0;
        
        double ratio = Math.Max(-1, Math.Min(1, (e.Speed * Math.Sin(enemyDir - absBearing)) / bulletSpeed));
        double leadAngle = Math.Asin(ratio);
        
        double gunDirection = absBearing + leadAngle;
        
        double distance = Math.Sqrt(dx * dx + dy * dy);
        double time = distance / bulletSpeed;
        
        double predictedX = e.X + e.Speed * time * Math.Cos(enemyDir);
        double predictedY = e.Y + e.Speed * time * Math.Sin(enemyDir);
        
        double bearingFromGun = GunBearingTo(predictedX, predictedY);
        
        TurnGunLeft(bearingFromGun);
        Fire(firePower);
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
