using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// Qwuck
// ------------------------------------------------------------------
// Targeting: Circular
// Movement:  
// ------------------------------------------------------------------
/*

*/
// ------------------------------------------------------------------
public class Qwuck : Bot
{
    static double WALL_MARGIN = 30;
    static double CORNER_MARGIN = 60;
    static double STICK_LENGTH = 100;
    static double OSCILLATION_RADIUS = 60;
    static double MAX_SPEED = 8;
    static double MIN_SPEED = 2;
    private const double MAX_TURN_RATE = 15;
    private const double MIN_TURN_RATE = 5;
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
    static double oscillatingSpeed = 8;
    



    static void Main()
    {
        new Qwuck().Start();
    }

    Qwuck() : base(BotInfo.FromFile("Qwuck.json")) { }

    public override void Run()
    {
        Console.WriteLine("Hello! I'm Qwuck!");
        
        BodyColor = Color.FromArgb(0x00, 0x64, 0x64); // Dark cyan
        TurretColor = Color.Yellow;
        RadarColor = Color.FromArgb(0x00, 0xC8, 0x00);   // lime
        BulletColor = Color.FromArgb(0x00, 0x96, 0x32); // green
        ScanColor = Color.Red;

        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        AdjustRadarForBodyTurn = true;

        MaxSpeed = MAX_SPEED;

        for (int i = 0; i < 8; i++) {
            TurnRadarRight(45);
        }
        Console.WriteLine(string.Format("{0}", enemies.Count));
        AdjustGunForBodyTurn = true;
        navigating = true;

        corner = SafestCorner();
    } 

    public override void OnTick(TickEvent e) {
        // Console.WriteLine(string.Format("Safest corner: {0:0.00} {1:0.00}", corner.x, corner.y));

        if (navigating && !WallSmoothing()) {
            // Console.WriteLine(string.Format("Distance to corner: {0:0.00} {1}", DistanceTo(corner.x, corner.y), navigating));
            double turn = BearingTo(corner.x, corner.y);
            Point2D magicStick = CalcStickEnd(turn + 40 * (Math.Sin(DateTime.Now.Millisecond * 2 * Math.PI / 1100)));
            MoveTo(magicStick.x, magicStick.y);
            if (DistanceTo(corner.x, corner.y) < OSCILLATION_RADIUS) {
                TargetSpeed = 0;
                // SetTurnLeft(90);
                navigating = false;
            }
        } else {
            Oscillate();
        }
        Console.WriteLine(string.Format("Distance to corner: {0:0.00} {1}", DistanceTo(corner.x, corner.y), navigating));

        if (DistanceTo(corner.x, corner.y) > 4 * OSCILLATION_RADIUS) {
            navigating = true;
        }
    
        if (!SafestCorner().Equals(corner)) {
            navigating = true;
            corner = SafestCorner();
        }

        if (oneVsOne) {
            if (targetLocked) {
                Console.WriteLine(string.Format("Targeting: {0:0.00} {1:0.00}", targetId, enemies[targetId].LastEnergy));
                TrackScanAt(enemies[targetId].LastX, enemies[targetId].LastY);
                ShootPredict(enemies[targetId].LastX, enemies[targetId].LastY, enemies[targetId].LastSpeed, enemies[targetId].LastDirection, CalcFirePower(enemies[targetId].LastX, enemies[targetId].LastY));
                targetLocked = false;
            } else {
                SetTurnRadarLeft(20);
            }
        } else {
            SetTurnRadarLeft(20);
            int selectId = SelectTargetEnemy();
            if (selectId != -1) {
                ShootPredict(enemies[selectId].LastX, enemies[selectId].LastY, enemies[selectId].LastSpeed, enemies[selectId].LastDirection, CalcFirePower(enemies[selectId].LastX, enemies[selectId].LastY));
            }
        }
    }

    public override void OnScannedBot(ScannedBotEvent e) {
        if (!enemies.ContainsKey(e.ScannedBotId))
        {
            enemies[e.ScannedBotId] = new EnemyData();
        }

        enemies[e.ScannedBotId].LastX = e.X;
        enemies[e.ScannedBotId].LastY = e.Y;
        enemies[e.ScannedBotId].LastEnergy = e.Energy;
        enemies[e.ScannedBotId].LastSpeed = e.Speed;
        enemies[e.ScannedBotId].LastDirection = e.Direction;

        // Console.WriteLine(string.Format("id: {0:0} x: {1:0.00} y: {2:0.00} energy: {3:0.00} speed: {4:0.00} dir: {5:0.00}", 
        //     e.ScannedBotId, 
        //     e.X,
        //     e.Y,
        //     e.Energy,
        //     e.Speed,
        //     e.Direction
        //     ));
        // Console.WriteLine(string.Format("count: {0:0.00}", enemies.Count));
        // if (DistanceTo(e.X, e.Y) < 200) {
        //     targetId = e.ScannedBotId;
        //     oneVsOne = true;
        // } else {
        //     oneVsOne = false;
        // }
        if (e.ScannedBotId == targetId) {
            targetLocked = true;
        }
    }

