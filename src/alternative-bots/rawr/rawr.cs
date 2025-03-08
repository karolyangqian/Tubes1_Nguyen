using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// Rawr
// ------------------------------------------------------------------
// Targetting: Linear Targteting
// Movement: Anti-Gravity
// ------------------------------------------------------------------
public class Rawr : Bot
{
    static void Main()
    {
        new Rawr().Start();
    }

    Rawr() : base(BotInfo.FromFile("rawr.json")) { }

    static int      lastTargetId;
    static double   lastDistance = double.PositiveInfinity;
    static bool     movingForward;

    public override void Run()
    {
        BodyColor = Color.White;
        TurretColor = Color.White;
        RadarColor = Color.Black;
        BulletColor = Color.Black;
        ScanColor = Color.Black;
        Console.WriteLine("ArenaWidth: " + ArenaWidth);
        Console.WriteLine("ArenaHeight: " + ArenaHeight);

        SetTurnRadarRight(double.PositiveInfinity);
        AdjustGunForBodyTurn = true;
        movingForward = true;

    }

    public override void OnScannedBot(ScannedBotEvent e)
    { 
        Console.WriteLine("\n\nI see a Bot!");
        double dx = e.X - X;
        double dy = e.Y - Y;
        double distance = DistanceTo(e.X, e.Y);
        double firePower = EnemyCount;
        double bulletSpeed = CalcBulletSpeed(firePower);
        
        double absBearing = Math.Atan2(dy, dx);
        
        double enemyDir = e.Direction * Math.PI / 180.0;
        
        double ratio = Math.Max(-1, Math.Min(1, (e.Speed * Math.Sin(enemyDir - absBearing)) / bulletSpeed));
        double leadAngle = Math.Asin(ratio);
        
        double gunDirection = absBearing + leadAngle;
        
        double time = distance / bulletSpeed;
        
        double predictedX = e.X + e.Speed * time * Math.Cos(enemyDir);
        double predictedY = e.Y + e.Speed * time * Math.Sin(enemyDir);
        
        double bearingFromGun = GunBearingTo(predictedX, predictedY);
        

        if (lastTargetId == e.ScannedBotId) {
            SetTurnGunLeft(bearingFromGun);
            SetFire(firePower);
            lastDistance = double.PositiveInfinity;
        }

        // SetFire(firePower);

        if (lastDistance > distance) {
            lastDistance = distance;
            lastTargetId = e.ScannedBotId;
        }
        
        SetTurnRight(NormalizeRelativeAngle((
        Math.Atan2((-5 * Math.Sin(absBearing) / distance) + 1/X - 1/(ArenaWidth - X),
                   (-5 * Math.Cos(absBearing) / distance) + 1/Y - 1/(ArenaHeight - Y))
                    - (Direction * Math.PI / 180)) * 180 / Math.PI + 90));
        // SetTurnRight(NormalizeRelativeAngle(BearingTo(predictedX, predictedY) + 90));

        if (movingForward) {
            SetForward(double.PositiveInfinity);
        } else {
            SetBack(double.PositiveInfinity);
        }
        // MaxSpeed = 12 / Math.Abs(TurnRemaining * Math.PI / 180);

        if (GunHeat < 1) {
            SetTurnRadarRight(RadarTurnRemaining);
        }

        Console.WriteLine("distance: " + distance);
        Console.WriteLine("TurnRemaining: " + TurnRemaining);
        Console.WriteLine("GunHeat: " + GunHeat);
        Console.WriteLine("RadarTurnRemaining: " + RadarTurnRemaining);
        Console.WriteLine("MaxSpeed: " + MaxSpeed);
        Console.WriteLine("Direction: " + Direction);
        Console.WriteLine("X: " + X + " Y: " + Y);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        movingForward = !movingForward;
    }
}