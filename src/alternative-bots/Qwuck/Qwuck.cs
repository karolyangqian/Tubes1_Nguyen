using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// Qwuck
// ------------------------------------------------------------------
// Targeting: Circular and linear targeting
// Movement: Minimum risk corner, wall avoidance, walking stick, and sinusoidal oscillation
// ------------------------------------------------------------------
/*
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣤⡶⠿⠿⠷⣶⣄⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣰⡿⠁⠀⠀⢀⣀⡀⠙⣷⡀⠀⠀⠀
⠀⠀⠀⡀⠀⠀⠀⠀⠀⢠⣿⠁⠀⠀⠀⠘⠿⠃⠀⢸⣿⣿⣿⣿
⠀⣠⡿⠛⢷⣦⡀⠀⠀⠈⣿⡄⠀⠀⠀⠀⠀⠀⠀⣸⣿⣿⣿⠟
⢰⡿⠁⠀⠀⠙⢿⣦⣤⣤⣼⣿⣄⠀⠀⠀⠀⠀⢴⡟⠛⠋⠁⠀
⣿⠇⠀⠀⠀⠀⠀⠉⠉⠉⠉⠉⠁⠀⠀⠀⠀⠀⠈⣿⡀⠀⠀⠀
⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢹⡇⠀⠀⠀
⣿⡆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣼⡇⠀⠀⠀
⠸⣷⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⡿⠀⠀⠀⠀
⠀⠹⣷⣤⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣰⡿⠁⠀⠀⠀⠀
⠀⠀⠀⠉⠙⠛⠿⠶⣶⣶⣶⣶⣶⠶⠿⠟⠛⠉⠀⠀⠀⠀⠀⠀
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
    private Dictionary<int, EnemyData> enemies =  new Dictionary<int, EnemyData>();
    private Random random = new Random();
    private Point2D corner = new Point2D(CORNER_MARGIN, CORNER_MARGIN);
    private bool navigating;
    private bool oneVsOne;
    private int targetId;
    private bool targetLocked;
    private bool isScanningAll;
    private Queue <Point2D> trail = new Queue<Point2D>();
    

    static void Main()
    {
        new Qwuck().Start();
    }

    Qwuck() : base(BotInfo.FromFile("Qwuck.json")) { }

    public override void Run()
    {
        Console.WriteLine("Hello! I'm Qwuck!");
        
        // Warna robot
        BodyColor = Color.FromArgb(0x00, 0x64, 0x64); // Dark cyan
        TurretColor = Color.Yellow;
        RadarColor = Color.FromArgb(0x00, 0xC8, 0x00);   // lime
        BulletColor = Color.FromArgb(0x00, 0x96, 0x32); // green
        ScanColor = Color.FromArgb(0x00, 0xC8, 0x00); // lime

        // Membuat putaran setiap komponen independen
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        AdjustRadarForBodyTurn = true;

        // Reiinitialize variables
        enemies = new Dictionary<int, EnemyData>();
        MaxSpeed = MAX_SPEED;
        navigating = true;
        targetId = -1;
        targetLocked = false;
        oneVsOne = false;
        isScanningAll = true;

        // Scan semua musuh
        for (int i = 0; i < 8; i++) {
            TurnRadarRight(45);
        }

        isScanningAll = false;

        // Cari corner teraman
        corner = new Point2D(CORNER_MARGIN, CORNER_MARGIN);
        corner = SafestCorner();
    }

    public override void OnTick(TickEvent e) 
    {
        // Update trail
        trail.Enqueue(new Point2D(X, Y));
        if (trail.Count > 50) {
            trail.Dequeue();
        }
        foreach (Point2D point in trail) {
            Graphics.FillEllipse(new SolidBrush(Color.FromArgb(0x00, 0x64, 0x64)), (float) point.x, (float) point.y, 2, 2);
        }
        // Movement 
        if (navigating && !WallAvoidance()) {
            // Bergerak dengan lintasan sinusoidal menuju corner teraman
            double turn = BearingTo(corner.x, corner.y);
            Point2D walkingStick = CalcStickEnd(turn + 40 * (Math.Sin(TurnNumber * 2 * Math.PI / 40)), Direction);
            MoveTo(walkingStick.x, walkingStick.y);
            if (DistanceTo(corner.x, corner.y) < OSCILLATION_RADIUS) {
                TargetSpeed = 0;
                navigating = false;
            }
        } else {
            // Berputar di corner teraman
            Oscillate();
        }

        // Kembali ke corner teraman jika terlalu jauh atau corner tidak aman
        if (DistanceTo(corner.x, corner.y) > 4 * OSCILLATION_RADIUS) {
            navigating = true;
        }
    
        if (!SafestCorner().Equals(corner)) {
            navigating = true;
            corner = SafestCorner();
        }

        // Targeting
        if (oneVsOne) {
            // Jika hanya ada satu musuh, lakukan locking scan
            if (targetLocked) {
                TrackScanAt(enemies[targetId].LastX, enemies[targetId].LastY);
                CircularTargetingConditional(targetId, CalcFirePower(enemies[targetId].LastX, enemies[targetId].LastY));
                targetLocked = false;
            } else {
                SetTurnRadarLeft(20);
            }
        } else {
            // Jika ada lebih dari satu musuh, scan semua musuh dan pilih target terdekat
            SetTurnRadarLeft(20);
            int selectId = SelectTargetEnemy();
            if (selectId != -1) {
                CircularTargetingConditional(selectId, CalcFirePower(enemies[selectId].LastX, enemies[selectId].LastY));
            }
        }
    }

    public override void OnScannedBot(ScannedBotEvent e) 
    {
        // Menyimpan informasi setiap robot yang terdeteksi
        if (!enemies.ContainsKey(e.ScannedBotId))
        {
            enemies[e.ScannedBotId] = new EnemyData();
            enemies[e.ScannedBotId].LastX = e.X;
            enemies[e.ScannedBotId].LastY = e.Y;
            enemies[e.ScannedBotId].LastEnergy = e.Energy;
            enemies[e.ScannedBotId].LastSpeed = e.Speed;
            enemies[e.ScannedBotId].LastDirection = e.Direction;
            enemies[e.ScannedBotId].LastTurnNumber = TurnNumber;
        }

        enemies[e.ScannedBotId].PrevX = enemies[e.ScannedBotId].LastX;
        enemies[e.ScannedBotId].PrevY = enemies[e.ScannedBotId].LastY;
        enemies[e.ScannedBotId].PrevDirection = enemies[e.ScannedBotId].LastDirection;
        enemies[e.ScannedBotId].PrevSpeed = enemies[e.ScannedBotId].LastSpeed;
        enemies[e.ScannedBotId].PrevTurnNumber = enemies[e.ScannedBotId].LastTurnNumber;

        enemies[e.ScannedBotId].LastX = e.X;
        enemies[e.ScannedBotId].LastY = e.Y;
        enemies[e.ScannedBotId].LastSpeed = e.Speed;
        enemies[e.ScannedBotId].LastDirection = e.Direction;
        enemies[e.ScannedBotId].LastEnergy = e.Energy;
        enemies[e.ScannedBotId].LastTurnNumber = TurnNumber;

        // Memilih target jika hanya ada satu musuh atau musuh yang terdeteksi terlalu dekat
        if ((DistanceTo(e.X, e.Y) < 200 || enemies.Count == 1) && !isScanningAll) {
            targetId = e.ScannedBotId;
            oneVsOne = true;
        } else {
            oneVsOne = false;
        }

        // Lock target
        if (e.ScannedBotId == targetId) {
            targetLocked = true;
        }
    }

    public override void OnBotDeath(BotDeathEvent e) 
    {
        // Menghapus musuh yang mati dari daftar musuh
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


// ========================== METHODS ===============================

    // Bergerak dengan lintasan  melingkar sinusoidal
    private void Oscillate()
    {
        if (!WallAvoidance()) {
            double turn = 20 + 40 * (Math.Sin(TurnNumber * 2 * Math.PI / 45));
            Point2D walkStick = CalcStickEnd(turn, Direction);

            double newX = Math.Max(WALL_MARGIN, Math.Min(ArenaWidth - WALL_MARGIN, walkStick.x));
            double newY = Math.Max(WALL_MARGIN, Math.Min(ArenaHeight - WALL_MARGIN, walkStick.y));

            MoveTo(newX, newY, MAX_OSCILLATING_SPEED);
        }
    }

    // Menghindari dinding dengan teknik walking stick
    private bool WallAvoidance() 
    {
        Point2D frontStick = CalcStickEnd(0, Direction);
        if (IsOutsideArena(frontStick.x, frontStick.y)) {
            frontStick.x = Math.Max(WALL_MARGIN, Math.Min(ArenaWidth - WALL_MARGIN, frontStick.x));
            frontStick.y = Math.Max(WALL_MARGIN, Math.Min(ArenaHeight - WALL_MARGIN, frontStick.y));
            
            var (stickL, stickR) = CalcWalkingStick();
            double L = BearingTo(stickL.x, stickL.y);
            double R = BearingTo(stickR.x, stickR.y);
            double F = BearingTo(frontStick.x, frontStick.y);

            Graphics.DrawEllipse(new Pen(Color.FromArgb(0x00, 0x64, 0x64), 2), (float) stickL.x, (float) stickL.y, 20, 20);
            Graphics.DrawEllipse(new Pen(Color.FromArgb(0x00, 0x64, 0x64), 2), (float) stickR.x, (float) stickR.y, 20, 20);
            Graphics.DrawEllipse(new Pen(Color.FromArgb(0x00, 0x64, 0x64), 2), (float) frontStick.x, (float) frontStick.y, 20, 20);

            if (DistanceTo(stickL.x, stickL.y) < STICK_LENGTH && DistanceTo(stickR.x, stickR.y) < STICK_LENGTH) {
                MoveTo(ArenaWidth / 2, ArenaHeight / 2);
                return true;
            }

            double angleL = Math.Abs(L - F);
            double angleR = Math.Abs(R - F);

            if (Math.Abs(angleL) < Math.Abs(angleR)) {
                MoveTo(stickL.x, stickL.y);
            } else {
                MoveTo(stickR.x, stickR.y);
            }
            return true;
        }
        return false;
    }

    private bool IsACorner(double x, double y, double margin) 
    {
        return (x < margin && y < margin) || (x < margin && y > ArenaHeight - margin) || (x > ArenaWidth - margin && y < margin) || (x > ArenaWidth - margin && y > ArenaHeight - margin);
    }
    private bool IsCloseToWall()
    {
        return X < WALL_MARGIN || X > ArenaWidth - WALL_MARGIN || Y < WALL_MARGIN || Y > ArenaHeight - WALL_MARGIN;
    }

    private bool IsOutsideArena(double x, double y) 
    {
        return x < 0 || x > ArenaWidth || y < 0 || y > ArenaHeight;
    }

    // Mencari corner teraman dengan menghitung jarak rata-rata dari musuh ke setiap corner
    private Point2D SafestCorner() 
    {
        Point2D[] corners = new Point2D[] {
            new Point2D(CORNER_MARGIN, CORNER_MARGIN),
            new Point2D(CORNER_MARGIN, ArenaHeight - CORNER_MARGIN),
            new Point2D(ArenaWidth - CORNER_MARGIN, CORNER_MARGIN),
            new Point2D(ArenaWidth - CORNER_MARGIN, ArenaHeight - CORNER_MARGIN)
        };
        Point2D safest = new Point2D(CORNER_MARGIN, CORNER_MARGIN);
        double maxDistance = 0;
        double sumDistance = 0;
        foreach (Point2D corner in corners) {
            foreach (EnemyData enemy in enemies.Values) {
                sumDistance += Math.Sqrt(Math.Pow(corner.x - enemy.LastX, 2) + Math.Pow(corner.y - enemy.LastY, 2));
            }
            sumDistance /= enemies.Count > 0 ? enemies.Count : 1;
            if (sumDistance > maxDistance) {
                maxDistance = sumDistance;
                safest = corner;
            }
            sumDistance = 0;
        }

        return safest;
    }

    // Bergerak menuju sebuah titik 
    private void MoveTo(double x, double y, double vel = 8) 
    {
        double turn = vel > 0 ? BearingTo(x, y) : 180 - BearingTo(x, y);
        vel = Math.Abs(vel);
        SetTurnLeft(turn);
        double turnRadius = Math.Abs((180 - Math.Abs(turn)) / 180 * vel / (TurnRate == 0 ? 1 : TurnRate));
        double dist = DistanceTo(x, y);
        if (Math.Abs(turn) < 30 && dist < STICK_LENGTH) {
            TargetSpeed = vel * dist / STICK_LENGTH;
        } else {
            TargetSpeed = Math.Abs(turn != 0 ? TurnRate * turnRadius : vel);
        }
    }

    private void TrackScanAt(double x, double y) 
    {
        var bearingFromRadar = NormalizeRelativeAngle(RadarBearingTo(x, y));
        SetTurnRadarLeft(bearingFromRadar + (bearingFromRadar > 0 ? 20 : -20));
    }

    private int SelectTargetEnemy() 
    {
        double minFactor = double.PositiveInfinity;
        int closestId = -1;
        foreach (int id in enemies.Keys) {
            double distance = DistanceTo(enemies[id].LastX, enemies[id].LastY);
            double factor = distance;
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

    // Circular targeting dengan kondisi
    private void CircularTargetingConditional(int enemyId, double firePower) 
    {
        if (enemies[enemyId].LastX == enemies[enemyId].PrevX && enemies[enemyId].LastY == enemies[enemyId].PrevY) {
            HeadOnTargeting(enemies[enemyId].LastX, enemies[enemyId].LastY, firePower);
            return;
        }
        double bulletSpeed = CalcBulletSpeed(firePower);
        double dt = enemies[enemyId].LastTurnNumber - enemies[enemyId].PrevTurnNumber;
        double w = NormalizeRelativeAngle(enemies[enemyId].LastDirection - enemies[enemyId].PrevDirection) / dt;
        if (Math.Abs(w) < 1) {
            LinearTargeting(enemies[enemyId].LastX, enemies[enemyId].LastY, enemies[enemyId].LastSpeed, enemies[enemyId].LastDirection, firePower);
            return;
        }
        double r = Math.Abs(enemies[enemyId].LastSpeed / DegreesToRadians(w));
        Point2D center = CalcStickEnd(w > 0 ? 90 : -90, enemies[enemyId].LastDirection, r, new Point2D(enemies[enemyId].LastX, enemies[enemyId].LastY));
        double timeBullet = DistanceTo(center.x, center.y) / bulletSpeed;
        double theta = w * timeBullet;
        Point2D predicted = rotatePoint(new Point2D(enemies[enemyId].LastX, enemies[enemyId].LastY), center, theta);
        double turn = GunBearingTo(predicted.x, predicted.y);
        SetTurnGunLeft(turn);
        SetFire(firePower);
    }


    // Linear targeting menggunakan solusi persamaan kuadratik dari persamaan gerak
    private void LinearTargeting(double targetX, double targetY, double targetSpeed, double targetDirection, double firePower) 
    {
        double vb = CalcBulletSpeed(firePower);
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
        SetTurnGunLeft(angleToPredicted);
        SetFire(firePower);
    }

    // Head on targeting
    private void HeadOnTargeting(double targetX, double targetY, double firePower) 
    {
        double turn = GunBearingTo(targetX, targetY);
        SetTurnGunLeft(turn);
        SetFire(firePower);
    }

    // Menghitung stick yang digunakan untuk wall avoidance
    private (Point2D, Point2D) CalcWalkingStick() 
    {
        Point2D left = CalcStickEnd(90, Direction);
        Point2D right = CalcStickEnd(-90, Direction);

        left.x = Math.Max(WALL_MARGIN, Math.Min(ArenaWidth - WALL_MARGIN, left.x));
        left.y = Math.Max(WALL_MARGIN, Math.Min(ArenaHeight - WALL_MARGIN, left.y));
        right.x = Math.Max(WALL_MARGIN, Math.Min(ArenaWidth - WALL_MARGIN, right.x));
        right.y = Math.Max(WALL_MARGIN, Math.Min(ArenaHeight - WALL_MARGIN, right.y));

        return (left, right);
    }

    // Menghitung ujung stick
    private Point2D CalcStickEnd(double angle, double heading, double length = STICK_LENGTH, Point2D center = null) 
    {
        if (center == null) {
            center = new Point2D(X, Y);
        }
        double x = center.x + (STICK_LENGTH) * Math.Cos(DegreesToRadians(heading + angle));
        double y = center.y + (STICK_LENGTH) * Math.Sin(DegreesToRadians(heading + angle));

        return new Point2D(x, y);
    }

    private double DegreesToRadians(double degrees) 
    {
        return degrees * Math.PI / 180;
    }

    private double RadiansToDegrees(double radians) 
    {
        return radians * 180 / Math.PI;
    }

    private Point2D rotatePoint(Point2D point, Point2D center, double angle) 
    {
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
    public double LastTurnNumber { get; set; }
    public double PrevX { get; set; }
    public double PrevY { get; set; }
    public double PrevDirection { get; set; }
    public double PrevSpeed { get; set; }
    public double PrevTurnNumber { get; set; }
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