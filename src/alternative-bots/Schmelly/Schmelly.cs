using System;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// Movement: Anti-Gravity
// Targeting: Circular Targeting

public class Schmelly : Bot
{   
    private double Height = 600;
    private double Width = 800;
    private double CenterX = 400;
    private double CenterY = 300;
    private double WallMargin = 100; // Batas jark dari dinding yang "dekat"
    private double scannedEnemyX;
    private double scannedEnemyY;
    private double scannedEnemySpeed;
    private double scannedEnemyDirection;
    private double scannedCurrEnergy = 0;
    private bool enemyDetected = false;

    private double firePower;

    private readonly static double  RADAR_LOCK = 0.7;
    private static double GUN_FACTOR = 5;
    private static EnemyData enemyData;

    // Dictionary<int, EnemyData> enemyData = new Dictionary<int, EnemyData>(); // Track data tank musuh
    
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
        if (enemyDetected) {
            enemyDetected = false;
            if (isNearWall()) {
                moveToCenter();
            } else {
                antiGravity();
            }
        } 
    }

/* ================================= Event Handler ================================= */

    public override void OnScannedBot(ScannedBotEvent e)
    {
        // Catat data musuh yang di-scan
        scannedEnemyX = e.X;
        scannedEnemyY = e.Y;
        scannedEnemySpeed = e.Speed;
        scannedCurrEnergy = e.Energy;
        scannedEnemyDirection = e.Direction;
        enemyDetected = true;
        int enemyID = e.ScannedBotId; 

        // Cari 'bearing' musuh (derajat musuh terhadap kita)
        double enemyBearing = getEnemyBearing(scannedEnemyX, scannedEnemyY);
        
        // Catat data musuh dalam dictionary
        enemyData = new EnemyData(scannedEnemyX + DistanceTo(scannedEnemyX, scannedEnemyY)*Math.Sin(enemyBearing), scannedEnemyY + DistanceTo(scannedEnemyX, scannedEnemyY)*Math.Cos(enemyBearing), scannedEnemyDirection, scannedEnemySpeed, scannedCurrEnergy);

        // Radar lock
        setRadarTurn(scannedEnemyX, scannedEnemyY);

        // Atur firepower tank
        setGunFire(scannedEnemyX, scannedEnemyY);

        // Targeting
        LinearTarget(scannedEnemyX, scannedEnemyY, scannedEnemySpeed, scannedEnemyDirection, firePower);
    }

    /* ============================ Helper Functions ============================ */
    // Cari bearing musuh terhadap tank kita
    public double getEnemyBearing(double enemyX, double enemyY) {
        double deltaX = enemyX - X;
        double deltaY = enemyY - Y;
        return Math.Atan2(deltaY, deltaX) + (Direction * (Math.PI/180));
    }
    
    // True jika jarak terlalu dekat dengan dinding
    public bool isNearWall() {
        return (X < WallMargin || X > Width - WallMargin || Y < WallMargin || Y > Height - WallMargin);
    }

    // Setting radar
    public void setRadarTurn(double enemyX, double enemyY) {
        double radarAngle = double.PositiveInfinity * NormalizeRelativeAngle(RadarBearingTo(enemyX, enemyY));
        if (!double.IsNaN(radarAngle) && (GunHeat < RADAR_LOCK || EnemyCount == 1))
        {
            SetTurnRadarLeft(radarAngle); // Lock radar ke musuh
        }
    }

    // Setting firepower
    public void setGunFire(double enemyX, double enemyY) {
        firePower = Energy / DistanceTo(enemyX, enemyY) * GUN_FACTOR;
        if (GunTurnRemaining == 0) {
            SetFire(firePower);
        }
    }

    // Cari jarak musuh
    public double getEnemyDistance(EnemyData enemyData) {
        return DistanceTo(enemyData.coordinate.X, enemyData.coordinate.Y);
    }

    // Prosedur untuk melakukan targeting secara linear 
    public void LinearTarget(double targetX, double targetY, double targetSpeed, double targetDirection, double firePower) {
        double vB = CalcBulletSpeed(firePower);
        double vXt = targetSpeed * Math.Cos(degToRad(targetDirection));
        double vYt = targetSpeed * Math.Sin(degToRad(targetDirection));
        double Xt = targetX;
        double Yt = targetY;

        // Persamaan kuadrat
        double a = Math.Pow(vXt, 2) + Math.Pow(vYt, 2) - Math.Pow(vB, 2);
        double b = 2 * (vXt * (Xt - X) + vYt * (Yt - Y));
        double c = Math.Pow(Xt - X, 2) + Math.Pow(Yt - Y, 2);
        double discriminant = Math.Pow(b, 2) - 4 * a * c;

        if (discriminant < 0) { // Langsung tembak saja ke arah musuh
            SetTurnGunLeft(GunBearingTo(targetX, targetY));
            SetFire(firePower);
        } else {
            double t1 = (-b + Math.Sqrt(discriminant)) / (2 * a);
            double t2 = (-b - Math.Sqrt(discriminant)) / (2 * a);
            double t = Math.Min(t1 > 0 ? t1 : double.PositiveInfinity, t2 > 0 ? t2 : double.PositiveInfinity);

            double predictedX = targetX + targetSpeed * t * Math.Cos(degToRad(targetDirection));
            double predictedY = targetY + targetSpeed * t * Math.Sin(degToRad(targetDirection));
            SetTurnGunLeft(GunBearingTo(predictedX, predictedY));
            SetFire(firePower);
        }
    }

    // Prosedur untuk menggerakkan tank ke tengah arena jika sudah dekat dinding
    public void moveToCenter(double vel = 8) {
        double turn = vel > 0 ? BearingTo(CenterX, CenterY) : 180 - BearingTo(CenterX, CenterY);
        vel = Math.Abs(vel);
        SetTurnLeft(turn);
        double turnRadius = Math.Abs((180 - Math.Abs(turn)) / 180 * vel / (TurnRate == 0 ? 1 : TurnRate));
        double distance = DistanceTo(CenterX, CenterY);

        if (Math.Abs(turn) < 30 && distance < WallMargin) {
            TargetSpeed = vel * distance / WallMargin;
        } else {
            TargetSpeed = Math.Abs(turn != 0 ? TurnRate * turnRadius : vel);
        }
    }

    // Prosedur untuk melakukan anti-gravity movement --> menjauhi musuh
    public void antiGravity() {
        double xForce = 0;
        double yForce = 0;

        double enemyAbsBearing = NormalizeAbsoluteAngle(radToDeg(Math.Atan2(scannedEnemyX - X, scannedEnemyY - Y)));
        double enemyDistance = getEnemyDistance(enemyData);
        xForce -= (Math.Sin(degToRad(enemyAbsBearing)) / Math.Pow(enemyDistance, 2)) * enemyData.enemyEnergy;
        yForce -= (Math.Cos(degToRad(enemyAbsBearing)) / Math.Pow(enemyDistance, 2)) * enemyData.enemyEnergy;

        double angle = NormalizeRelativeAngle(radToDeg(Math.Atan2(xForce, yForce)));

        if (Math.Abs(CalcBearing(angle)) < 90) {
            SetTurnRight(CalcBearing(angle));
            SetForward(100);
        } else {
            SetTurnRight(CalcBearing(angle) > 0 ? CalcBearing(angle) - 180 : CalcBearing(angle) + 180);
            Back(50);
        }
    }

    public double degToRad(double degree) {
        return degree * (Math.PI/180);
    }

    public double radToDeg(double radian) {
        return radian * (180/Math.PI);
    }
}

public struct EnemyData {
    
    public Point2D coordinate;
    public double enemyDirection;
    public double enemySpeed;
    public double enemyEnergy;

    public EnemyData(double X, double Y, double enemyDirection, double enemySpeed, double enemyEnergy) {
        this.coordinate = new Point2D(X, Y);
        this.enemyDirection = enemyDirection;
        this.enemySpeed = enemySpeed;
        this.enemyEnergy = enemyEnergy;
    }
    public EnemyData(Point2D coordinate, double enemyDirection, double enemySpeed, double enemyEnergy) {
        this.coordinate = coordinate;
        this.enemyDirection = enemyDirection;
        this.enemySpeed = enemySpeed;
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
