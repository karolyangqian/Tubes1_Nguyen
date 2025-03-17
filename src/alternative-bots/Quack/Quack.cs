using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// Quack
// ------------------------------------------------------------------
// A sample bot original made for Robocode by Mathew Nelson.
// Ported to Robocode Tank Royale by Flemming N. Larsen.
//
// Sits still. Tracks and fires at the nearest bot it sees.
// ------------------------------------------------------------------
public class Quack : Bot
{
    private double scannedEnemyX;
    private double scannedEnemyY;
    private double scannedEnemySpeed;
    private double scannedEnemyDirection;
    private bool enemyDetected;
    private const double minSpeed = 2;
    private const double maxSpeed = 10;
    private const double maxTurnRate = 15;
    private const double minTurnRate = 5;


    // The main method starts our bot
    static void Main(string[] args)
    {
        new Quack().Start();
    }

    // Constructor, which loads the bot config file
    Quack() : base(BotInfo.FromFile("Quack.json")) { }

    // Called when a new round is started -> initialize and do some movement
    public override void Run()
    {
        // Set colors
        var pink = Color.FromArgb(0xFF, 0x69, 0xB4);
        BodyColor = pink;
        TurretColor = pink;
        RadarColor = pink;
        ScanColor = pink;
        BulletColor = pink;
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        AdjustRadarForBodyTurn = true;
        enemyDetected = false;
        
        SetTurnRadarRight(double.PositiveInfinity);
        
        MaxSpeed = maxSpeed;
        MaxTurnRate = maxTurnRate;
        TargetSpeed = 5;
        TurnRate = 10;
    }

// ================================ DRIVERS ==================================


// ================================ EVENT HANDLERS ==================================

    public override void OnTick(TickEvent tickEvent)
    {
        if (enemyDetected) {
            TrackScanAt(scannedEnemyX, scannedEnemyY);
            // ShootAt(scannedEnemyX, scannedEnemyY, 1, 3);
            ShootPredict(scannedEnemyX, scannedEnemyY, scannedEnemySpeed, scannedEnemyDirection, CalculateFirePower(scannedEnemyX, scannedEnemyY));
            enemyDetected = false;
        } else {
            SetTurnRadarLeft(20);
        }
        Random rnd = new Random();
        double newSpeed = TargetSpeed + rnd.NextDouble()*2 * GetRandomSign();
        if (newSpeed > minSpeed && newSpeed < maxSpeed) {
            TargetSpeed = newSpeed;
        }

        double newTurnRate = TurnRate + rnd.NextDouble()*2 * GetRandomSign();
        if (newTurnRate > minTurnRate && newTurnRate < maxTurnRate) {
            TurnRate = newTurnRate;
        }
        // TargetSpeed =  rnd.NextDouble() * GetRandomSign();
        // TurnRate += rnd.NextDouble() * GetRandomSign();
        Console.WriteLine(string.Format("GunHeat: {0:0.00} Energy: {1:0.00}", GunHeat, Energy));
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        scannedEnemyX = e.X;
        scannedEnemyY = e.Y;
        scannedEnemySpeed = e.Speed;
        scannedEnemyDirection = e.Direction;
        enemyDetected = true;
        // double radarAngle = double.PositiveInfinity * NormalizeRelativeAngle(RadarBearingTo(e.X, e.Y));

        // if (!double.IsNaN(radarAngle) && (GunHeat < 1 || EnemyCount == 1)) 
        // {
        //     SetTurnRadarLeft(radarAngle);
        // }
    }

    // We won the round -> do a victory dance!
    public override void OnWonRound(WonRoundEvent e)
    {
        // Victory dance turning right 360 degrees 100 times
        TurnLeft(36_000);
    }

// ================================ FEATURES ==================================

    private void TrackScanAt(double x, double y) {
        var bearingFromRadar = NormalizeRelativeAngle(RadarBearingTo(x, y));
        SetTurnRadarLeft(bearingFromRadar + (bearingFromRadar > 0 ? 10 : -10));
        // bearingFromRadar = RadarBearingTo(x, y);
        // TurnRadarLeft(bearingFromRadar - 10);
    }

    private void TrackScanWithGunAt(double x, double y) {
        AdjustRadarForGunTurn = false;
        var bearingFromGun = GunBearingTo(x, y);
        TurnGunLeft(bearingFromGun + 10);
        bearingFromGun = RadarBearingTo(x, y);
        TurnGunLeft(bearingFromGun - 10);
    }

    private void AimGunAt(double x, double y, double degreesMaxOffset) {
        var bearingFromGun = GunBearingTo(x, y);
        var rnd = new Random();
        TurnGunLeft(bearingFromGun + GetRandomSign()*rnd.NextDouble()*degreesMaxOffset);
    }

    private void ShootAt(double x, double y, double power, double degreesMaxOffset) {
        AimGunAt(x, y, degreesMaxOffset);
        Fire(power);
    }

    private void ShootPredict(double targetX, double targetY, double targetSpeed, double targetDirection, double firePower) {
        double bulletSpeed = CalcBulletSpeed(firePower);

        // double absBearing = Math.Atan2(dy, dx);
        
        double enemyDir = targetDirection * Math.PI / 180.0;
        
        // double ratio = Math.Max(-1, Math.Min(1, (scannedEnemySpeed * Math.Sin(enemyDir - absBearing)) / bulletSpeed));
        // double leadAngle = Math.Asin(ratio);
        
        // double gunDirection = absBearing + leadAngle;
        
        double time = DistanceTo(targetX, targetY) / bulletSpeed;
        
        double predictedX = targetX + targetSpeed * time * Math.Cos(enemyDir);
        double predictedY = targetY + targetSpeed * time * Math.Sin(enemyDir);
        
        double bearingFromGun = GunBearingTo(predictedX, predictedY);
        
        SetFire(firePower);
        SetTurnGunLeft(bearingFromGun);
    }

    private double CalculateFirePower(double targetX, double targetY) {
        double MAX_POWER_RADIUS = Math.Sqrt(ArenaHeight * ArenaHeight + ArenaWidth * ArenaWidth) * 0.3;
        double MIN_POWER_RADIUS = MAX_POWER_RADIUS * 0.2;
        double MAX_POWER = 3;
        double MIN_POWER = 0.5;

        double dist = DistanceTo(targetX, targetY);
        double val = dist > MAX_POWER_RADIUS ? MAX_POWER_RADIUS : dist < MIN_POWER_RADIUS ? MIN_POWER_RADIUS : dist;
        double factor = (MAX_POWER_RADIUS - MIN_POWER_RADIUS - (val - MIN_POWER_RADIUS)) / (MAX_POWER_RADIUS - MIN_POWER_RADIUS);
        return MIN_POWER + factor * (MAX_POWER - MIN_POWER);
    }

// ================================ UTILS ==================================

    private int GetRandomSign() {
        var rnd = new Random();
        return rnd.Next(0, 2) == 0 ? -1 : 1;
    }
}