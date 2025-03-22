using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// TestBot
// ------------------------------------------------------------------
// Targeting: Circular
// Movement:  
// ------------------------------------------------------------------
/*

*/
// ------------------------------------------------------------------
public class TestBot : Bot
{
    static double WALL_MARGIN = 70;
    static double CORNER_MARGIN = 100;
    static double STICK_LENGTH = 50;
    static double OSCILLATION_RADIUS = 60;
    static double MAX_SPEED = 8;
    static double GUN_FACTOR = 5;
    static bool navigating = false;
    static Dictionary<int, EnemyData> enemies = new Dictionary<int, EnemyData>();
    static Random random = new Random();
    static Point2D corner = new Point2D(0, 0);
    static bool oneVsOne = false;
    static int targetId = -1;
    static bool targetLocked = false;
    static double wallSmoothTurnIncr = 0.5;
    static double wallSmoothSpeedIncr = 2;



    static void Main()
    {
        new TestBot().Start();
    }

    TestBot() : base(BotInfo.FromFile("TestBot.json")) { }

    public override void Run()
    {
        Console.WriteLine("Hello! I'm TestBot!");
        
        BodyColor = Color.FromArgb(0x00, 0x64, 0x64); // Dark cyan
        TurretColor = Color.Yellow;
        RadarColor = Color.FromArgb(0x00, 0xC8, 0x00);   // lime
        BulletColor = Color.FromArgb(0x00, 0x96, 0x32); // green
        ScanColor = Color.Red;

        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        AdjustRadarForBodyTurn = true;

        MaxSpeed = MAX_SPEED;

        // for (int i = 0; i < 8; i++) {
        //     TurnRadarRight(45);
        // }
        AdjustGunForBodyTurn = true;
        navigating = true;

        corner = SafestCorner();
    } 

    public override void OnTick(TickEvent e) {
        // Console.WriteLine(string.Format("Safest corner: {0:0.00} {1:0.00}", corner.x, corner.y));
        if (navigating) {
            MoveTo(corner.x, corner.y);
            if (DistanceTo(corner.x, corner.y) < 100) {
                TargetSpeed = 0;
                navigating = false;
            }
        } else {
            Oscillate();
        }
        Console.WriteLine(string.Format("Distance to corner: {0:0.00} {1}", DistanceTo(corner.x, corner.y), navigating));

    }

    public override void OnScannedBot(ScannedBotEvent e) {
        
    }

    public override void OnBotDeath(BotDeathEvent e) {
        
    }

    public override void OnHitWall(HitWallEvent botHitWallEvent) {
        // Console.WriteLine("Hit Wallllllllllllllllllllllllll");
    }


// ========================== METHODS ===============================

    private void Oscillate()
    {
        Point2D magicStick = CalcMagicStick();
        double theta = 20 * Math.PI / 180;
        double newX = (magicStick.x - X) * Math.Cos(theta) - (magicStick.y - Y) * Math.Sin(theta) + X;
        double newY = (magicStick.y - Y) * Math.Cos(theta) + (magicStick.x - Y) * Math.Sin(theta) + Y;

        newX = Math.Max(WALL_MARGIN, Math.Min(ArenaWidth - WALL_MARGIN, newX));
        newY = Math.Max(WALL_MARGIN, Math.Min(ArenaHeight - WALL_MARGIN, newY));

        MoveTo(newX, newY);
        // MoveTo(0, 0);
        // TargetSpeed = 4;S
        double turn = RadarBearingTo(newX, newY);
        TurnRadarLeft(turn);
        Console.WriteLine(string.Format("MoveTo: {0:0.00} {1:0.00}", newX, newY));

    }
    private bool IsCloseToWall()
    {
        return X < WALL_MARGIN || X > ArenaWidth - WALL_MARGIN || Y < WALL_MARGIN || Y > ArenaHeight - WALL_MARGIN;
    }

    private bool IsOutsideArena(double x, double y) {
        return x < 0 || x > ArenaWidth || y < 0 || y > ArenaHeight;
    }

