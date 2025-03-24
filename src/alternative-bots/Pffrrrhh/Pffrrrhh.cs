using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// Pffrrrhh
// ------------------------------------------------------------------
// Targetting: Linear
// Movement: High Risk High Return
// ------------------------------------------------------------------
public class Pffrrrhh : Bot
{   
    static void Main(string[] args)
    {
        new Pffrrrhh().Start();
    }

    Pffrrrhh() : base(BotInfo.FromFile("Pffrrrhh.json")) { }

    public override void Run()
    {
        AdjustRadarForBodyTurn = true;
        AdjustGunForBodyTurn = true;
        SetTurnRadarRight(double.PositiveInfinity);

        BodyColor = Color.FromArgb(150, 75, 0);
        TurretColor = Color.FromArgb(150, 75, 0);
        RadarColor = Color.FromArgb(150, 75, 0);
        BulletColor = Color.FromArgb(150, 75, 0);
        ScanColor = Color.FromArgb(150, 75, 0);
        TracksColor = Color.FromArgb(150, 75, 0);
        ScanColor = Color.FromArgb(150, 75, 0);
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        double radarAngle = double.PositiveInfinity * NormalizeRelativeAngle(RadarBearingTo(e.X, e.Y));
        if (!double.IsNaN(radarAngle))
            SetTurnRadarLeft(radarAngle);
        
        double firePower = Math.Min(3 * Energy / DistanceTo(e.X, e.Y), 0.1);
        if (GunTurnRemaining == 0)
        {
            SetFire(firePower);
        }

        LinearTargeting(e.X, e.Y, e.Speed, e.Direction, firePower);

        double risk = 0;
        double targetX = e.X;
        double targetY = e.Y;
        for (int i = 0; i < 360; i++)
        {
            double x = X + 100 * Math.Cos(DegreesToRadians(i));
            double y = Y + 100 * Math.Sin(DegreesToRadians(i));
            double tempRisk = e.Energy / (Math.Pow(x - e.X, 2) + Math.Pow(y - e.Y, 2) + 1e-6);

            if (tempRisk > risk)
            {
                risk = tempRisk;
                targetX = x;
                targetY = y;
            }
        }
        double turn = BearingTo(targetX, targetY) * Math.PI / 180;
        SetTurnLeft(Math.Tan(turn) * 180 / Math.PI);
        SetForward(DistanceTo(targetX, targetY) * Math.Cos(turn));
    }

    private void LinearTargeting(double targetX, double targetY, double targetSpeed, double targetDirection, double firePower) {
        double vb = CalcBulletSpeed(firePower);
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
        SetTurnGunLeft(angleToPredicted);
    }

    private double DegreesToRadians(double degrees) {
        return degrees * Math.PI / 180;
    }
}
