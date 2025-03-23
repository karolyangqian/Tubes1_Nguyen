using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// diem
// ------------------------------------------------------------------
// Targetting: None
// Movement: None
// ------------------------------------------------------------------
public class Diem : Bot
{   
    private Random random = new Random();
    
    static void Main(string[] args)
    {
        new Diem().Start();
    }

    Diem() : base(BotInfo.FromFile("DiemL.json")) { }

    public override void Run()
    {
        AdjustRadarForBodyTurn = true;
        AdjustGunForBodyTurn = true;
        SetTurnRadarRight(double.PositiveInfinity);
    }

    public override void OnTick(TickEvent e)
    {
        BodyColor = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
        TurretColor = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
        RadarColor = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
        BulletColor = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
        ScanColor = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        double radarAngle = double.PositiveInfinity * NormalizeRelativeAngle(RadarBearingTo(e.X, e.Y));
        if (!double.IsNaN(radarAngle))
            SetTurnRadarLeft(radarAngle);
        
        double firePower = 2;
        if (GunTurnRemaining == 0)
        {
            SetFire(firePower);
        }

        // Head on
        // TurnGunLeft(GunBearingTo(e.X, e.Y));

        // Linear
        // double bulletSpeed = CalcBulletSpeed(firePower);
        
        // double dx = e.X - X;
        // double dy = e.Y - Y;
        
        // double enemyDir = e.Direction * Math.PI / 180.0;
        
        // double distance = Math.Sqrt(dx * dx + dy * dy);
        // double time = distance / bulletSpeed;
        
        // double predictedX = e.X + e.Speed * time * Math.Cos(enemyDir);
        // double predictedY = e.Y + e.Speed * time * Math.Sin(enemyDir);

        
        // double vb = CalcBulletSpeed(firePower);
        // double dir = e.Direction * Math.PI / 180;
        // double vxt = e.Speed * Math.Cos(dir);
        // double vyt = e.Speed * Math.Sin(dir);
        // double xt = e.X;
        // double yt = e.Y;
        // double a = Math.Pow(vxt, 2) + Math.Pow(vyt, 2) - Math.Pow(vb, 2);
        // double b = 2 * (vxt * (xt - X) + vyt * (yt - Y));
        // double c = Math.Pow(xt - X, 2) + Math.Pow(yt - Y, 2);
        // double d = Math.Pow(b, 2) - 4 * a * c;
        // double t1 = (-b + Math.Sqrt(d)) / (2 * a);
        // double t2 = (-b - Math.Sqrt(d)) / (2 * a);
        // double t = Math.Max(t1, t2);
        // double predictedX = e.X + vxt * t;
        // double predictedY = e.Y + vyt * t;

        // var g = Graphics;
        // g.FillRectangle(Brushes.Red, (float)predictedX, (float)predictedY, 10, 10);

        // TurnGunLeft(GunBearingTo(predictedX, predictedY));

        LinearTargeting(e.X, e.Y, e.Speed, e.Direction, firePower);
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
        double t1 = (-b + Math.Sqrt(d)) / (2 * a);
        double t2 = (-b - Math.Sqrt(d)) / (2 * a);
        double time = Math.Min(t1 > 0 ? t1 : double.PositiveInfinity, t2 > 0 ? t2 : double.PositiveInfinity);

        double predictedX = targetX + targetSpeed * time * Math.Cos(DegreesToRadians(targetDirection));
        double predictedY = targetY + targetSpeed * time * Math.Sin(DegreesToRadians(targetDirection));

        predictedX = Math.Max(0, Math.Min(ArenaWidth, predictedX));
        predictedY = Math.Max(0, Math.Min(ArenaHeight, predictedY));

        var g = Graphics;
        Pen skyBluePen = new Pen(Brushes.Red);
        g.DrawRectangle(skyBluePen, (float)predictedX, (float)predictedY, 20, 20);
        
        double angleToPredicted = GunBearingTo(predictedX, predictedY);
        // double turn = angleToPredicted > angleToEnemy ? angleToPredicted - 2 : angleToPredicted + 2;
        SetTurnGunLeft(angleToPredicted);
        // SetFire(firePower);
        // Console.WriteLine(string.Format("Predicted: {0:0.00} {1:0.00} t1: {2:0.00} t2: {3:0.00}", predictedX, predictedY, t1, t2));
    }

    private double DegreesToRadians(double degrees) {
        return degrees * Math.PI / 180;
    }
}