    public override void OnBotDeath(BotDeathEvent e) {
        if (enemies.ContainsKey(e.VictimId))
        {
            enemies.Remove(e.VictimId);
            if (e.VictimId == targetId) {
                oneVsOne = false;
                targetId = -1;
            }
            if (enemies.Count == 1) {
                targetId = enemies.Keys.First();
                oneVsOne = true;
            }
        }
    }

    public override void OnHitWall(HitWallEvent botHitWallEvent) {
        // Console.WriteLine("Hit Wallllllllllllllllllllllllll");
    }


// ========================== METHODS ===============================

    private void Oscillate()
    {
        // TargetSpeed = OSCILLATION_RADIUS * TurnRate * Math.PI / 180;
        // TurnRate = 7 +  5 * Math.Sin(DateTime.UtcNow.Millisecond);
        // double v = 3;

        if (!WallSmoothing()) {
            double turn = 20 + 40 * (Math.Sin(DateTime.Now.Millisecond * 2 * Math.PI / 1400));
            Console.WriteLine(string.Format("Turn: {0:0.00}", turn));
            Point2D walkStick = CalcStickEnd(turn);
            // double theta = 15 * Math.PI / 180;
            // double newX = (walkStick.x - X) * Math.Cos(theta) - (walkStick.y - Y) * Math.Sin(theta) + X;
            // double newY = (walkStick.y - Y) * Math.Cos(theta) + (walkStick.x - Y) * Math.Sin(theta) + Y;

            double newX = Math.Max(WALL_MARGIN, Math.Min(ArenaWidth - WALL_MARGIN, walkStick.x));
            double newY = Math.Max(WALL_MARGIN, Math.Min(ArenaHeight - WALL_MARGIN, walkStick.y));

            double a = 3;
            MoveTo(newX, newY, oscillatingSpeed);
            // double newSpeed = TargetSpeed + random.NextDouble()*2 * (random.NextDouble() > 0.4 ? 1 : -1);
            // if (newSpeed > MIN_SPEED && newSpeed < MAX_SPEED) {
            //     TargetSpeed = newSpeed;
            // }

            // double newTurnRate = TurnRate + random.NextDouble()*2 * (random.NextDouble() > 0.4 ? 1 : -1);
            // if (newTurnRate > MIN_TURN_RATE && newTurnRate < MAX_TURN_RATE) {
            //     TurnRate = newTurnRate;
            // }
            // Console.WriteLine("newSpeed: {0:0.00} newTR: {1:0.00}", newSpeed, newTurnRate);

        }


        // if (IsOutsideArena(magicStick.x, magicStick.y)) {
        //     TargetSpeed = Speed * -1;
        // } else {
        //     TargetSpeed += 2.5 * random.NextDouble() * (random.NextDouble() > 0.4 ? 1 : -1);
        //     if (Math.Abs(TargetSpeed) < minSpeed) TargetSpeed = minSpeed * (random.NextDouble() > 0.4 ? 1 : -1);
        //     TurnRate = TargetSpeed * 180 / Math.PI / (OSCILLATION_RADIUS - random.NextDouble() * (OSCILLATION_RADIUS-10));
        // }
        // if (IsOutsideArena(magicStick.x, magicStick.y)) {
        //     // TurnRate += wallSmoothTurnIncr * (TurnRate > 0 ? 1 : -1);
        //     // TargetSpeed -= wallSmoothSpeedIncr * (TargetSpeed > 0 ? 1 : -1);
        //     // TurnRate = 15;
        //     // if (TurnRate > 0) {
        //     //     SetTurnLeft(10);
        //     // } else {
        //     //     SetTurnLeft(-10);
        //     // }
        //     TargetSpeed = 2;
        //     // Forward(-50);
        // } else {
        // }
        // TargetSpeed += 2.5 * random.NextDouble() * (random.NextDouble() > 0.4 ? 1 : -1);
        // if (Math.Abs(TargetSpeed) < minSpeed) TargetSpeed = minSpeed * (random.NextDouble() > 0.4 ? 1 : -1);
        // TurnRate = TargetSpeed * 180 / Math.PI / (OSCILLATION_RADIUS - random.NextDouble() * (OSCILLATION_RADIUS-10));
    }

