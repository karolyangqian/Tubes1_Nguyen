using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// Qwuck
// ------------------------------------------------------------------
// Targeting: Head on targeting
// Movement: Minimum risk corner, wall avoidance, walking stick, and sinusoidal oscillation
// ------------------------------------------------------------------
/*

*/
// ------------------------------------------------------------------
public class Qwuck : Bot
{
    // Constants
    private const double WALL_MARGIN = 30;
    private const double CORNER_MARGIN = 60;
    private const double STICK_LENGTH = 100;
    private const double OSCILLATION_RADIUS = 60;
    private const double MAX_OSCILLATING_SPEED = 8;
    private const double MAX_SPEED = 8;
    private const double MIN_SPEED = 2;
    private const double MAX_TURN_RATE = 15;
    private const double MIN_TURN_RATE = 5;
    private double GUN_FACTOR = 5;

    // Reinitialized in Run()
    private Dictionary<int, EnemyData> enemies = new Dictionary<int, EnemyData>();
    private Random random = new Random();
    private Point2D corner = new Point2D(0, 0);
    private bool navigating;
    private bool oneVsOne;
    private int targetId;
    private bool targetLocked;
    

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
        navigating = true;
        targetId = -1;
        targetLocked = false;
        oneVsOne = false;

        double startTime = DateTime.Now.Millisecond;

        for (int i = 0; i < 8; i++) {
            TurnRadarRight(45);
        }
        Console.WriteLine(string.Format("{0}", enemies.Count));