    private Point2D SafestCorner() {
        Point2D[] corners = new Point2D[] {
            new Point2D(CORNER_MARGIN, CORNER_MARGIN),
            new Point2D(CORNER_MARGIN, ArenaHeight - CORNER_MARGIN),
            new Point2D(ArenaWidth - CORNER_MARGIN, CORNER_MARGIN),
            new Point2D(ArenaWidth - CORNER_MARGIN, ArenaHeight - CORNER_MARGIN)
        };
        Point2D safest = corners[0];
        double maxDistance = 0;
        double sumDistance = 0;
        foreach (Point2D corner in corners) {
            foreach (EnemyData enemy in enemies.Values) {
                sumDistance += Math.Sqrt(Math.Pow(corner.x - enemy.LastX, 2) + Math.Pow(corner.y - enemy.LastY, 2));
            }
            if (sumDistance > maxDistance) {
                maxDistance = sumDistance;
                safest = corner;
            }
            sumDistance = 0;
        }

        return safest;
    }

    private void MoveTo(double x, double y, double max_speed = 8) {
        double turn = NormalizeRelativeAngle(BearingTo(x, y));
        // double p = 0.5;
        // TurnRate = p * turn;
        SetTurnLeft(turn);
        double turnRadius = (180 - Math.Abs(turn)) / 180 * max_speed / (TurnRate == 0 ? 1 : TurnRate);
        TargetSpeed = turn != 0 ? TurnRate * turnRadius : max_speed;
        // double turn = BearingTo(x, y) * Math.PI / 180;
        // SetTurnLeft(180 / Math.PI * Math.Tan(turn));
        // SetForward(DistanceTo(x, y) * Math.Cos(turn));
        // double turn = BearingTo(x, y);
        // SetForward(DistanceTo(x, y));
    }

    private void TrackScanAt(double x, double y) {
        var bearingFromRadar = NormalizeRelativeAngle(RadarBearingTo(x, y));
        SetTurnRadarLeft(bearingFromRadar + (bearingFromRadar > 0 ? 20 : -20));
    }

    private int SelectTargetEnemy() {
        double minFactor = double.PositiveInfinity;
        int closestId = -1;
        foreach (int id in enemies.Keys) {
            double distance = DistanceTo(enemies[id].LastX, enemies[id].LastY);
            double energy = enemies[id].LastEnergy;
            double factor = distance * energy;
            if (factor < minFactor) {
                minFactor = factor;
                closestId = id;
            }
        }
        return closestId;
    }

    private double CalcFirePower(double targetX, double targetY)
    {
        return Energy / DistanceTo(targetX, targetY) * GUN_FACTOR;
    }

    private void ShootPredict(double targetX, double targetY, double targetSpeed, double targetDirection, double firePower) {
        double bulletSpeed = CalcBulletSpeed(firePower);

        double enemyDir = targetDirection * Math.PI / 180.0;
        
        double time = DistanceTo(targetX, targetY) / bulletSpeed;
        
        double predictedX = targetX + targetSpeed * time * Math.Cos(enemyDir);
        double predictedY = targetY + targetSpeed * time * Math.Sin(enemyDir);
        
        double angleToPredicted = GunBearingTo(predictedX, predictedY);
        double angleToEnemy = GunBearingTo(targetX, targetY);
        double turn = angleToPredicted > angleToEnemy ? angleToPredicted - 2 : angleToPredicted + 2;
        // SetTurnGunLeft(GunBearingTo(targetX, targetY));
        // double turn = Math.Asin(targetSpeed / CalcBulletSpeed(firePower)) * 180 / Math.PI;
        
        SetFire(firePower);
        SetTurnGunLeft(turn);
    }

    private Point2D CalcMagicStick() {
        double x = X + (STICK_LENGTH) * Math.Cos(Direction * Math.PI / 180);
        double y = Y + (STICK_LENGTH) * Math.Sin(Direction * Math.PI / 180);
        Console.WriteLine(string.Format("stick: {0:0.00} {1:0.00} length: {2:0.00}", x, y, Math.Sqrt(Math.Pow(x - X, 2) + Math.Pow(y - Y, 2))));

        return new Point2D(x, y);
    }

}

class EnemyData {
    public double LastDirection { get; set; }
    public double LastSpeed { get; set;}
    public double LastX { get; set; }
    public double LastY { get; set; }
    public double LastEnergy { get; set; }
}

class Point2D {
    public double x;
    public double y;

    public Point2D(double x, double y) {
        this.x = x;
        this.y = y;
    }

    public bool Equals(Point2D other) {
        return this.x == other.x && this.y == other.y;
    }
}