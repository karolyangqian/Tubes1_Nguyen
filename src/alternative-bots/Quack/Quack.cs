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
    private const double MAX_SHOOT_RANGE_THRESH = 600;
    private double scannedEnemyX;
    private double scannedEnemyY;
    private double scannedEnemySpeed;
    private double scannedEnemyDirection;
    private bool enemyDetected;


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

        QuackDriverV1();
    }

// ================================ DRIVERS ==================================

    public void QuackDriverV1() {
        while (IsRunning)
        {
            if (enemyDetected) {
                TrackScanAt(scannedEnemyX, scannedEnemyY);
                // ShootAt(scannedEnemyX, scannedEnemyY, 1, 3);
                ShootPredict(scannedEnemyX, scannedEnemyY, scannedEnemySpeed, scannedEnemyDirection, CalculateFirePower(scannedEnemyX, scannedEnemyY));
                enemyDetected = false;
            } else {
                TurnRadarLeft(20);
            }
            SetTurnLeft(10);
            // Limit our speed to 5
            MaxSpeed = 5;
            // Start moving (and turning)
            SetForward(10);
        }
    }

// ================================ EVENT HANDLERS ==================================

    // We scanned another bot -> we have a target, so go get it
    // public override void OnScannedBot(ScannedBotEvent e)
    // {
    //     // Calculate direction of the scanned bot and bearing to it for the gun
    //     var bearingFromGun = GunBearingTo(e.X, e.Y);

    //     // Turn the gun toward the scanned bot
    //     TurnGunLeft(bearingFromGun);

    //     // If it is close enough, fire!
    //     if (Math.Abs(bearingFromGun) <= 3 && GunHeat == 0)
    //         Fire(Math.Min(3 - Math.Abs(bearingFromGun), Energy - .1));

    //     // Generates another scan event if we see a bot.
    //     // We only need to call this if the gun (and therefore radar)
    //     // are not turning. Otherwise, scan is called automatically.
    //     if (bearingFromGun == 0)
    //         Rescan();
    // }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        scannedEnemyX = e.X;
        scannedEnemyY = e.Y;
        scannedEnemySpeed = e.Speed;
        scannedEnemyDirection = e.Direction;
        enemyDetected = true;
    }

    // We won the round -> do a victory dance!
    public override void OnWonRound(WonRoundEvent e)
    {
        // Victory dance turning right 360 degrees 100 times
        TurnLeft(36_000);
    }

// ================================ FEATURES ==================================

    private void TrackScanAt(double x, double y) {
        var bearingFromRadar = RadarBearingTo(x, y);
        TurnRadarLeft(bearingFromRadar + 10);
        bearingFromRadar = RadarBearingTo(x, y);
        TurnRadarLeft(bearingFromRadar - 10);
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
        
        double dx = targetX - X;
        double dy = targetY - Y;
        // double absBearing = Math.Atan2(dy, dx);
        
        double enemyDir = targetDirection * Math.PI / 180.0;
        
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

// ================================ UTILS ==================================

    // normalizes a bearing to between +180 and -180
    private double NormalizeBearing(double angle) {
        while (angle >  180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }
    private double DegreesToRadians(double degrees) {
        return degrees * (Math.PI / 180);
    } 

    private int GetRandomSign() {
        var rnd = new Random();
        return rnd.Next(0, 2) == 0 ? -1 : 1;
    }
}