using System;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// Movement: Anti-Gravity
// Targeting: Circular Targeting

public class Schmelly : Bot
{   
    private const double MAX_SHOOT_RANGE_THRESH = 600;
    private double Height = 600;
    private double Width = 800;
    private double WallMargin = 60;
    private double scannedEnemyX;
    private double scannedEnemyY;
    private double scannedEnemySpeed;
    private double scannedEnemyDirection = 0;
    private double scannedEnemyOldDirection = 0;
    // private double scannedPrevEnergy = 100;
    private double scannedCurrEnergy = 0;
    // private double firePower = 0;
    // static double targetDistance;
    // static double scannedEnemyDistance;
    // static int trackTargetID;
    static Dictionary<int, EnemyData> enemyData = new Dictionary<int, EnemyData>(); // Track data tank musuh

    private readonly static double  RADAR_LOCK = 0.7;
    static double GUN_FACTOR = 10;
    static double MIN_ENERGY = 10;
    static List<double> dirHistory = new List<double>();
    
    static void Main()
    {
        new Schmelly().Start();
    }

    Schmelly() : base(BotInfo.FromFile("Schmelly.json")) { }

    public override void Run() 
    {
        /* Customize bot colors, read the documentation for more information */
        BodyColor = Color.FromArgb(0xFF, 0xFF, 0x00); // Kuning
        TurretColor = Color.FromArgb(0xFF, 0xA5, 0x00); // Orange
        RadarColor = Color.FromArgb(0xFF, 0x00, 0x00); // Merah
        BulletColor = Color.FromArgb(0x8B, 0x45, 0x13); // Coklat
        ScanColor = Color.FromArgb(0x00, 0xFF, 0x00); // Hijau
        TracksColor = Color.FromArgb(0x8B, 0x45, 0x13); // Coklat
        GunColor = Color.FromArgb(0xFF, 0xFF, 0x00); // Kuning

        SetTurnRadarLeft(double.PositiveInfinity);
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        AdjustRadarForBodyTurn = true;
    }

    public override void OnTick(TickEvent e) 
    {
        Console.WriteLine("X: " + X + " Y: " + Y);
        // Anti-Gravity
        double xForce = 0;
        double yForce = 0;

        // double forceAtas = 1000 / ((Height - Y) * (Height - Y));
        // double forceBawah = 1000 / (Y * Y);
        // double forceKanan = 1000 / ((Width - X) * (Width - X));
        // double forceKiri = 1000 / (X * X);
        // Console.WriteLine("Force Atas: " + forceAtas + " Force Bawah: " + forceBawah + " Force Kanan: " + forceKanan + " Force Kiri: " + forceKiri);

        foreach (EnemyData value in enemyData.Values) {
            double enemyAbsBearing = NormalizeAbsoluteAngle((Math.Atan2(value.coordinate.X - X, value.coordinate.Y - Y)));
            double enemyDistance = getEnemyDistance(value);
            xForce -= (Math.Sin(enemyAbsBearing) / (enemyDistance * enemyDistance)) * value.enemyEnergy * 100;
            yForce -= (Math.Cos(enemyAbsBearing) / (enemyDistance * enemyDistance)) * value.enemyEnergy * 100;
        }
        // xForce -= forceKanan + forceKiri;
        // yForce -= forceAtas + forceBawah;

        if (X < WallMargin || X > Width - WallMargin) {
            xForce = 0;
        }
        if (Y < WallMargin || Y > Height - WallMargin) {
            yForce = 0;
        }
        
        Console.WriteLine("XForce: " + xForce + " YForce: " + yForce);
        double forceMagnitude = Math.Sqrt(xForce * xForce + yForce * yForce) * 100;
        Console.WriteLine("Force Magnitude: " + forceMagnitude);
        double angleRadian = Math.Atan2(xForce, yForce);
        double angle = angleRadian * (180 / Math.PI);
        Console.WriteLine("Angle: " + angle);
        if (xForce != 0 && yForce != 0) {
            Console.WriteLine(NormalizeRelativeAngle(angle - Direction));
            // SetTurnRight(NormalizeRelativeAngle(angle - Direction));
            // SetForward(forceMagnitude);
            if (Math.Abs(angle - Direction) < 90) {
                SetTurnRight(NormalizeRelativeAngle(angle - Direction));
                SetForward(100);
            } else {
                SetTurnRight(NormalizeRelativeAngle(angle + 180 - Direction));
                SetForward(100);
            }
        } else if (xForce == 0 && yForce == 0) {
            SetTurnRight(90);
            SetForward(100);
        } else if (xForce == 0 && ((Direction >= 45 && Direction <= 135) || (Direction >= 225 && Direction <= 315))) {
            SetTurnRight(135);
            SetForward(100);

        } else if (yForce == 0 && ((Direction <= 45 && Direction >= 315) || (Direction >= 135 && Direction <= 225))) {
            SetTurnRight(135);
            SetForward(100);

        } else { // Oscillate
            Forward(100);
            if (X < WallMargin || X > Width - WallMargin || Y < WallMargin || Y > Height - WallMargin) {
                SetTurnRight(90);
                SetForward(100);
            } 
            Back(50);
        }

    }

/* ================================= Event Handler ================================= */