    private bool WallSmoothing() {
        Point2D frontStick = CalcStickEnd(0);
        if (IsOutsideArena(frontStick.x, frontStick.y)) {
            // if (IsACorner(frontStick.x, frontStick.y, WALL_MARGIN)) {
            //     MoveTo(ArenaWidth / 2, ArenaHeight / 2);
            //     return true;
            // } 
            frontStick.x = Math.Max(WALL_MARGIN, Math.Min(ArenaWidth - WALL_MARGIN, frontStick.x));
            frontStick.y = Math.Max(WALL_MARGIN, Math.Min(ArenaHeight - WALL_MARGIN, frontStick.y));
            
            var (stickL, stickR) = CalcMagicStick();
            double L = BearingTo(stickL.x, stickL.y);
            double R = BearingTo(stickR.x, stickR.y);
            double F = BearingTo(frontStick.x, frontStick.y);

            if (DistanceTo(stickL.x, stickL.y) < STICK_LENGTH && DistanceTo(stickR.x, stickR.y) < STICK_LENGTH) {
                MoveTo(ArenaWidth / 2, ArenaHeight / 2);
                return true;
            }

            // Console.WriteLine(string.Format("L: {0:0.00} R: {1:0.00} F: {2:0.00}", L, R, F));

            double angleL = Math.Abs(L - F);
            double angleR = Math.Abs(R - F);

            // Console.WriteLine(string.Format("angleL: {0:0.00} angleR: {1:0.00}", angleL, angleR));


            if (Math.Abs(angleL) < Math.Abs(angleR)) {
                MoveTo(stickL.x, stickL.y);
            } else {
                MoveTo(stickR.x, stickR.y);
            }
            return true;
        }
        return false;
    }

    private bool IsACorner(double x, double y, double margin) {
        return (x < margin && y < margin) || (x < margin && y > ArenaHeight - margin) || (x > ArenaWidth - margin && y < margin) || (x > ArenaWidth - margin && y > ArenaHeight - margin);
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

    private void MoveTo(double x, double y, double vel = 8) {
        double turn = vel > 0 ? BearingTo(x, y) : 180 - BearingTo(x, y);
        // Console.WriteLine(string.Format("Turn: {0:0.00}", turn));
        vel = Math.Abs(vel);
        SetTurnLeft(turn);
        double turnRadius = Math.Abs((180 - Math.Abs(turn)) / 180 * vel / (TurnRate == 0 ? 1 : TurnRate));
        double dist = DistanceTo(x, y);
        if (Math.Abs(turn) < 30 && dist < STICK_LENGTH) {
            TargetSpeed = vel * dist / STICK_LENGTH;
        } else {
            TargetSpeed = Math.Abs(turn != 0 ? TurnRate * turnRadius : vel);
        }
        Console.WriteLine(string.Format("MoveTo: {0:0.00} {1:0.00}", x, y));

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
        // double turn = GunBearingTo(targetX, targetY);
        // SetTurnGunLeft(turn);
        // double mea = NormalizeRelativeAngle(Math.Asin(targetSpeed / CalcBulletSpeed(firePower)) * 180 / Math.PI);
        // Console.WriteLine(string.Format("MEA: {0:0.00} turn: {1:0.00}", mea, turn));

        SetFire(firePower);
        SetTurnGunLeft(turn);
    }

    private (Point2D, Point2D) CalcMagicStick() {
        Point2D left = CalcStickEnd(90);
        Point2D right = CalcStickEnd(-90);

        left.x = Math.Max(WALL_MARGIN, Math.Min(ArenaWidth - WALL_MARGIN, left.x));
        left.y = Math.Max(WALL_MARGIN, Math.Min(ArenaHeight - WALL_MARGIN, left.y));
        right.x = Math.Max(WALL_MARGIN, Math.Min(ArenaWidth - WALL_MARGIN, right.x));
        right.y = Math.Max(WALL_MARGIN, Math.Min(ArenaHeight - WALL_MARGIN, right.y));

        return (left, right);
    }

    private Point2D CalcStickEnd(double angle) {
        double x = X + (STICK_LENGTH) * Math.Cos((Direction + angle) * Math.PI / 180);
        double y = Y + (STICK_LENGTH) * Math.Sin((Direction + angle) * Math.PI / 180);
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