        corner = SafestCorner();
    } 

    public override void OnTick(TickEvent e) {
        // Console.WriteLine(string.Format("Safest corner: {0:0.00} {1:0.00}", corner.x, corner.y));

        if (navigating && !WallAvoidance()) {
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
        // Console.WriteLine(string.Format("Distance to corner: {0:0.00} {1}", DistanceTo(corner.x, corner.y), navigating));

        if (DistanceTo(corner.x, corner.y) > 4 * OSCILLATION_RADIUS) {
            navigating = true;
        }
    
        if (!SafestCorner().Equals(corner)) {
            navigating = true;
            corner = SafestCorner();
        }

        if (oneVsOne) {
            if (targetLocked) {
                // Console.WriteLine(string.Format("Targeting: {0:0.00} {1:0.00}", targetId, enemies[targetId].LastEnergy));
                TrackScanAt(enemies[targetId].LastX, enemies[targetId].LastY);
                ShootPredict(targetId, CalcFirePower(enemies[targetId].LastX, enemies[targetId].LastY));
                targetLocked = false;
            } else {
                SetTurnRadarLeft(20);
            }
        } else {
            SetTurnRadarLeft(20);
            int selectId = SelectTargetEnemy();
            if (selectId != -1) {
                ShootPredict(selectId, CalcFirePower(enemies[selectId].LastX, enemies[selectId].LastY));
            }
        }
    }

    public override void OnScannedBot(ScannedBotEvent e) {
        Console.WriteLine(string.Format("Scanned: {0:0.00}", e.ScannedBotId));
        if (!enemies.ContainsKey(e.ScannedBotId))
        {
            enemies[e.ScannedBotId] = new EnemyData();
            enemies[e.ScannedBotId].LastX = e.X;
            enemies[e.ScannedBotId].LastY = e.Y;
            enemies[e.ScannedBotId].LastEnergy = e.Energy;
            enemies[e.ScannedBotId].LastSpeed = e.Speed;
            enemies[e.ScannedBotId].LastDirection = e.Direction;
            enemies[e.ScannedBotId].LastTime = DateTime.Now.Millisecond;
        }

        enemies[e.ScannedBotId].PrevX = enemies[e.ScannedBotId].LastX;
        enemies[e.ScannedBotId].PrevY = enemies[e.ScannedBotId].LastY;
        enemies[e.ScannedBotId].PrevDirection = enemies[e.ScannedBotId].LastDirection;
        enemies[e.ScannedBotId].PrevSpeed = enemies[e.ScannedBotId].LastSpeed;
        enemies[e.ScannedBotId].PrevTime = enemies[e.ScannedBotId].LastTime;

        enemies[e.ScannedBotId].LastX = e.X;
        enemies[e.ScannedBotId].LastY = e.Y;
        enemies[e.ScannedBotId].LastSpeed = e.Speed;
        enemies[e.ScannedBotId].LastDirection = e.Direction;
        enemies[e.ScannedBotId].LastEnergy = e.Energy;
        enemies[e.ScannedBotId].LastTime = DateTime.Now.Millisecond;

        // Console.WriteLine(string.Format("id: {0:0} x: {1:0.00} y: {2:0.00} energy: {3:0.00} speed: {4:0.00} dir: {5:0.00}", 
        //     e.ScannedBotId, 
        //     e.X,
        //     e.Y,
        //     e.Energy,
        //     e.Speed,
        //     e.Direction
        //     ));
        // Console.WriteLine(string.Format("count: {0:0.00}", enemies.Count));
        if (DistanceTo(e.X, e.Y) < 200 || enemies.Count == 1) {
            targetId = e.ScannedBotId;
            oneVsOne = true;
        } else {
            oneVsOne = false;
        }
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
        if (!WallAvoidance()) {
            double turn = 20 + 40 * (Math.Sin(DateTime.Now.Millisecond * 2 * Math.PI / 1400));
            // Console.WriteLine(string.Format("Turn: {0:0.00}", turn));
            Point2D walkStick = CalcStickEnd(turn);

            double newX = Math.Max(WALL_MARGIN, Math.Min(ArenaWidth - WALL_MARGIN, walkStick.x));
            double newY = Math.Max(WALL_MARGIN, Math.Min(ArenaHeight - WALL_MARGIN, walkStick.y));

            MoveTo(newX, newY, MAX_OSCILLATING_SPEED);
        }
    }

    private bool WallAvoidance() {
        Point2D frontStick = CalcStickEnd(0);
        if (IsOutsideArena(frontStick.x, frontStick.y)) {
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
        // Console.WriteLine(string.Format("MoveTo: {0:0.00} {1:0.00}", x, y));

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
            double factor = distance / energy;
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

    private void ShootPredict(int enemyId, double firePower) {
        // Linear targeting
        // LinearTargeting(targetX, targetY, targetSpeed, targetDirection, firePower);

        // MEA targeting
        // double mea = NormalizeRelativeAngle(Math.Asin(targetSpeed / CalcBulletSpeed(firePower)) * 180 / Math.PI);
        // Console.WriteLine(string.Format("MEA: {0:0.00} turn: {1:0.00}", mea, turn));

        // Head on targeting
        // HeadOnTargeting(targetX, targetY, firePower);

        // Circular targeting
        BasicCircularTargeting(enemyId, firePower);
    }

    private void BasicCircularTargeting(int enemyId, double firePower) {
        if (enemies[enemyId].LastX == enemies[enemyId].PrevX && enemies[enemyId].LastY == enemies[enemyId].PrevY) {
            HeadOnTargeting(enemies[enemyId].LastX, enemies[enemyId].LastY, firePower);
            return;
        }
        double bulletSpeed = CalcBulletSpeed(firePower);
        // double dt = enemies[enemyId].LastTime - enemies[enemyId].PrevTime;
        double dt = (Math.Sqrt(Math.Pow(enemies[enemyId].LastX - enemies[enemyId].PrevX, 2) + Math.Pow(enemies[enemyId].LastY - enemies[enemyId].PrevY, 2))) / enemies[enemyId].LastSpeed;
        double w = NormalizeRelativeAngle(enemies[enemyId].LastDirection - enemies[enemyId].PrevDirection) / dt;
        if (Math.Abs(w) < 1) {
            LinearTargeting(enemies[enemyId].LastX, enemies[enemyId].LastY, enemies[enemyId].LastSpeed, enemies[enemyId].LastDirection, firePower);
            return;
        }
        double timeBullet = DistanceTo(enemies[enemyId].LastX, enemies[enemyId].LastY) / bulletSpeed + dt;
        double r = Math.Abs(enemies[enemyId].LastSpeed / DegreesToRadians(w));
        Point2D center = CalcStickEnd(w > 0 ? 90 : -90, r, new Point2D(enemies[enemyId].LastX, enemies[enemyId].LastY));
        double theta = w * timeBullet;
        Point2D predicted = rotatePoint(new Point2D(enemies[enemyId].LastX, enemies[enemyId].LastY), center, theta);
        double turn = GunBearingTo(predicted.x, predicted.y);
        // Console.WriteLine(string.Format("Predicted: {0:0.00} {1:0.00} Turn: {2:0.00} r: {3:0.00} cx: {4:0.00} cy: {5:0.00} th: {6:0.00} w: {7:0.00}", predicted.x, predicted.y, turn, r, center.x, center.y, theta, w));
        SetTurnGunLeft(turn);
        SetFire(firePower);
    }

    private void LinearTargeting(double targetX, double targetY, double targetSpeed, double targetDirection, double firePower) {
        double vb = CalcBulletSpeed(firePower);
        // double time = DistanceTo(targetX, targetY) / bulletSpeed;
        // double time = DistanceTo(targetX, targetY) / Math.Abs(bulletSpeed - targetSpeed);
        double vxt = targetSpeed * Math.Cos(DegreesToRadians(targetDirection));
        double vyt = targetSpeed * Math.Sin(DegreesToRadians(targetDirection));
        double xt = targetX;
        double yt = targetY;
        double a = Math.Pow(vxt, 2) + Math.Pow(vyt, 2) - Math.Pow(vb, 2);
        double b = 2 * (vxt * (xt - X) + vyt * (yt - Y));
        double c = Math.Pow(xt - X, 2) + Math.Pow(yt - Y, 2);
        double d = Math.Pow(b, 2) - 4 * a * c;
        if (d < 0) {
            HeadOnTargeting(targetX, targetY, firePower);
            return;
        }

        double t1 = (-b + Math.Sqrt(d)) / (2 * a);
        double t2 = (-b - Math.Sqrt(d)) / (2 * a);
        double time = Math.Min(t1 > 0 ? t1 : double.PositiveInfinity, t2 > 0 ? t2 : double.PositiveInfinity);


        double predictedX = targetX + targetSpeed * time * Math.Cos(DegreesToRadians(targetDirection));
        double predictedY = targetY + targetSpeed * time * Math.Sin(DegreesToRadians(targetDirection));

        if (IsOutsideArena(predictedX, predictedY)) {
            double m = (predictedY - targetX) / (predictedX - targetY);
            double k = predictedY - m * predictedX;

            if (predictedX < 0) {
                predictedX = WALL_MARGIN;
                predictedY = m * predictedX + k;
            } else if (predictedX > ArenaWidth) {
                predictedX = ArenaWidth - WALL_MARGIN;
                predictedY = m * predictedX + k;
            } else if (predictedY < 0) {
                predictedY = 0 + WALL_MARGIN;
                predictedX = (predictedY - k) / m;
            } else if (predictedY > ArenaHeight) {
                predictedY = ArenaHeight - WALL_MARGIN;
                predictedX = (predictedY - k) / m;
            }
        }

        double angleToPredicted = GunBearingTo(predictedX, predictedY);
        double angleToEnemy = GunBearingTo(targetX, targetY);
        // double turn = angleToPredicted > angleToEnemy ? angleToPredicted - 2 : angleToPredicted + 2;
        SetTurnGunLeft(angleToPredicted);
        SetFire(firePower);
        Console.WriteLine(string.Format("Predicted: {0:0.00} {1:0.00} t1: {2:0.00} t2: {3:0.00}", predictedX, predictedY, t1, t2));
    }

    private void HeadOnTargeting(double targetX, double targetY, double firePower) {
        double turn = GunBearingTo(targetX, targetY);
        SetTurnGunLeft(turn);
        SetFire(firePower);
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

    private Point2D CalcStickEnd(double angle, double length = STICK_LENGTH, Point2D center = null) {
        if (center == null) {
            center = new Point2D(X, Y);
        }
        double x = center.x + (STICK_LENGTH) * Math.Cos(DegreesToRadians(Direction + angle));
        double y = center.y + (STICK_LENGTH) * Math.Sin(DegreesToRadians(Direction + angle));
        return new Point2D(x, y);
    }

    private double DegreesToRadians(double degrees) {
        return degrees * Math.PI / 180;
    }

    private double RadiansToDegrees(double radians) {
        return radians * 180 / Math.PI;
    }

    private Point2D rotatePoint(Point2D point, Point2D center, double angle) {
        double x = center.x + (point.x - center.x) * Math.Cos(DegreesToRadians(angle)) - (point.y - center.y) * Math.Sin(DegreesToRadians(angle));
        double y = center.y + (point.x - center.x) * Math.Sin(DegreesToRadians(angle)) + (point.y - center.y) * Math.Cos(DegreesToRadians(angle));
        return new Point2D(x, y);
    }

}

class EnemyData {
    public double LastX { get; set; }
    public double LastY { get; set; }
    public double LastDirection { get; set; }
    public double LastSpeed { get; set;}
    public double LastEnergy { get; set; }
    public double LastTime { get; set; }
    public double PrevX { get; set; }
    public double PrevY { get; set; }
    public double PrevDirection { get; set; }
    public double PrevSpeed { get; set; }
    public double PrevTime { get; set; }
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