using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Schtinky : Bot
{   
    private const double MAX_SHOOT_RANGE_THRESH = 600;
    private double scannedEnemyX;
    private double scannedEnemyY;
    private double scannedEnemySpeed;
    private double scannedEnemyDirection;
    private bool enemyDetected = false;
    private bool enemyEnergyDrop = false;
    private double scannedPrevEnergy = 100;
    private double scannedCurrEnergy = 0;
    private bool isGoingLeft = false;
    private double firePower = 0;

    static void Main(string[] args)
    {
        new Schtinky().Start();
    }

    Schtinky() : base(BotInfo.FromFile("Schtinky.json")) { }

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


        while (IsRunning)
        {
            if (enemyDetected) {
                firePower = CalculateFirePower(scannedEnemyX, scannedEnemyY);
                TrackScanAt(scannedEnemyX, scannedEnemyY);
                ShootPredict(scannedEnemyX, scannedEnemyY, scannedEnemySpeed, scannedEnemyDirection, firePower);
                enemyDetected = false;
            } else {
                TurnRadarLeft(20);
            }
            if (enemyEnergyDrop) {
                // Movement dodging
                isGoingLeft = false;
                double angleEnemy = angleTo(scannedEnemyX, scannedEnemyY) - 180;
                double turnAngle = ReactEnemyShoot(this.X, this.Y, angleEnemy, this.Direction);

                if (turnAngle > 90 || turnAngle < -90) {
                    if (turnAngle > 90) {
                        isGoingLeft = true;
                        SetTurnLeft(180 - turnAngle);
                        SetTurnRadarLeft(10);
                    } else {
                        SetTurnRight(180 + turnAngle);
                        SetTurnRadarRight(10);
                    }
                    SetBack(100);
                } else {
                    if (turnAngle < 0) {
                        isGoingLeft = true;
                        SetTurnRadarRight(10);
                    } else SetTurnRadarLeft(10);
                    SetTurnRight(turnAngle);
                    SetForward(100);
                }
                enemyEnergyDrop = false;
            } 
            // else {
            //     if (!isGoingLeft) {
            //         SetTurnLeft(3);
            //         SetForward(50);
            //         isGoingLeft = true;
            //     } else {
            //         SetTurnRight(3);
            //         SetForward(50);
            //         isGoingLeft = false;
            //     }
            // }
        }
    }

/* ================================= Event Handler ================================= */

    public override void OnScannedBot(ScannedBotEvent e)
    {
        scannedEnemyX = e.X;
        scannedEnemyY = e.Y;
        scannedCurrEnergy = e.Energy;
        scannedEnemySpeed = e.Speed;
        scannedEnemyDirection = e.Direction;
        enemyDetected = true;
        if (scannedCurrEnergy < scannedPrevEnergy) {
            enemyEnergyDrop = true;
        }
        scannedPrevEnergy = scannedCurrEnergy;
    }

    public override void OnHitWall(HitWallEvent botHitWallEvent)
    {
        SetBack(100);
        SetTurnRight(90);
    }

    // public override void OnHitBot(HitBotEvent botHitBotEvent)
    // {
    //     if (botHitBotEvent.IsRammed) {
    //         SetBack(100);
    //         SetTurnRight(90);
    //     }
    // }

    /* ================================= Methods ================================= */

    /*
        Params:
        - coordX: koordinat X robot ini
        - coordY: koordinat Y robot ini
        - enemyHeading: arah musuh dalam derajat
        - botHeading: arah robot ini dalam derajat
     */
    private double ReactEnemyShoot(double coordX, double coordY, double enemyHeading, double botHeading) {
        double enemyDistance = GetEnemyDistance(scannedEnemyX, scannedEnemyY);
        double enemyBulletSpeed = CalcBulletSpeed(scannedPrevEnergy - scannedCurrEnergy);
        double enemyBulletTime = enemyDistance / enemyBulletSpeed;

        // enemyHeading harusnya dalam radian, arah musuh dalam derajat, bukan arah radar 
        double enemyDirection = enemyHeading * Math.PI / 180.0; // konversi ke radian

        // Prediksi peluru musuh
        double predictedBulletX = scannedEnemyX + Math.Sin(enemyDirection) * enemyBulletTime * enemyBulletSpeed;
        double predictedBulletY = scannedEnemyY + Math.Cos(enemyDirection) * enemyBulletTime * enemyBulletSpeed;

        double safeFromEnemyBullet = (enemyHeading + 90) * Math.PI / 180.0; // tegak lurus terhadap arah peluru
        double safeX = coordX + Math.Sin(safeFromEnemyBullet) * 100;
        double safeY = coordY + Math.Cos(safeFromEnemyBullet) * 100;

        double dX = safeX - coordX;
        double dY = safeY - coordY;
        double angle = Math.Atan2(dY, dX);

        double turnAngle = NormalizeBearing((angle * (180.0 / Math.PI)) - botHeading);

        return turnAngle;
    }

    private void TrackScanAt(double x, double y) {
        var bearingFromRadar = RadarBearingTo(x, y);
        TurnRadarLeft(bearingFromRadar + 10);
        bearingFromRadar = RadarBearingTo(x, y);
        TurnRadarLeft(bearingFromRadar - 10);
    }
    

    private void ShootPredict(double targetX, double targetY, double targetSpeed, double targetDirection, double firePower) {
        double bulletSpeed = CalcBulletSpeed(firePower);
        
        double dx = targetX - X;
        double dy = targetY - Y;
        // double absBearing = Math.Atan2(dy, dx);
        
        double enemyDir = targetDirection * Math.PI / 180.0; // convert to radians
        
        // double ratio = Math.Max(-1, Math.Min(1, (scannedEnemySpeed * Math.Sin(enemyDir - absBearing)) / bulletSpeed));
        // double leadAngle = Math.Asin(ratio);
        
        // double gunDirection = absBearing + leadAngle;
        
        double distance = Math.Sqrt(dx * dx + dy * dy);
        double time = distance / bulletSpeed;
        
        double predictedX = targetX + targetSpeed * time * Math.Cos(enemyDir);
        double predictedY = targetY + targetSpeed * time * Math.Sin(enemyDir);
        
        double bearingFromGun = GunBearingTo(predictedX, predictedY);
        
        TurnGunLeft(bearingFromGun);
        Fire(firePower);
    }

    private double CalculateFirePower(double targetX, double targetY) {
        double dist = DistanceTo(targetX, targetY);
        dist = dist > MAX_SHOOT_RANGE_THRESH ? MAX_SHOOT_RANGE_THRESH : dist;
        double val = (MAX_SHOOT_RANGE_THRESH - dist) / MAX_SHOOT_RANGE_THRESH;
        return 1 + 2 * val;
    }

    
/* ================================= Utils ================================= */
    // normalizes a bearing to between +180 and -180
    private double NormalizeBearing(double angle) {
        while (angle >  180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }

    private double angleTo(double X, double Y) {
        double dX = X - this.X;
        double dY = Y - this.Y;

        double angleRadian = Math.Atan2(dX, dY);
        double angleDegree = angleRadian * 180 / Math.PI;

        return NormalizeBearing(angleDegree);
    }

    private double DegreesToRadians(double degrees) {
        return degrees * (Math.PI / 180);
    } 

    private int GetRandomSign() {
        var rnd = new Random();
        return rnd.Next(0, 2) == 0 ? -1 : 1;
    }

    private double GetEnemyDistance(double enemyX, double enemyY) {
        return DistanceTo(enemyX, enemyY);
    }

    private double GetEnemyBulletSpeed(double firePower) {
        return 20 - 3 * firePower;
    }
    
}