    public override void OnScannedBot(ScannedBotEvent e)
    {
        scannedEnemyX = e.X;
        scannedEnemyY = e.Y;
        scannedEnemySpeed = e.Speed;
        scannedCurrEnergy = e.Energy;
        double enemyDistance = DistanceTo(scannedEnemyX, scannedEnemyY);
        int enemyID = e.ScannedBotId; 

        // Cari 'bearing' musuh (derajat musuh terhadap kita)
        double deltaX = scannedEnemyX - X;
        double deltaY = scannedEnemyY - Y;
        double enemyBearing = Math.Atan2(deltaY, deltaX) + (Direction * (Math.PI/180));
        
        // Catat data musuh dalam array
        if (!enemyData.ContainsKey(enemyID)) {
            enemyData.Add(enemyID, new EnemyData(scannedEnemyX + DistanceTo(scannedEnemyX, scannedEnemyY)*Math.Sin(enemyBearing), scannedEnemyY + DistanceTo(scannedEnemyX, scannedEnemyY)*Math.Cos(enemyBearing), scannedCurrEnergy));
        } else {
            enemyData[enemyID] = new EnemyData(scannedEnemyX + DistanceTo(scannedEnemyX, scannedEnemyY)*Math.Sin(enemyBearing), scannedEnemyY + DistanceTo(scannedEnemyX, scannedEnemyY)*Math.Cos(enemyBearing), scannedCurrEnergy);
        }

        // Radar lock
        double radarAngle = double.PositiveInfinity * NormalizeRelativeAngle(RadarBearingTo(e.X, e.Y));
        if (!double.IsNaN(radarAngle) && (GunHeat < RADAR_LOCK || EnemyCount == 1))
        {
            SetTurnRadarLeft(radarAngle);
        }

        // double firePower = Energy / DistanceTo(e.X, e.Y) * GUN_FACTOR;
        // if (GunTurnRemaining == 0 && (Energy > MIN_ENERGY || DistanceTo(e.X, e.Y) < 50))
        // {
        //     SetFire(firePower);
        // }

        double firePower = Energy / DistanceTo(e.X, e.Y) * GUN_FACTOR;
        if (GunTurnRemaining == 0) {
            SetFire(firePower);
        }

        double bulletSpeed = CalcBulletSpeed(firePower);
        double enemyDirection = e.Direction * (Math.PI/180);
        dirHistory.Add(enemyDirection);
        if (dirHistory.Count > 5) {
            dirHistory.RemoveAt(0);
        }

        double angularVelocity = 0;
        if (dirHistory.Count >= 2) {
            double totChange = 0;
            for (int i = 1; i < dirHistory.Count; i++) {
                double delta = dirHistory[i] - dirHistory[i-1];
                delta = (delta + Math.PI) % (2 * Math.PI) - Math.PI;
                totChange += delta;
            }
            angularVelocity = totChange / (dirHistory.Count - 1);
        }
        double predictedX = e.X;
        double predictedY = e.Y;

        int deltaTime = 0;
        while (deltaTime++ * bulletSpeed < DistanceTo(predictedX, predictedY)) {
            enemyDirection += angularVelocity;
            predictedX += scannedEnemySpeed * Math.Cos(enemyDirection);
            predictedY += scannedEnemySpeed * Math.Sin(enemyDirection);
        }

        if (predictedX < 0)
        {
            predictedX -= 1;
        } else if (predictedX > Width)
        {
            predictedX = 2 * Width - predictedX;
        }

        if (predictedY < 0)
        {
            predictedY -= 1;
        } else if (predictedY > Height)
        {
            predictedY = 2 * Height - predictedY;
        }

        SetTurnGunLeft(GunBearingTo(predictedX, predictedY));

        // Circular targeting: asumsi hanya 1 musuh
        // double enemyDirection = e.Direction;
        // double enemyDirChange = enemyDirection - scannedEnemyOldDirection;
        // scannedEnemyOldDirection = enemyDirection;

        // double deltaTime = 0;
        // double predictedX = e.X;
        // double predictedY = e.Y;
        // while (((deltaTime++) * (20.0-3.0*firePower)) < DistanceTo(predictedX, predictedY)) {
        //     predictedX += Math.Sin(enemyDirection) * scannedEnemySpeed;
        //     predictedY += Math.Cos(enemyDirection) * scannedEnemySpeed;
        //     enemyDirection += enemyDirChange;
        //     if (predictedX < 18 || predictedY < 18 || predictedX > Width-18 || predictedY > Height-18) {
        //         predictedX = Math.Min(Math.Max(18, predictedX), Width-18);
        //         predictedY = Math.Min(Math.Max(18, predictedY), Width-18);
        //         break;
        //     } 
        // }
        // SetTurnGunLeft(GunBearingTo(predictedX, predictedY));
    }

    public override void OnHitBot(HitBotEvent e)
    {
        // Console.WriteLine("Ouch! I hit a bot at " + e.X + ", " + e.Y);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        // Console.WriteLine("Ouch! I hit a wall, must turn back!");
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        int deadEnemy = e.VictimId;
        if (enemyData.ContainsKey(deadEnemy)) {
            enemyData.Remove(deadEnemy);
        }
    }

    /* ============================ Helper Functions ============================ */
    public double getEnemyDistance(EnemyData enemyData) {
        return DistanceTo(enemyData.coordinate.X, enemyData.coordinate.Y);
    }
}

public struct EnemyData {
    public Point2D coordinate;
    public double enemyEnergy;

    public EnemyData(double X, double Y, double enemyEnergy) {
        this.coordinate = new Point2D(X, Y);
        this.enemyEnergy = enemyEnergy;
    }
    public EnemyData(Point2D coordinate, double enemyEnergy) {
        this.coordinate = coordinate;
        this.enemyEnergy = enemyEnergy;
    }
}

public struct Point2D {
    public double X;
    public double Y;
    public Point2D(double X, double Y) {
        this.X = X;
        this.Y = Y;
    }
}
