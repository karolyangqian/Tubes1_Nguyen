using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// Roar
// ------------------------------------------------------------------
// Targeting: Linear
// Movement: Corner 
// ------------------------------------------------------------------
/*

v1.1
- Add Radar Lock
- Add enemy distance to movement

v1.2 
- Improve movement

*/
// ------------------------------------------------------------------
public class Roar : Bot
{
    static int MOVE_WALL_MARGIN = 25;

    static int moveDir = 1;
    static double enemyDistance = double.PositiveInfinity;

    static void Main()
    {
        new Roar().Start();
    }

    Roar() : base(BotInfo.FromFile("roar.json")) { }

    public override void Run()
    {
        Console.WriteLine("Hello! I'm Roar!");
        
        BodyColor = Color.Red;
        TurretColor = Color.Yellow;
        RadarColor = Color.Red;
        BulletColor = Color.Red;
        ScanColor = Color.Red;

        SetTurnRadarRight(double.PositiveInfinity);
        AdjustGunForBodyTurn = true;
    } 

    public override void OnTick(TickEvent e) {
        // Console.WriteLine("GunHeat: " + GunHeat + " Energy: " + Energy);

        // Corner Movement
        int x = MOVE_WALL_MARGIN + (int) (enemyDistance / 2.7);
        int y = MOVE_WALL_MARGIN;

        if (DistanceRemaining == 0) 
        {
            moveDir = -moveDir;
        }

        if (moveDir > 0) 
        {
            y = x;
            x = MOVE_WALL_MARGIN;
        }

        if (X > ArenaWidth / 2)
        {
            x = (int) (ArenaWidth - x);
        }

        if (Y > ArenaHeight / 2)
        {
            y = (int) (ArenaHeight - y);
        }

        double turn = BearingTo(x, y) * Math.PI / 180;
        SetTurnLeft(180 / Math.PI * Math.Tan(turn));
        SetForward(DistanceTo(x, y) * Math.Cos(turn));

        // Console.WriteLine("X: " + X + " Y: " + Y);
        // Console.WriteLine("Target X: " + x + " Target Y: " + y);
        // Console.WriteLine("DistanceRemaining: " + DistanceRemaining);
    }

    public override void OnScannedBot(ScannedBotEvent e) {
        // Console.WriteLine("\nI see a bot!");
        // Console.WriteLine("Id       : " + e.ScannedBotId);
        // Console.WriteLine("Energy   : " + e.Energy);
        // Console.WriteLine("Direction: " + e.Direction);
        // Console.WriteLine("Speed    : " + e.Speed);
        // Console.WriteLine("X        : " + e.X);
        // Console.WriteLine("Y        : " + e.Y);
        // Console.WriteLine("Distance : " + DistanceTo(e.X, e.Y));

        enemyDistance = DistanceTo(e.X, e.Y);

        // Appearance
        if (moveDir == 1)
        {
            BodyColor = Color.Red;
            TurretColor = Color.Yellow;
            RadarColor = Color.Red;
            BulletColor = Color.Red;
            ScanColor = Color.Red;
        }
        else
        {
            BodyColor = Color.Yellow;
            TurretColor = Color.Red;
            RadarColor = Color.Yellow;
            BulletColor = Color.Yellow;
            ScanColor = Color.Yellow;
        }

        // Console.WriteLine("myX: " + X + " myY: " + Y);
        // Console.WriteLine("target x: " + x + " target y: " + y);
        // Console.WriteLine("DistanceRemaining: " + DistanceRemaining);

        double radarAngle = double.PositiveInfinity * NormalizeRelativeAngle(RadarBearingTo(e.X, e.Y));

        if (!double.IsNaN(radarAngle) && (GunHeat < 1 || EnemyCount == 1)) 
        {
            SetTurnRadarLeft(radarAngle);
        }

        // Targeting
        double firePower = (Math.Sqrt(ArenaHeight * ArenaHeight + ArenaWidth * ArenaWidth)) / DistanceTo(e.X, e.Y) * 0.3;

        if (GunTurnRemaining == 0)
        {
            SetFire(firePower);
        }

        double bulletSpeed = CalcBulletSpeed(firePower);
        
        double enemyDir = e.Direction * Math.PI / 180.0;
        
        double time = DistanceTo(e.X, e.Y) / bulletSpeed;
        
        double predictedX = e.X + e.Speed * time * Math.Cos(enemyDir);
        double predictedY = e.Y + e.Speed * time * Math.Sin(enemyDir);
        
        double bearingFromGun = GunBearingTo(predictedX, predictedY);

        SetTurnGunLeft(bearingFromGun);
    }
}